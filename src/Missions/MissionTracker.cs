using MariesWonderland.Data;
using MariesWonderland.Helpers;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Missions;

/// <summary>A mission reward granted (or lost to an ended term) by <see cref="MissionTracker.ReceiveRewards"/>.</summary>
public sealed record MissionRewardGrant(PossessionType PossessionType, int PossessionId, int Count);

/// <summary>
/// The one place that writes <c>IUserMission</c>. A mission's row is created when the mission becomes available
/// (term open, unlock met, or granted by a shop purchase). Counter conditions count from then on, never earlier
/// activity; state conditions (levels, scores, exalts) read the account as it is now, so they can clear at once.
/// Progress keeps counting after a mission clears, until its reward is received.
/// </summary>
public sealed class MissionTracker(DarkMasterMemoryDatabase masterDb, MissionCatalog catalog)
{
    private readonly DarkMasterMemoryDatabase _masterDb = masterDb;
    private readonly MissionCatalog _catalog = catalog;

    /// <summary>
    /// Brings the user's missions up to date: restarts daily missions on a new game day, creates rows for missions
    /// that became available, re-reads state conditions and evaluates the "clear all dailies" missions.
    /// </summary>
    public void Refresh(DarkUserMemoryDatabase userDb, long nowMs)
    {
        Dictionary<int, EntityIUserMission> rows = userDb.EntityIUserMission.ToDictionary(r => r.MissionId);

        ResetDailies(rows, nowMs);
        foreach (MissionDefinition mission in _catalog.All)
        {
            if (!rows.ContainsKey(mission.MissionId) && mission.IsInTerm(nowMs) && IsUnlocked(userDb, rows, mission))
            {
                rows[mission.MissionId] = CreateRow(userDb, mission, nowMs);
            }
        }

        EvaluateStateConditions(userDb, rows, nowMs);
        EvaluateDailyCompletion(userDb, rows, nowMs);
    }

    /// <summary>
    /// Adds <paramref name="amount"/> to every available counter mission of <paramref name="conditionType"/> whose
    /// filter accepts the event, then settles any missions that cleared.
    /// </summary>
    public void Record(DarkUserMemoryDatabase userDb, MissionClearConditionType conditionType, int amount, Func<MissionFilter?, bool> matches, long nowMs)
    {
        Dictionary<int, EntityIUserMission> rows = userDb.EntityIUserMission.ToDictionary(r => r.MissionId);
        foreach (MissionDefinition mission in _catalog.ByConditionType(conditionType))
        {
            if (!mission.IsFilterUnmapped && matches(mission.Filter) && mission.IsInTerm(nowMs)
                && rows.TryGetValue(mission.MissionId, out EntityIUserMission? row) && row.MissionProgressStatusType != MissionProgressStatusType.REWARD_RECEIVED)
            {
                AddProgress(userDb, rows, mission, row, amount, nowMs);
            }
        }

        // The action behind the event (a quest clear, an enhance) can also have raised levels or scores.
        EvaluateStateConditions(userDb, rows, nowMs);
        EvaluateDailyCompletion(userDb, rows, nowMs);
    }

    /// <summary>An unfiltered counter event: counts for missions without a filter only.</summary>
    public void Record(DarkUserMemoryDatabase userDb, MissionClearConditionType conditionType, int amount, long nowMs)
        => Record(userDb, conditionType, amount, filter => filter == null, nowMs);

    /// <summary>An event on one weapon: counts for unfiltered missions and those naming the weapon.</summary>
    public void RecordWeapon(DarkUserMemoryDatabase userDb, MissionClearConditionType conditionType, int weaponId, int amount, long nowMs)
        => Record(userDb, conditionType, amount, filter => filter == null || filter.MatchesWeapon(weaponId), nowMs);

    /// <summary>An event on one costume: counts for unfiltered missions and those naming the costume.</summary>
    public void RecordCostume(DarkUserMemoryDatabase userDb, MissionClearConditionType conditionType, int costumeId, int amount, long nowMs)
        => Record(userDb, conditionType, amount, filter => filter == null || filter.MatchesCostume(costumeId), nowMs);

