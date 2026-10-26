using MariesWonderland.Configuration;
using MariesWonderland.Data;
using MariesWonderland.Helpers;
using MariesWonderland.Missions;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Gift;
using MariesWonderland.Proto.Quest;
using Microsoft.Extensions.Options;

namespace MariesWonderland.Quests;

/// <summary>
/// Quest rules shared by every quest type: quest and mission rows, first-clear and mission rewards,
/// big wins, drops, EXP and gold. Each quest type's service handles its own progress tables and flow,
/// and calls in here for the rest.
/// </summary>
public sealed class QuestEngine(DarkMasterMemoryDatabase masterDb, GameConfig gameConfig, QuestCampaignHelper questCampaigns, ImportantItemEffectHelper importantItems, MissionTracker missions, IOptions<DevModeOptions> devMode)
{
    private readonly DarkMasterMemoryDatabase _masterDb = masterDb;
    private readonly GameConfig _gameConfig = gameConfig;
    private readonly QuestCampaignHelper _questCampaigns = questCampaigns;
    private readonly ImportantItemEffectHelper _importantItems = importantItems;
    private readonly MissionTracker _missions = missions;
    private readonly DevModeOptions _devMode = devMode.Value;
    private readonly QuestMissionEvaluator _missionEvaluator = new(masterDb);

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
    /// Spends the stamina for <paramref name="times"/> runs of the quest (after stamina campaigns, beginner and
    /// comeback ones included), unless
    /// <see cref="DevModeOptions.FreeStamina"/> is on. The cost counts for stamina missions either way.
    /// </summary>
    public void ConsumeStamina(DarkUserMemoryDatabase userDb, EntityMQuest? quest, QuestCampaignTarget campaignTarget, int times, long nowMs)
    {
        if (quest == null || quest.Stamina <= 0 || times <= 0)
        {
            return;
        }

        int cost = _questCampaigns.ApplyStaminaCampaign(quest.Stamina, campaignTarget, nowMs, UserCampaignStatus.Of(_masterDb, userDb)) * times;
        _missions.Record(userDb, MissionClearConditionType.CONSUME_STAMINA_AMOUNT, cost, nowMs);
        if (_devMode.IsFreeStaminaEnabled)
        {
            return;
        }

        EntityIUserStatus status = userDb.EntityIUserStatus[0];
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
        ResetStaleDailyClearCount(userQuest, nowMs);
        userQuest.QuestStateType = (int)QuestStateType.ACTIVE;
        userQuest.LatestStartDatetime = nowMs;
    }

    /// <summary>A quest's daily clear count starts again when its last clear was before today's game day.</summary>
    public static void ResetStaleDailyClearCount(EntityIUserQuest userQuest, long nowMs)
    {
        if (userQuest.DailyClearCount > 0 && GameDayHelper.IsBeforeToday(userQuest.LastClearDatetime, nowMs))
        {
            userQuest.DailyClearCount = 0;
        }
    }

    /// <summary>Resets every quest's stale daily clear count, so the client sees today's limits.</summary>
    public static void ResetDailyClearCounts(DarkUserMemoryDatabase userDb, long nowMs)
    {
        foreach (EntityIUserQuest userQuest in userDb.EntityIUserQuest)
        {
            ResetStaleDailyClearCount(userQuest, nowMs);
        }
    }

    /// <summary>Keeps the fastest clear: the frames of all the run's waves added up.</summary>
    private static void RecordClearTime(EntityIUserQuest userQuest, IReadOnlyList<EntitySBattleDetail>? battles)
    {
        long frames = battles?.Sum(b => b.ElapsedFrameCount) ?? 0;
        if (frames > 0 && (userQuest.ShortestClearFrames == 0 || frames < userQuest.ShortestClearFrames))
        {
            userQuest.ShortestClearFrames = (int)frames;
        }
    }

    public static void MarkCleared(EntityIUserQuest userQuest, int clearCount, long nowMs)
    {
        ResetStaleDailyClearCount(userQuest, nowMs);
        userQuest.QuestStateType = (int)QuestStateType.CLEARED;
        userQuest.ClearCount += clearCount;
        userQuest.DailyClearCount += clearCount;
        userQuest.LastClearDatetime = nowMs;
    }

