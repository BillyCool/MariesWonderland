using MariesWonderland.Configuration;
using MariesWonderland.Data;
using MariesWonderland.Helpers;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Quest;
using Microsoft.Extensions.Options;

namespace MariesWonderland.Quests;

/// <summary>
/// Quest rules shared by every quest type: quest and mission rows, first-clear and mission rewards,
/// big wins, drops, EXP and gold. Each quest type's service handles its own progress tables and flow,
/// and calls in here for the rest.
/// </summary>
public sealed class QuestEngine(DarkMasterMemoryDatabase masterDb, GameConfig gameConfig, QuestCampaignHelper questCampaigns, IOptions<DevModeOptions> devMode)
{
    private readonly DarkMasterMemoryDatabase _masterDb = masterDb;
    private readonly GameConfig _gameConfig = gameConfig;
    private readonly QuestCampaignHelper _questCampaigns = questCampaigns;
    private readonly DevModeOptions _devMode = devMode.Value;

    public EntityMQuest? FindQuest(int questId)
        => _masterDb.EntityMQuest.FirstOrDefault(q => q.QuestId == questId);

    /// <summary>Gets or creates the user's quest row, and creates any missing rows for the quest's missions.</summary>
    public EntityIUserQuest EnsureQuestRows(DarkUserMemoryDatabase userDb, int questId)
    {
        EntityIUserQuest userQuest = GetOrCreateUserQuest(userDb, questId);

        EntityMQuest? quest = FindQuest(questId);
        if (quest == null)
        {
            return userQuest;
        }

        foreach (int missionId in MissionIds(quest))
        {
            if (!userDb.EntityIUserQuestMission.Any(m => m.QuestId == questId && m.QuestMissionId == missionId))
            {
                userDb.EntityIUserQuestMission.Add(new EntityIUserQuestMission { UserId = userDb.UserId, QuestId = questId, QuestMissionId = missionId });
            }
        }

        return userQuest;
    }

    public static EntityIUserQuest GetOrCreateUserQuest(DarkUserMemoryDatabase userDb, int questId)
        => userDb.EntityIUserQuest.GetOrCreate(
            q => q.QuestId == questId,
            () => new EntityIUserQuest { UserId = userDb.UserId, QuestId = questId });

    /// <summary>
    /// Spends the stamina for <paramref name="times"/> runs of the quest (after stamina campaigns), unless
    /// <see cref="DevModeOptions.FreeStamina"/> is on.
    /// </summary>
    public void ConsumeStamina(DarkUserMemoryDatabase userDb, EntityMQuest? quest, QuestCampaignTarget campaignTarget, int times, long nowMs)
    {
        if (_devMode.IsFreeStaminaEnabled || quest == null || quest.Stamina <= 0 || times <= 0)
        {
            return;
        }

        EntityIUserStatus status = userDb.EntityIUserStatus[0];
        int cost = _questCampaigns.ApplyStaminaCampaign(quest.Stamina, campaignTarget, nowMs) * times;
        int maxStamina = _masterDb.EntityMUserLevel.Find(status.Level)?.MaxStamina ?? 0;
        StaminaHelper.Consume(status, cost, maxStamina, _gameConfig.UserStaminaRecoverySecond, nowMs);
    }

    /// <summary>
    /// True when the quest is actually played rather than auto-cleared on start. A background quest still
    /// counts when it has a battle (non-zero recommended deck power); pure cutscenes have none.
    /// </summary>
    public static bool IsPlayable(EntityMQuest? quest)
    {
        if (quest == null)
        {
            return false;
        }

        return quest.IsRunInTheBackground ? quest.RecommendedDeckPower > 0 : quest.IsCountedAsQuest;
    }

    public static void MarkActive(EntityIUserQuest userQuest, long nowMs)
    {
        userQuest.QuestStateType = (int)QuestStateType.ACTIVE;
        userQuest.LatestStartDatetime = nowMs;
    }