    /// <summary>An event in one exploration: counts for unfiltered missions and those naming the exploration.</summary>
    public void RecordExplore(DarkUserMemoryDatabase userDb, MissionClearConditionType conditionType, int exploreId, int amount, long nowMs)
        => Record(userDb, conditionType, amount, filter => filter == null || filter.MatchesExplore(exploreId), nowMs);

    /// <summary>Panels newly unlocked on one Mythic Slab board group: counts for unfiltered missions and those naming the group.</summary>
    public void RecordBoardPanels(DarkUserMemoryDatabase userDb, int characterBoardGroupId, int panels, long nowMs)
        => Record(userDb, MissionClearConditionType.CHARACTER_BOARD_PANEL_RELEASE_BY_COUNT, panels, filter => filter == null || filter.MatchesBoardGroup(characterBoardGroupId), nowMs);

    /// <summary>Items drawn from a summon: counts for unfiltered missions and those naming this kind of summon.</summary>
    public void RecordGachaDraw(DarkUserMemoryDatabase userDb, GachaLabelType? labelType, IReadOnlyCollection<int> gachaIds, bool isDailyFreeDraw, int draws, long nowMs, int priceItemId = 0)
        => Record(userDb, MissionClearConditionType.GACHA_DRAW_BY_COUNT, draws, filter => filter == null || filter.MatchesGacha(labelType, gachaIds, isDailyFreeDraw, priceItemId), nowMs);

    /// <summary>Items bought in one shop (each item counts, so 5 of one item is 5): counts for unfiltered missions and those naming the shop type.</summary>
    public void RecordShopBuy(DarkUserMemoryDatabase userDb, ShopType shopType, int quantity, long nowMs)
        => Record(userDb, MissionClearConditionType.SHOP_BUY_BY_COUNT, quantity, filter => filter == null || filter.MatchesShop(shopType), nowMs);

    /// <summary>A profile comment was set: counts for missions asking for any comment or for this exact one.</summary>
    public void RecordProfileMessage(DarkUserMemoryDatabase userDb, string message, long nowMs)
        => Record(userDb, MissionClearConditionType.USER_MESSAGE_MATCH_WORD, 1, filter => filter == null || filter.MatchesMessage(message), nowMs);

    /// <summary>
    /// Counts <paramref name="times"/> clears of a quest for the quest-clear missions it matches. A skipped clear
    /// (skip ticket) doesn't count for the "without skip" missions.
    /// </summary>
    public void RecordQuestClear(DarkUserMemoryDatabase userDb, MissionQuestEvent quest, int times, bool isSkipped, long nowMs)
    {
        Record(userDb, MissionClearConditionType.QUEST_CLEAR_BY_COUNT, times, filter => filter == null || filter.MatchesQuest(quest), nowMs);
        if (!isSkipped)
        {
            Record(userDb, MissionClearConditionType.QUEST_CLEAR_BY_COUNT_WITHOUT_SKIP, times, filter => filter == null || filter.MatchesQuest(quest), nowMs);
        }
    }

    /// <summary>Re-reads the state conditions (levels, scores, exalts) after something changed them outside an event.</summary>
    public void EvaluateStates(DarkUserMemoryDatabase userDb, long nowMs)
    {
        Dictionary<int, EntityIUserMission> rows = userDb.EntityIUserMission.ToDictionary(r => r.MissionId);
        EvaluateStateConditions(userDb, rows, nowMs);
        EvaluateDailyCompletion(userDb, rows, nowMs);
    }