    /// <summary>
    /// Finishes a run. A win grants EXP and gold, first-clear rewards (first clear only), the rewards of
    /// missions not yet cleared plus the big win when this run completes them, and drops; then marks the
    /// quest and its missions cleared. A loss (retired or annihilated) grants nothing and leaves the
    /// missions as they were; a quest cleared before goes back to <see cref="QuestStateType.CLEARED"/>.
    /// A replay win grants only EXP, gold and drops: replay quests have no first-clear or mission rewards
    /// and keep no mission rows. A mission counts when the run's <paramref name="battles"/> and deck meet its
    /// condition (<see cref="QuestMissionEvaluator"/>); without battles every mission counts.
    /// </summary>
    public QuestFinishResult Finish(DarkUserMemoryDatabase userDb, EntityMQuest quest, QuestCampaignTarget campaignTarget, bool isWin, long nowMs, bool isReplay = false, IReadOnlyList<EntitySBattleDetail>? battles = null)
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
        QuestRun? run = battles == null ? null : _missionEvaluator.BuildRun(userDb, quest.QuestId, battles);
        List<QuestReward> missionClearRewards = [];
        List<QuestReward> bigWinRewards = [];
        HashSet<int> missionsMetThisRun = [];
        int regularMissionCount = 0;
        bool allRegularMissionsCleared = true;
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
            bool wasCleared = IsMissionCleared(userDb, quest.QuestId, mission.QuestMissionId);
            bool isMet = run == null || _missionEvaluator.IsMet(mission, run);
            if (isMet)
            {
                missionsMetThisRun.Add(mission.QuestMissionId);
                if (!wasCleared)
                {
                    AddMissionReward(missionClearRewards, mission);
                }
            }