    public static void MarkCleared(EntityIUserQuest userQuest, int clearCount, long nowMs)
    {
        userQuest.QuestStateType = (int)QuestStateType.CLEARED;
        userQuest.ClearCount += clearCount;
        userQuest.DailyClearCount += clearCount;
        userQuest.LastClearDatetime = nowMs;
    }

    /// <summary>Resets the progress of the quest's missions so a restarted run evaluates them again.</summary>
    public static void ResetMissionProgress(DarkUserMemoryDatabase userDb, int questId)
    {
        foreach (EntityIUserQuestMission mission in userDb.EntityIUserQuestMission.Where(m => m.QuestId == questId))
        {
            mission.IsClear = false;
            mission.ProgressValue = 0;
            mission.LatestClearDatetime = 0;
        }
    }

    /// <summary>
    /// Finishes a run. A win grants EXP and gold, first-clear rewards (first clear only), the rewards of
    /// missions not yet cleared plus the big win when this run completes them, and drops; then marks the
    /// quest and its missions cleared. A loss (retired or annihilated) grants nothing and leaves the
    /// missions as they were; a quest cleared before goes back to <see cref="QuestStateType.CLEARED"/>.
    /// A replay win grants only EXP, gold and drops: replay quests have no first-clear or mission rewards
    /// and keep no mission rows.
    /// </summary>
    public QuestFinishResult Finish(DarkUserMemoryDatabase userDb, EntityMQuest quest, QuestCampaignTarget campaignTarget, bool isWin, long nowMs, bool isReplay = false)
    {
        EntityIUserQuest userQuest = GetOrCreateUserQuest(userDb, quest.QuestId);

        if (!isWin)
        {
            if (userQuest.ClearCount > 0 && userQuest.QuestStateType == (int)QuestStateType.ACTIVE)
            {
                userQuest.QuestStateType = (int)QuestStateType.CLEARED;
            }

            return new QuestFinishResult();
        }

        bool isFirstClear = userQuest.ClearCount == 0 && !isReplay;

        List<EntityMQuestFirstClearRewardGroup> firstClearRows = isFirstClear ? ResolveFirstClearRewardRows(userDb, quest) : [];
        List<QuestReward> missionClearRewards = [];
        List<QuestReward> bigWinRewards = [];
        int regularMissionCount = 0;
        int bigWinMissionId = 0;

        IEnumerable<int> missionIds = isReplay ? [] : MissionIds(quest);
        foreach (int missionId in missionIds)
        {
            EntityMQuestMission? mission = _masterDb.EntityMQuestMission.FirstOrDefault(m => m.QuestMissionId == missionId);
            if (mission == null)
            {
                continue;
            }

            if (mission.QuestMissionConditionType == QuestMissionConditionType.COMPLETE)
            {
                bigWinMissionId = mission.QuestMissionId;
                continue;
            }

            regularMissionCount++;
            if (!IsMissionCleared(userDb, quest.QuestId, mission.QuestMissionId))
            {
                AddMissionReward(missionClearRewards, mission);
            }
        }

        bool isBigWin = false;
        if (regularMissionCount > 0 && bigWinMissionId != 0 && !IsMissionCleared(userDb, quest.QuestId, bigWinMissionId))
        {
            isBigWin = true;
            EntityMQuestMission? bigWinMission = _masterDb.EntityMQuestMission.FirstOrDefault(m => m.QuestMissionId == bigWinMissionId);
            if (bigWinMission != null)
            {
                AddMissionReward(bigWinRewards, bigWinMission);
            }
        }

        ApplyExpRewards(userDb, quest, nowMs);
        ApplyGoldReward(userDb, quest, nowMs);

        if (isFirstClear)
        {
            foreach (EntityMQuestFirstClearRewardGroup row in firstClearRows)
            {
                PossessionHelper.Apply(userDb, row.PossessionType, row.PossessionId, row.Count, _masterDb, nowMs);
            }

            ApplyWeaponStoryUnlocks(userDb, quest.QuestId);
        }

        // Only missions not cleared yet are listed, so each mission's reward is granted once.
        foreach (QuestReward reward in missionClearRewards.Concat(bigWinRewards))
        {
            PossessionHelper.Apply(userDb, (PossessionType)reward.PossessionType, reward.PossessionId, reward.Count, _masterDb, nowMs);
        }

        List<QuestReward> dropRewards = ApplyDropRewards(userDb, BuildDropRewards(quest, campaignTarget, nowMs), nowMs);
        MarkCleared(userQuest, 1, nowMs);
        if (!isReplay)
        {
            ClearQuestMissions(userDb, quest.QuestId, nowMs);
        }

        return new QuestFinishResult
        {
            DropRewards = dropRewards,
            FirstClearRewards = [.. firstClearRows.Select(r => ToReward(r.PossessionType, r.PossessionId, r.Count))],
            MissionClearRewards = missionClearRewards,
            MissionClearCompleteRewards = bigWinRewards,
            IsBigWin = isBigWin,
            BigWinMissionId = isBigWin ? bigWinMissionId : 0
        };
    }