    /// <summary>Makes the missions of a group available, as when a shop purchase grants them.</summary>
    public void GrantGroup(DarkUserMemoryDatabase userDb, int missionGroupId, long nowMs)
    {
        Dictionary<int, EntityIUserMission> rows = userDb.EntityIUserMission.ToDictionary(r => r.MissionId);
        foreach (MissionDefinition mission in _catalog.ByGroup(missionGroupId))
        {
            if (!rows.ContainsKey(mission.MissionId) && mission.IsInTerm(nowMs))
            {
                rows[mission.MissionId] = CreateRow(userDb, mission, nowMs);
            }
        }

        EvaluateStateConditions(userDb, rows, nowMs);
    }

    /// <summary>
    /// Grants the rewards of cleared missions and marks them received. A mission whose term has ended is marked
    /// received too, but its rewards are returned as expired instead of granted.
    /// </summary>
    public (List<MissionRewardGrant> Received, List<MissionRewardGrant> Expired) ReceiveRewards(DarkUserMemoryDatabase userDb, IEnumerable<int> missionIds, long nowMs)
    {
        List<MissionRewardGrant> received = [];
        List<MissionRewardGrant> expired = [];
        foreach (int missionId in missionIds.Distinct())
        {
            EntityIUserMission? row = userDb.EntityIUserMission.FirstOrDefault(r => r.MissionId == missionId);
            MissionDefinition? mission = _catalog.Find(missionId);
            if (row == null || mission == null || row.MissionProgressStatusType != MissionProgressStatusType.CLEAR)
            {
                continue;
            }

            bool isExpired = !mission.IsInTerm(nowMs);
            foreach (EntityMMissionReward reward in mission.Rewards)
            {
                MissionRewardGrant grant = new(reward.PossessionType, reward.PossessionId, reward.Count);
                if (isExpired)
                {
                    expired.Add(grant);
                    continue;
                }

                PossessionHelper.Apply(userDb, reward.PossessionType, reward.PossessionId, reward.Count, _masterDb, nowMs);
                received.Add(grant);
            }

            row.MissionProgressStatusType = MissionProgressStatusType.REWARD_RECEIVED;
            row.LatestVersion = nowMs;
        }

        return (received, expired);
    }

    // ────────── Lifecycle ──────────

    private bool IsUnlocked(DarkUserMemoryDatabase userDb, Dictionary<int, EntityIUserMission> rows, MissionDefinition mission)
        => mission.UnlockType switch
        {
            MissionUnlockConditionType.UNKNOWN => true,
            MissionUnlockConditionType.QUEST_CLEAR => userDb.EntityIUserQuest.Any(q => q.QuestId == mission.UnlockValue && q.ClearCount > 0),
            MissionUnlockConditionType.MISSION_CLEAR_BY_ID => rows.TryGetValue(mission.UnlockValue, out EntityIUserMission? required) && IsCleared(required),
            // Value 0 is the hidden-story (EVALUATE category) missions: always available.
            MissionUnlockConditionType.EVALUATE => mission.UnlockValue == 0,
            // GRANT missions come from GrantGroup (shop purchases); webview panel pages and the rest are not supported yet.
            _ => false
        };

    private static EntityIUserMission CreateRow(DarkUserMemoryDatabase userDb, MissionDefinition mission, long nowMs)
    {
        EntityIUserMission row = new()
        {
            UserId = userDb.UserId,
            MissionId = mission.MissionId,
            StartDatetime = nowMs,
            MissionProgressStatusType = MissionProgressStatusType.IN_PROGRESS,
            LatestVersion = nowMs
        };
        userDb.EntityIUserMission.Add(row);
        return row;
    }

    /// <summary>Daily missions started before the current game day start over: progress 0, in progress again.</summary>
    private void ResetDailies(Dictionary<int, EntityIUserMission> rows, long nowMs)
    {
        long dayStart = GameDayHelper.StartOfDay(nowMs);
        foreach (EntityIUserMission row in rows.Values)
        {
            MissionDefinition? mission = _catalog.Find(row.MissionId);
            if (mission is { IsDaily: true } && row.StartDatetime < dayStart && mission.IsInTerm(nowMs))
            {
                row.StartDatetime = nowMs;
                row.ProgressValue = 0;
                row.MissionProgressStatusType = MissionProgressStatusType.IN_PROGRESS;
                row.ClearDatetime = 0;
                row.LatestVersion = nowMs;
            }
        }
    }