            allRegularMissionsCleared &= wasCleared || isMet;
        }

        bool isBigWin = false;
        if (regularMissionCount > 0 && bigWinMissionId != 0 && allRegularMissionsCleared)
        {
            missionsMetThisRun.Add(bigWinMissionId);
            if (!IsMissionCleared(userDb, quest.QuestId, bigWinMissionId))
            {
                isBigWin = true;
                EntityMQuestMission? bigWinMission = _masterDb.EntityMQuestMission.FirstOrDefault(m => m.QuestMissionId == bigWinMissionId);
                if (bigWinMission != null)
                {
                    AddMissionReward(bigWinRewards, bigWinMission);
                }
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
            UserCampaignHelper.StartBeginnerCampaignOnFirstClear(_masterDb, userDb, quest.QuestId, nowMs);
        }

        // Only missions not cleared yet are listed, so each mission's reward is granted once.
        foreach (QuestReward reward in missionClearRewards.Concat(bigWinRewards))
        {
            PossessionHelper.Apply(userDb, (PossessionType)reward.PossessionType, reward.PossessionId, reward.Count, _masterDb, nowMs);
        }

        List<QuestReward> dropRewards = ApplyDropRewards(userDb, BuildDropRewards(userDb, quest, campaignTarget, nowMs), nowMs);
        MarkCleared(userQuest, 1, nowMs);
        RecordClearTime(userQuest, battles);
        (HashSet<int> deckCharacterIds, HashSet<int> deckCostumeIds) = DeckMembers(userDb, quest.QuestId);
        _missions.RecordQuestClear(userDb, new MissionQuestEvent(campaignTarget, deckCharacterIds, deckCostumeIds, quest.DailyClearableCount), 1, isSkipped: false, nowMs);
        if (!isReplay)
        {
            ClearQuestMissions(userDb, quest.QuestId, nowMs, missionsMetThisRun);
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
        List<QuestReward> drops = ApplyDropRewards(userDb, BuildDropRewards(userDb, quest, campaignTarget, nowMs), nowMs);
        ApplyGoldReward(userDb, quest, nowMs);
        ApplyExpRewards(userDb, quest, nowMs);
        _missions.RecordQuestClear(userDb, new MissionQuestEvent(campaignTarget, [], DailyClearableCount: quest.DailyClearableCount), 1, isSkipped: true, nowMs);
        return drops;
    }

    /// <summary>
    /// The characters and costumes in the deck the quest was started with (from its session), for "with X in your
    /// loadout" missions. Both empty when the session or deck is unknown.
    /// </summary>
    public (HashSet<int> CharacterIds, HashSet<int> CostumeIds) DeckMembers(DarkUserMemoryDatabase userDb, int questId)
    {
        int deckNumber = userDb.EntitySQuestSession.FirstOrDefault(s => s.QuestId == questId)?.UserDeckNumber ?? 0;
        EntityIUserDeck? deck = userDb.EntityIUserDeck.FirstOrDefault(d => d.DeckType == DeckType.QUEST && d.UserDeckNumber == deckNumber);
        if (deck == null)
        {
            return ([], []);
        }

        HashSet<int> characterIds = [];
        HashSet<int> costumeIds = [];
        foreach (string? deckCharacterUuid in new[] { deck.UserDeckCharacterUuid01, deck.UserDeckCharacterUuid02, deck.UserDeckCharacterUuid03 })
        {
            string? costumeUuid = userDb.EntityIUserDeckCharacter.FirstOrDefault(c => c.UserDeckCharacterUuid == deckCharacterUuid)?.UserCostumeUuid;
            int costumeId = userDb.EntityIUserCostume.FirstOrDefault(c => c.UserCostumeUuid == costumeUuid)?.CostumeId ?? 0;
            EntityMCostume? costume = _masterDb.EntityMCostume.FirstOrDefault(c => c.CostumeId == costumeId);
            if (costume != null)
            {
                characterIds.Add(costume.CharacterId);
                costumeIds.Add(costume.CostumeId);
            }
        }

        return (characterIds, costumeIds);
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

    /// <summary>
    /// The quest's pickup drops with active drop campaigns (beginner and comeback ones too, for those users) and the
    /// user's important-item boosts applied (bonuses add together). Nothing is granted.
    /// </summary>
    public List<QuestReward> BuildDropRewards(DarkUserMemoryDatabase userDb, EntityMQuest quest, QuestCampaignTarget campaignTarget, long nowMs)
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

        return _questCampaigns.ApplyDropCampaignBonuses(
            rewards,
            campaignTarget,
            nowMs,
            reward => _importantItems.DropBonusPermil(userDb, campaignTarget, reward, nowMs),
            UserCampaignStatus.Of(_masterDb, userDb));
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
                    (int grantedPartsId, bool isAutoSold, EntityIUserParts? granted) = PossessionHelper.GrantOrSellPartsDrop(
                        userDb, reward.PossessionId, _masterDb, _gameConfig.ConsumableItemIdForGold, raritySet, rankSet, nowMs);

                    QuestReward applied = new()
                    {
                        PossessionType = reward.PossessionType,
                        PossessionId = grantedPartsId,
                        Count = 1,
                        RewardEffectId = RewardEffectHelper.EffectId(_masterDb, possessionType, grantedPartsId),
                        IsAutoSale = isAutoSold
                    };
                    if (granted != null)
                    {
                        // The result screen shows the memoir's level and stats from this.
                        applied.Parts = PartsMessage(userDb, granted);
                    }

                    appliedRewards.Add(applied);
                }

                continue;
            }

            PossessionHelper.Apply(userDb, possessionType, reward.PossessionId, reward.Count, _masterDb, nowMs);
            appliedRewards.Add(reward.Clone());
        }

        return appliedRewards;
    }

    /// <summary>A granted memoir as the client reads it from a reward: level, main stat and sub stats.</summary>
    private static Parts PartsMessage(DarkUserMemoryDatabase userDb, EntityIUserParts parts)
    {
        Parts message = new() { Level = parts.Level, PartsStatusMainId = parts.PartsStatusMainId };
        foreach (EntityIUserPartsStatusSub sub in userDb.EntityIUserPartsStatusSub
            .Where(s => s.UserPartsUuid == parts.UserPartsUuid)
            .OrderBy(s => s.StatusIndex))
        {
            message.PartsStatusSub.Add(new PartsStatusSub
            {
                StatusIndex = sub.StatusIndex,
                PartsStatusSubLotteryId = sub.PartsStatusSubLotteryId,
                Level = sub.Level,
                StatusKindType = (int)sub.StatusKindType,
                StatusCalculationType = (int)sub.StatusCalculationType,
                StatusChangeValue = sub.StatusChangeValue
            });
        }

        return message;
    }

    /// <summary>
    /// Marks the quest's missions cleared, creating any missing rows: all of them, or only
    /// <paramref name="clearOnly"/> among the listed ones.
    /// </summary>
    public void ClearQuestMissions(DarkUserMemoryDatabase userDb, int questId, long nowMs, IReadOnlySet<int>? clearOnly = null)
    {
        EntityMQuest? quest = FindQuest(questId);
        if (quest == null)
        {
            return;
        }

        // Invisible missions (no rewards, never listed) are cleared too; hidden stories unlock from them.
        foreach (int missionId in MissionIds(quest).Where(id => clearOnly == null || clearOnly.Contains(id)).Concat(InvisibleMissionIds(quest)))
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

    private IEnumerable<int> MissionIds(EntityMQuest quest) => MissionGroupIds(quest.QuestMissionGroupId);

    private IEnumerable<int> InvisibleMissionIds(EntityMQuest quest) => MissionGroupIds(quest.InvisibleQuestMissionGroupId);

    private IEnumerable<int> MissionGroupIds(int questMissionGroupId)
    {
        if (questMissionGroupId == 0)
        {
            return [];
        }

        return [.. _masterDb.EntityMQuestMissionGroup
            .Where(g => g.QuestMissionGroupId == questMissionGroupId)
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

    private QuestReward ToReward(PossessionType possessionType, int possessionId, int count)
        => new()
        {
            PossessionType = (int)possessionType,
            PossessionId = possessionId,
            Count = count,
            RewardEffectId = RewardEffectHelper.EffectId(_masterDb, possessionType, possessionId)
        };

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
            // Regeneration up to now counts against the level the user was at.
            StaminaHelper.Settle(status, _masterDb, _gameConfig, nowMs);
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

            int maxLevel = LevelHelper.CostumeMaxLevel(_masterDb, userDb, costume, masterCostume);
            (costume.Level, costume.Exp) = LevelHelper.ApplyExp(_masterDb, rarity.RequiredExpForLevelUpNumericalParameterMapId, costume.Exp + quest.CostumeExp, maxLevel);
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