    /// <summary>
    /// Clears a background quest the story moved past without a finish call: EXP, gold and (on the first
    /// clear) first-clear items, then the quest and its missions are marked cleared. No drops or mission rewards.
    /// </summary>
    public void FinalizePassedQuest(DarkUserMemoryDatabase userDb, EntityMQuest quest, long nowMs)
    {
        EntityIUserQuest userQuest = EnsureQuestRows(userDb, quest.QuestId);

        ApplyExpRewards(userDb, quest, nowMs);
        ApplyGoldReward(userDb, quest, nowMs);
        if (userQuest.ClearCount == 0)
        {
            foreach (EntityMQuestFirstClearRewardGroup row in ResolveFirstClearRewardRows(userDb, quest))
            {
                PossessionHelper.Apply(userDb, row.PossessionType, row.PossessionId, row.Count, _masterDb, nowMs);
            }
        }

        MarkCleared(userQuest, 1, nowMs);
        ClearQuestMissions(userDb, quest.QuestId, nowMs);
    }

    /// <summary>Grants one skipped clear: drops, gold and EXP. Returns the drops as granted.</summary>
    public List<QuestReward> ApplySkippedClear(DarkUserMemoryDatabase userDb, EntityMQuest quest, QuestCampaignTarget campaignTarget, long nowMs)
    {
        List<QuestReward> drops = ApplyDropRewards(userDb, BuildDropRewards(quest, campaignTarget, nowMs), nowMs);
        ApplyGoldReward(userDb, quest, nowMs);
        ApplyExpRewards(userDb, quest, nowMs);
        return drops;
    }

    /// <summary>
    /// The first-clear reward rows for the quest. A reward switch picks an alternative group once its
    /// condition quest is cleared.
    /// </summary>
    public List<EntityMQuestFirstClearRewardGroup> ResolveFirstClearRewardRows(DarkUserMemoryDatabase userDb, EntityMQuest quest)
    {
        if (quest.QuestFirstClearRewardGroupId == 0)
        {
            return [];
        }

        int rewardGroupId = quest.QuestFirstClearRewardGroupId;
        EntityMQuestFirstClearRewardSwitch? switchRow = _masterDb.EntityMQuestFirstClearRewardSwitch.FirstOrDefault(s => s.QuestId == quest.QuestId);
        if (switchRow != null)
        {
            EntityIUserQuest? conditionQuest = userDb.EntityIUserQuest.FirstOrDefault(q => q.QuestId == switchRow.SwitchConditionClearQuestId);
            if (conditionQuest != null && conditionQuest.QuestStateType == (int)QuestStateType.CLEARED)
            {
                rewardGroupId = switchRow.QuestFirstClearRewardGroupId;
            }
        }

        return [.. _masterDb.EntityMQuestFirstClearRewardGroup.Where(r => r.QuestFirstClearRewardGroupId == rewardGroupId)];
    }