    private static bool IsCleared(EntityIUserMission row)
        => row.MissionProgressStatusType is MissionProgressStatusType.CLEAR or MissionProgressStatusType.REWARD_RECEIVED;

    /// <summary>
    /// Sets progress and clears the mission once it reaches the target. Each mission that clears counts towards
    /// the "complete N missions" missions.
    /// </summary>
    private void SetProgress(DarkUserMemoryDatabase userDb, Dictionary<int, EntityIUserMission> rows, MissionDefinition mission, EntityIUserMission row, long progress, long nowMs)
    {
        int value = (int)Math.Clamp(progress, 0, int.MaxValue);
        if (value == row.ProgressValue)
        {
            return;
        }

        row.ProgressValue = value;
        row.LatestVersion = nowMs;
        if (row.MissionProgressStatusType != MissionProgressStatusType.IN_PROGRESS || value < mission.ClearValue)
        {
            return;
        }

        row.MissionProgressStatusType = MissionProgressStatusType.CLEAR;
        row.ClearDatetime = nowMs;

        foreach (MissionDefinition counter in _catalog.ByConditionType(MissionClearConditionType.MISSION_CLEAR_BY_COUNT))
        {
            if (counter.MissionId != mission.MissionId && counter.IsInTerm(nowMs)
                && rows.TryGetValue(counter.MissionId, out EntityIUserMission? counterRow) && !IsReceived(counterRow))
            {
                AddProgress(userDb, rows, counter, counterRow, 1, nowMs);
            }
        }
    }

    private void AddProgress(DarkUserMemoryDatabase userDb, Dictionary<int, EntityIUserMission> rows, MissionDefinition mission, EntityIUserMission row, int amount, long nowMs)
        => SetProgress(userDb, rows, mission, row, (long)row.ProgressValue + amount, nowMs);

    private static bool IsReceived(EntityIUserMission row) => row.MissionProgressStatusType == MissionProgressStatusType.REWARD_RECEIVED;

    // ────────── State conditions ──────────

    /// <summary>Raises the progress of missions that read the account's current state (never lowers it).</summary>
    private void EvaluateStateConditions(DarkUserMemoryDatabase userDb, Dictionary<int, EntityIUserMission> rows, long nowMs)
    {
        foreach (EntityIUserMission row in rows.Values.ToList())
        {
            MissionDefinition? mission = _catalog.Find(row.MissionId);
            if (mission == null || mission.IsFilterUnmapped || IsReceived(row) || !mission.IsInTerm(nowMs))
            {
                continue;
            }

            long? state = StateValue(userDb, mission);
            if (state is long value && value > row.ProgressValue)
            {
                SetProgress(userDb, rows, mission, row, value, nowMs);
            }
        }
    }