    /// <summary>The quest's pickup drops with any active drop campaign applied. Nothing is granted.</summary>
    public List<QuestReward> BuildDropRewards(EntityMQuest quest, QuestCampaignTarget campaignTarget, long nowMs)
    {
        if (quest.QuestPickupRewardGroupId == 0)
        {
            return [];
        }

        List<QuestReward> rewards = [];
        foreach (EntityMQuestPickupRewardGroup pickup in _masterDb.EntityMQuestPickupRewardGroup
            .Where(p => p.QuestPickupRewardGroupId == quest.QuestPickupRewardGroupId))
        {
            EntityMBattleDropReward? drop = _masterDb.EntityMBattleDropReward.FirstOrDefault(d => d.BattleDropRewardId == pickup.BattleDropRewardId);
            if (drop != null)
            {
                rewards.Add(ToReward(drop.PossessionType, drop.PossessionId, drop.Count));
            }
        }

        return _questCampaigns.ApplyDropCampaignBonuses(rewards, campaignTarget, nowMs);
    }

    /// <summary>Grants drop rewards, auto-selling memoirs that match the user's auto-sale settings.</summary>
    public List<QuestReward> ApplyDropRewards(DarkUserMemoryDatabase userDb, List<QuestReward> rewards, long nowMs)
    {
        ParseAutoSaleRules(userDb, out HashSet<int> raritySet, out HashSet<int> rankSet);
        List<QuestReward> appliedRewards = [];

        foreach (QuestReward reward in rewards)
        {
            PossessionType possessionType = (PossessionType)reward.PossessionType;
            if (possessionType == PossessionType.PARTS || possessionType == PossessionType.PARTS_ENHANCED)
            {
                int grantCount = Math.Max(reward.Count, 1);
                for (int i = 0; i < grantCount; i++)
                {
                    (int grantedPartsId, bool isAutoSold) = PossessionHelper.GrantOrSellPartsDrop(
                        userDb, reward.PossessionId, _masterDb, _gameConfig.ConsumableItemIdForGold, raritySet, rankSet, nowMs);

                    appliedRewards.Add(new QuestReward
                    {
                        PossessionType = reward.PossessionType,
                        PossessionId = grantedPartsId,
                        Count = 1,
                        RewardEffectId = reward.RewardEffectId,
                        IsAutoSale = isAutoSold
                    });
                }

                continue;
            }

            PossessionHelper.Apply(userDb, possessionType, reward.PossessionId, reward.Count, _masterDb, nowMs);
            appliedRewards.Add(reward.Clone());
        }

        return appliedRewards;
    }

    /// <summary>Marks all of the quest's missions cleared, creating any missing rows.</summary>
    public void ClearQuestMissions(DarkUserMemoryDatabase userDb, int questId, long nowMs)
    {
        EntityMQuest? quest = FindQuest(questId);
        if (quest == null)
        {
            return;
        }

        foreach (int missionId in MissionIds(quest))
        {
            EntityIUserQuestMission userMission = userDb.EntityIUserQuestMission.GetOrCreate(
                m => m.QuestId == quest.QuestId && m.QuestMissionId == missionId,
                () => new EntityIUserQuestMission { UserId = userDb.UserId, QuestId = quest.QuestId, QuestMissionId = missionId });
            userMission.IsClear = true;
            userMission.ProgressValue = 1;
            userMission.LatestClearDatetime = nowMs;
        }
    }

    /// <summary>The quest's first scene by sort order, or its battle-only target scene when asked for and present.</summary>
    public int GetQuestStartSceneId(int questId, bool preferBattleOnlyTarget = false)
    {
        IEnumerable<EntityMQuestScene> questScenes = _masterDb.EntityMQuestScene.Where(s => s.QuestId == questId);
        EntityMQuestScene? startScene = preferBattleOnlyTarget
            ? questScenes.FirstOrDefault(s => s.IsBattleOnlyTarget)
            : null;
        startScene ??= questScenes.OrderBy(s => s.SortOrder).FirstOrDefault();
        return startScene?.QuestSceneId ?? 0;
    }

    /// <summary>
    /// Builds the BattleDropReward list for a quest by following the master data chain:
    /// QuestScene -> QuestSceneBattle -> BattleGroup -> Battle -> BattleNpcDeck -> BattleNpcDeckCharacterDropCategory.
    /// Returns an empty list if master data is missing. BattleDropEffectId is always 1.
    /// </summary>
    public List<BattleDropReward> BuildBattleDropRewards(int questId)
    {
        HashSet<int> sceneIds = [.. _masterDb.EntityMQuestScene
            .Where(s => s.QuestId == questId)
            .Select(s => s.QuestSceneId)];

        if (sceneIds.Count == 0)
        {
            return [];
        }

        Dictionary<int, int> battleGroupByScene = [];
        foreach (EntityMQuestSceneBattle sceneBattle in _masterDb.EntityMQuestSceneBattle)
        {
            if (sceneIds.Contains(sceneBattle.QuestSceneId))
            {
                battleGroupByScene[sceneBattle.QuestSceneId] = sceneBattle.BattleGroupId;
            }
        }

        HashSet<(int SceneId, int CategoryId)> seen = [];
        List<BattleDropReward> drops = [];

        foreach (int sceneId in sceneIds)
        {
            if (!battleGroupByScene.TryGetValue(sceneId, out int groupId))
            {
                continue;
            }

            foreach (EntityMBattleGroup battleGroup in _masterDb.EntityMBattleGroup.Where(bg => bg.BattleGroupId == groupId))
            {
                EntityMBattle? battle = _masterDb.EntityMBattle.FirstOrDefault(b => b.BattleId == battleGroup.BattleId);
                if (battle == null)
                {
                    continue;
                }

                EntityMBattleNpcDeck? deck = _masterDb.EntityMBattleNpcDeck.FirstOrDefault(d =>
                    d.BattleNpcId == battle.BattleNpcId &&
                    d.DeckType == battle.DeckType &&
                    d.BattleNpcDeckNumber == battle.BattleNpcDeckNumber);
                if (deck == null)
                {
                    continue;
                }

                foreach (string uuid in (string[])[deck.BattleNpcDeckCharacterUuid01, deck.BattleNpcDeckCharacterUuid02, deck.BattleNpcDeckCharacterUuid03])
                {
                    if (string.IsNullOrEmpty(uuid))
                    {
                        continue;
                    }

                    EntityMBattleNpcDeckCharacterDropCategory? dropCategory = _masterDb.EntityMBattleNpcDeckCharacterDropCategory
                        .FirstOrDefault(dc => dc.BattleNpcId == battle.BattleNpcId && dc.BattleNpcDeckCharacterUuid == uuid);
                    if (dropCategory == null)
                    {
                        continue;
                    }

                    if (seen.Add((sceneId, dropCategory.BattleDropCategoryId)))
                    {
                        drops.Add(new BattleDropReward
                        {
                            QuestSceneId = sceneId,
                            BattleDropCategoryId = dropCategory.BattleDropCategoryId,
                            BattleDropEffectId = 1
                        });
                    }
                }
            }
        }

        return drops;
    }

    private IEnumerable<int> MissionIds(EntityMQuest quest)
    {
        if (quest.QuestMissionGroupId == 0)
        {
            return [];
        }

        return [.. _masterDb.EntityMQuestMissionGroup
            .Where(g => g.QuestMissionGroupId == quest.QuestMissionGroupId)
            .Select(g => g.QuestMissionId)];
    }

    private static bool IsMissionCleared(DarkUserMemoryDatabase userDb, int questId, int missionId)
        => userDb.EntityIUserQuestMission.FirstOrDefault(m => m.QuestId == questId && m.QuestMissionId == missionId)?.IsClear ?? false;