    /// <summary>The account's current value for a state condition; null for counter and unsupported conditions.</summary>
    private static long? StateValue(DarkUserMemoryDatabase userDb, MissionDefinition mission)
    {
        MissionFilter? filter = mission.Filter;
        return mission.ConditionType switch
        {
            MissionClearConditionType.USER_LEVEL => userDb.EntityIUserStatus.FirstOrDefault()?.Level ?? 0,
            MissionClearConditionType.MAX_DECK_POWER => Math.Max(
                userDb.EntityIUserDeckTypeNote.Select(n => n.MaxDeckPower).DefaultIfEmpty(0).Max(),
                userDb.EntityIUserDeck.Select(d => d.Power).DefaultIfEmpty(0).Max()),
            MissionClearConditionType.COSTUME_MAX_LEVEL => userDb.EntityIUserCostume
                .Where(c => filter?.CostumeIds == null || filter.CostumeIds.Contains(c.CostumeId))
                .Select(c => c.Level).DefaultIfEmpty(0).Max(),
            // Every evolution stage is listed, so the weapon counts before and after evolving.
            MissionClearConditionType.WEAPON_MAX_LEVEL => userDb.EntityIUserWeapon
                .Where(w => filter?.WeaponIds == null || filter.WeaponIds.Contains(w.WeaponId))
                .Select(w => w.Level).DefaultIfEmpty(0).Max(),
            // Best single-run score in the explorations the filter names (Normal and Hard are separate explore ids).
            MissionClearConditionType.EXPLORE_HIGH_SCORE => userDb.EntityIUserExploreScore
                .Where(s => filter?.ExploreIds == null || filter.ExploreIds.Contains(s.ExploreId))
                .Select(s => (long)s.MaxScore).DefaultIfEmpty(0).Max(),
            MissionClearConditionType.COSTUME_SKILL_MAX_LEVEL => userDb.EntityIUserCostumeActiveSkill.Select(s => s.Level).DefaultIfEmpty(0).Max(),
            MissionClearConditionType.WEAPON_SKILL_MAX_LEVEL => userDb.EntityIUserWeaponSkill.Select(s => s.Level).DefaultIfEmpty(0).Max(),
            MissionClearConditionType.WEAPON_ABILITY_MAX_LEVEL => userDb.EntityIUserWeaponAbility.Select(a => a.Level).DefaultIfEmpty(0).Max(),
            MissionClearConditionType.CHARACTER_REBIRTH_COUNT => userDb.EntityIUserCharacterRebirth
                .Where(r => filter?.CharacterId == null || r.CharacterId == filter.CharacterId)
                .Select(r => r.RebirthCount).DefaultIfEmpty(0).Max(),
            MissionClearConditionType.BIG_HUNT_HIGH_SCORE => userDb.EntityIUserBigHuntMaxScore
                .Where(s => filter?.BigHuntBossId == null || s.BigHuntBossId == filter.BigHuntBossId)
                .Select(s => s.MaxScore).DefaultIfEmpty(0).Max(),
            _ => null
        };
    }

    // ────────── "Clear all daily missions" ──────────

    /// <summary>
    /// Clears the "clear all daily missions" missions once every other daily mission available today is cleared;
    /// the sub-category variant looks only at daily missions of its own sub-category.
    /// </summary>
    private void EvaluateDailyCompletion(DarkUserMemoryDatabase userDb, Dictionary<int, EntityIUserMission> rows, long nowMs)
    {
        foreach (MissionDefinition meta in _catalog.ByConditionType(MissionClearConditionType.MISSION_CLEAR_FOR_ALL_DAILY)
            .Concat(_catalog.ByConditionType(MissionClearConditionType.MISSION_CLEAR_FOR_ALL_DAILY_BY_SUB_CATEGORY_ID)))
        {
            if (!rows.TryGetValue(meta.MissionId, out EntityIUserMission? metaRow) || metaRow.MissionProgressStatusType != MissionProgressStatusType.IN_PROGRESS || !meta.IsInTerm(nowMs))
            {
                continue;
            }

            bool bySubCategory = meta.ConditionType == MissionClearConditionType.MISSION_CLEAR_FOR_ALL_DAILY_BY_SUB_CATEGORY_ID;
            List<MissionDefinition> dailies = [.. _catalog.All.Where(d =>
                d.Category == meta.Category && d.IsDaily && d.IsInTerm(nowMs) && rows.ContainsKey(d.MissionId)
                && d.ConditionType is not (MissionClearConditionType.MISSION_CLEAR_FOR_ALL_DAILY or MissionClearConditionType.MISSION_CLEAR_FOR_ALL_DAILY_BY_SUB_CATEGORY_ID)
                && (!bySubCategory || d.SubCategoryId == meta.SubCategoryId))];
            if (dailies.Count > 0 && dailies.All(d => IsCleared(rows[d.MissionId])))
            {
                SetProgress(userDb, rows, meta, metaRow, Math.Max(metaRow.ProgressValue, meta.ClearValue), nowMs);
            }
        }
    }
}