    private void AddMissionReward(List<QuestReward> rewards, EntityMQuestMission mission)
    {
        EntityMQuestMissionReward? reward = _masterDb.EntityMQuestMissionReward.FirstOrDefault(r => r.QuestMissionRewardId == mission.QuestMissionRewardId);
        if (reward != null)
        {
            rewards.Add(ToReward(reward.PossessionType, reward.PossessionId, reward.Count));
        }
    }

    private static QuestReward ToReward(PossessionType possessionType, int possessionId, int count)
        => new() { PossessionType = (int)possessionType, PossessionId = possessionId, Count = count };

    /// <summary>
    /// Applies user, character and costume EXP, capped at each map's max level. A user level-up refills
    /// stamina to the new level's maximum. Rental quests skip character and costume EXP.
    /// </summary>
    private void ApplyExpRewards(DarkUserMemoryDatabase userDb, EntityMQuest quest, long nowMs)
    {
        // User EXP is always applied regardless of deck.
        EntityIUserStatus? status = userDb.EntityIUserStatus.FirstOrDefault();
        if (status != null)
        {
            int previousLevel = status.Level;
            (status.Level, status.Exp) = LevelHelper.ApplyExp(_masterDb, LevelHelper.UserExpMapId, status.Exp + quest.UserExp);

            EntityMUserLevel? userLevel = _masterDb.EntityMUserLevel.Find(status.Level);
            if (status.Level > previousLevel && userLevel != null)
            {
                StaminaHelper.RefillToMax(status, userLevel.MaxStamina, nowMs);
            }
        }

        // Rental quests use borrowed characters; EXP is not awarded to the player's own roster.
        if (IsRentalQuest(quest.QuestId))
        {
            return;
        }

        // Character/costume EXP only applies to units in the selected deck.
        EntitySQuestSession? session = userDb.EntitySQuestSession.FirstOrDefault(s => s.QuestId == quest.QuestId);
        if (session == null || session.UserDeckNumber == 0)
        {
            return;
        }

        EntityIUserDeck? deck = userDb.EntityIUserDeck.FirstOrDefault(d => d.DeckType == DeckType.QUEST && d.UserDeckNumber == session.UserDeckNumber);
        if (deck == null)
        {
            return;
        }

        // Collect costume UUIDs from the 3 character slots in this deck.
        HashSet<string> costumeUuids = [];
        foreach (string deckCharacterUuid in (string[])[deck.UserDeckCharacterUuid01, deck.UserDeckCharacterUuid02, deck.UserDeckCharacterUuid03])
        {
            if (string.IsNullOrEmpty(deckCharacterUuid))
            {
                continue;
            }

            EntityIUserDeckCharacter? deckCharacter = userDb.EntityIUserDeckCharacter.FirstOrDefault(c => c.UserDeckCharacterUuid == deckCharacterUuid);
            if (deckCharacter == null || string.IsNullOrEmpty(deckCharacter.UserCostumeUuid))
            {
                continue;
            }

            costumeUuids.Add(deckCharacter.UserCostumeUuid);
        }

        // Collect character IDs via master costume data.
        HashSet<int> characterIds = [];
        foreach (string costumeUuid in costumeUuids)
        {
            EntityIUserCostume? userCostume = userDb.EntityIUserCostume.FirstOrDefault(c => c.UserCostumeUuid == costumeUuid);
            if (userCostume == null)
            {
                continue;
            }

            EntityMCostume? masterCostume = _masterDb.EntityMCostume.FirstOrDefault(c => c.CostumeId == userCostume.CostumeId);
            if (masterCostume != null)
            {
                characterIds.Add(masterCostume.CharacterId);
            }
        }

        foreach (EntityIUserCharacter character in userDb.EntityIUserCharacter.Where(c => characterIds.Contains(c.CharacterId)))
        {
            (character.Level, character.Exp) = LevelHelper.ApplyExp(_masterDb, LevelHelper.CharacterExpMapId, character.Exp + quest.CharacterExp);
        }

        foreach (EntityIUserCostume costume in userDb.EntityIUserCostume.Where(c => costumeUuids.Contains(c.UserCostumeUuid)))
        {
            EntityMCostume? masterCostume = _masterDb.EntityMCostume.FirstOrDefault(c => c.CostumeId == costume.CostumeId);
            if (masterCostume == null)
            {
                continue;
            }

            EntityMCostumeRarity? rarity = _masterDb.EntityMCostumeRarity.FirstOrDefault(r => r.RarityType == masterCostume.RarityType);
            if (rarity == null)
            {
                continue;
            }

            (costume.Level, costume.Exp) = LevelHelper.ApplyExp(_masterDb, rarity.RequiredExpForLevelUpNumericalParameterMapId, costume.Exp + quest.CostumeExp);
        }
    }

    /// <summary>A quest is rental when any of its scenes uses a battle group with a rental deck entry.</summary>
    private bool IsRentalQuest(int questId)
    {
        foreach (int sceneId in _masterDb.EntityMQuestScene.Where(s => s.QuestId == questId).Select(s => s.QuestSceneId))
        {
            EntityMQuestSceneBattle? battle = _masterDb.EntityMQuestSceneBattle.FirstOrDefault(b => b.QuestSceneId == sceneId);
            if (battle != null && _masterDb.EntityMBattleRentalDeck.Any(r => r.BattleGroupId == battle.BattleGroupId))
            {
                return true;
            }
        }

        return false;
    }

    private void ApplyGoldReward(DarkUserMemoryDatabase userDb, EntityMQuest quest, long nowMs)
    {
        if (quest.Gold <= 0)
        {
            return;
        }

        PossessionHelper.Apply(userDb, PossessionType.CONSUMABLE_ITEM, _gameConfig.ConsumableItemIdForGold, quest.Gold, _masterDb, nowMs);
    }

    /// <summary>Unlocks weapon stories triggered by QUEST_CLEAR. ACQUISITION unlocks are handled by WeaponHelper.GrantWeapon.</summary>
    private void ApplyWeaponStoryUnlocks(DarkUserMemoryDatabase userDb, int questId)
    {
        foreach (EntityMWeaponStoryReleaseConditionGroup condition in _masterDb.EntityMWeaponStoryReleaseConditionGroup
            .Where(c => c.WeaponStoryReleaseConditionType == WeaponStoryReleaseConditionType.QUEST_CLEAR && c.ConditionValue == questId))
        {
            EntityMWeapon? weapon = _masterDb.EntityMWeapon.FirstOrDefault(w => w.WeaponStoryReleaseConditionGroupId == condition.WeaponStoryReleaseConditionGroupId);
            if (weapon != null)
            {
                WeaponHelper.GrantWeaponStory(userDb, weapon.WeaponId, condition.StoryIndex);
            }
        }
    }

    private static void ParseAutoSaleRules(DarkUserMemoryDatabase userDb, out HashSet<int> raritySet, out HashSet<int> rankSet)
    {
        raritySet = [];
        rankSet = [];

        foreach (EntityIUserAutoSaleSettingDetail setting in userDb.EntityIUserAutoSaleSettingDetail)
        {
            foreach (int value in ExtractInts(setting.PossessionAutoSaleItemValue))
            {
                if (value is 10 or 20 or 30 or 40 or 50)
                {
                    raritySet.Add(value);
                }
                else if (value is >= 1 and <= 5)
                {
                    rankSet.Add(value);
                }
            }
        }
    }

    private static IEnumerable<int> ExtractInts(string value)
    {
        int current = 0;
        bool hasDigits = false;

        foreach (char c in value)
        {
            if (char.IsDigit(c))
            {
                current = (current * 10) + (c - '0');
                hasDigits = true;
                continue;
            }

            if (!hasDigits)
            {
                continue;
            }

            yield return current;
            current = 0;
            hasDigits = false;
        }

        if (hasDigits)
        {
            yield return current;
        }
    }
}
