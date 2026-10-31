using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Helpers;
using MariesWonderland.Missions;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Gimmick;

namespace MariesWonderland.Services;

public class GimmickService(UserDataStore store, DarkMasterMemoryDatabase masterDb, GameConfig gameConfig, MissionTracker missions, TimeProvider timeProvider)
    : MariesWonderland.Proto.Gimmick.GimmickService.GimmickServiceBase
{
    private readonly UserDataStore _store = store;
    private readonly DarkMasterMemoryDatabase _masterDb = masterDb;
    private readonly GameConfig _gameConfig = gameConfig;
    private readonly MissionTracker _missions = missions;
    private readonly TimeProvider _timeProvider = timeProvider;

    /// <summary>
    /// The user moves on to the next sequence of a schedule (the next hidden story once one is read): records it and
    /// writes the rows its gimmicks need to show. The client resends this on every cage load, so it's idempotent.
    /// </summary>
    public override Task<UpdateSequenceResponse> UpdateSequence(UpdateSequenceRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);
        long nowMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();

        if (!userDb.EntityIUserGimmickSequence.Any(s =>
            s.GimmickSequenceScheduleId == request.GimmickSequenceScheduleId &&
            s.GimmickSequenceId == request.GimmickSequenceId))
        {
            userDb.EntityIUserGimmickSequence.Add(new EntityIUserGimmickSequence
            {
                UserId = userId,
                GimmickSequenceScheduleId = request.GimmickSequenceScheduleId,
                GimmickSequenceId = request.GimmickSequenceId,
            });
        }

        CageGimmickHelper.SeedSequenceGimmicks(_masterDb, userDb, request.GimmickSequenceScheduleId, request.GimmickSequenceId, nowMs);

        return Task.FromResult(new UpdateSequenceResponse());
    }

    /// <summary>
    /// The user picked up an ornament (the client sends the gimmick's bits with the new one set): records the bits and
    /// grants what the gimmick gives.
    /// </summary>
    public override Task<UpdateGimmickProgressResponse> UpdateGimmickProgress(UpdateGimmickProgressRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);
        long nowMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();
        List<GimmickReward> ornamentRewards = [];
        List<GimmickReward> sequenceClearRewards = [];
        bool isSequenceCleared = false;
        bool granted = false;

        EntityIUserGimmick gimmick = userDb.EntityIUserGimmick.GetOrCreate(
            g => g.GimmickSequenceScheduleId == request.GimmickSequenceScheduleId
                && g.GimmickSequenceId == request.GimmickSequenceId
                && g.GimmickId == request.GimmickId,
            () => new EntityIUserGimmick
            {
                UserId = userId,
                GimmickSequenceScheduleId = request.GimmickSequenceScheduleId,
                GimmickSequenceId = request.GimmickSequenceId,
                GimmickId = request.GimmickId,
            });
        gimmick.StartDatetime = nowMs;

        EntityMGimmickInterval? interval = _masterDb.EntityMGimmickInterval.FirstOrDefault(i => i.GimmickId == request.GimmickId);
        EntityIUserGimmickOrnamentProgress ornament = userDb.EntityIUserGimmickOrnamentProgress.GetOrCreate(
            o => o.GimmickSequenceScheduleId == request.GimmickSequenceScheduleId
                && o.GimmickSequenceId == request.GimmickSequenceId
                && o.GimmickId == request.GimmickId
                && o.GimmickOrnamentIndex == request.GimmickOrnamentIndex,
            () => new EntityIUserGimmickOrnamentProgress
            {
                UserId = userId,
                GimmickSequenceScheduleId = request.GimmickSequenceScheduleId,
                GimmickSequenceId = request.GimmickSequenceId,
                GimmickId = request.GimmickId,
                GimmickOrnamentIndex = request.GimmickOrnamentIndex,
                BaseDatetime = CageGimmickHelper.InitialBaseDatetime(interval, nowMs),
            });
        int previousBits = ornament.ProgressValueBit;
        long previousBase = ornament.BaseDatetime;
        ornament.ProgressValueBit = request.ProgressValueBit;
        ornament.BaseDatetime = nowMs;

        EntityMGimmick? masterGimmick = _masterDb.EntityMGimmick.FirstOrDefault(g => g.GimmickId == request.GimmickId);
        switch (masterGimmick?.GimmickType)
        {
            // A hidden-story report clears once its prerequisites are met: account missions (IUserMission) and
            // quest missions (IUserQuestMission), combined through ClearEvaluateConditionId.
            case GimmickType.REPORT:
                if (masterGimmick.ClearEvaluateConditionId != 0
                    && EvaluateConditionHelper.Evaluate(masterGimmick.ClearEvaluateConditionId, userDb, _masterDb) == false)
                {
                    break;
                }

                gimmick.IsGimmickCleared = true;
                (isSequenceCleared, sequenceClearRewards) = MarkSequenceClearedOnce(
                    userDb,
                    userId,
                    request.GimmickSequenceScheduleId,
                    request.GimmickSequenceId,
                    nowMs);
                break;

            // A fickle black bird: one reward when its bit is newly set.
            case GimmickType.CAGE_TREASURE_HUNT:
                if ((request.ProgressValueBit & ~previousBits) != 0)
                {
                    // Only the first catch of a spot counts as a bird slain; it comes back on later schedules.
                    bool isNewSpot = !userDb.EntityIUserGimmickOrnamentProgress.Any(o =>
                        o != ornament && o.GimmickId == request.GimmickId && o.GimmickOrnamentIndex == request.GimmickOrnamentIndex && o.ProgressValueBit != 0);
                    if (isNewSpot)
                    {
                        PlayHistoryHelper.Add(userDb, GamePlayHistoryType.BLACK_BIRDS_SLAIN, 1);
                    }

                    ornamentRewards.Add(GrantPlaceholderLoot(userDb, 1, nowMs));
                    granted = true;
                }
                break;

            // Lost Items: the whole pile goes at once; the timer restarts from now (BaseDatetime above).
            case GimmickType.CAGE_INTERVAL_DROP_ITEM:
                int piled = interval == null ? 0 : CageGimmickHelper.AvailableIntervalCount(interval, previousBase, nowMs);
                if (piled > 0)
                {
                    ornamentRewards.Add(GrantPlaceholderLoot(userDb, piled, nowMs));
                    granted = true;
                }
                break;

            // A Lost Archive: the client opens the archive of the first reward's important item, so it's sent on
            // every visit; it's granted only once.
            case GimmickType.CAGE_MEMORY:
                int? importantItemId = ResolveCageMemoryImportantItemId(masterGimmick);
                if (importantItemId is int itemId)
                {
                    if (!userDb.EntityIUserImportantItem.Any(item => item.ImportantItemId == itemId))
                    {
                        PossessionHelper.Apply(userDb, PossessionType.IMPORTANT_ITEM, itemId, 1, _masterDb, nowMs);
                        granted = true;
                    }

                    ornamentRewards.Add(new GimmickReward
                    {
                        PossessionType = (int)PossessionType.IMPORTANT_ITEM,
                        PossessionId = itemId,
                        Count = 1
                    });
                }
                break;
        }

        // Anything granted can complete missions ("Collect N Hidden Stories" counts the report important items).
        if (granted || sequenceClearRewards.Count > 0)
        {
            _missions.EvaluateStates(userDb, nowMs);
        }

        return Task.FromResult(new UpdateGimmickProgressResponse
        {
            GimmickOrnamentReward = { ornamentRewards },
            IsSequenceCleared = isSequenceCleared,
            GimmickSequenceClearReward = { sequenceClearRewards }
        });
    }

    /// <summary>
    /// Brings the user's gimmicks up to date with the schedules running now (the cage calls this when it loads):
    /// unfinished sequences of ended schedules are dropped; each running schedule whose release condition passes
    /// starts its first sequence; and every current sequence gets the rows its gimmicks need to show, including
    /// gimmicks whose own release condition has started to pass.
    /// </summary>
    public override Task<InitSequenceScheduleResponse> InitSequenceSchedule(Empty request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);
        long nowMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();

        List<EntityMGimmickSequenceSchedule> runningSchedules = [.. _masterDb.EntityMGimmickSequenceSchedule
            .Where(s => s.StartDatetime <= nowMs && nowMs <= s.EndDatetime)];
        HashSet<int> runningScheduleIds = [.. runningSchedules.Select(s => s.GimmickSequenceScheduleId)];

        foreach (EntityIUserGimmickSequence ended in userDb.EntityIUserGimmickSequence
                     .Where(s => !s.IsGimmickSequenceCleared && !runningScheduleIds.Contains(s.GimmickSequenceScheduleId))
                     .ToList())
        {
            CageGimmickHelper.RemoveUnfinishedSequenceGimmicks(userDb, ended.GimmickSequenceScheduleId, ended.GimmickSequenceId);
            userDb.EntityIUserGimmickSequence.Remove(ended);
        }

        // An unsupported release condition doesn't block (none exist in master data today).
        foreach (EntityMGimmickSequenceSchedule schedule in runningSchedules)
        {
            if (schedule.ReleaseEvaluateConditionId != 0
                && EvaluateConditionHelper.Evaluate(schedule.ReleaseEvaluateConditionId, userDb, _masterDb) == false)
            {
                continue;
            }

            userDb.EntityIUserGimmickSequence.GetOrCreate(
                s => s.GimmickSequenceScheduleId == schedule.GimmickSequenceScheduleId && s.GimmickSequenceId == schedule.FirstGimmickSequenceId,
                () => new EntityIUserGimmickSequence
                {
                    UserId = userId,
                    GimmickSequenceScheduleId = schedule.GimmickSequenceScheduleId,
                    GimmickSequenceId = schedule.FirstGimmickSequenceId
                });
        }

        foreach (EntityIUserGimmickSequence sequence in userDb.EntityIUserGimmickSequence
                     .Where(s => runningScheduleIds.Contains(s.GimmickSequenceScheduleId))
                     .ToList())
        {
            CageGimmickHelper.SeedSequenceGimmicks(_masterDb, userDb, sequence.GimmickSequenceScheduleId, sequence.GimmickSequenceId, nowMs);
        }

        return Task.FromResult(new InitSequenceScheduleResponse());
    }

    /// <summary>Marks one or more gimmicks as unlocked (a hidden story's missions start).</summary>
    public override Task<UnlockResponse> Unlock(UnlockRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        foreach (GimmickKey key in request.GimmickKey)
        {
            EntityIUserGimmickUnlock? unlock = userDb.EntityIUserGimmickUnlock.FirstOrDefault(u =>
                u.GimmickSequenceScheduleId == key.GimmickSequenceScheduleId &&
                u.GimmickSequenceId == key.GimmickSequenceId &&
                u.GimmickId == key.GimmickId);

            if (unlock is null)
            {
                unlock = new EntityIUserGimmickUnlock
                {
                    UserId = userId,
                    GimmickSequenceScheduleId = key.GimmickSequenceScheduleId,
                    GimmickSequenceId = key.GimmickSequenceId,
                    GimmickId = key.GimmickId,
                };
                userDb.EntityIUserGimmickUnlock.Add(unlock);
            }
            unlock.IsUnlocked = true;
        }

        return Task.FromResult(new UnlockResponse());
    }

    /// <summary>Grants <paramref name="times"/> placeholder loot rolls, merged into one reward line.</summary>
    private GimmickReward GrantPlaceholderLoot(DarkUserMemoryDatabase userDb, int times, long nowMs)
    {
        (PossessionType possessionType, int possessionId, int count) = CageGimmickHelper.PlaceholderLoot(_gameConfig);
        PossessionHelper.Apply(userDb, possessionType, possessionId, count * times, _masterDb, nowMs);
        return new GimmickReward { PossessionType = (int)possessionType, PossessionId = possessionId, Count = count * times };
    }

    private (bool SequenceCleared, List<GimmickReward> Rewards) MarkSequenceClearedOnce(
        DarkUserMemoryDatabase userDb,
        long userId,
        int scheduleId,
        int sequenceId,
        long nowMs)
    {
        EntityIUserGimmickSequence sequence = userDb.EntityIUserGimmickSequence.GetOrCreate(
            entry => entry.GimmickSequenceScheduleId == scheduleId && entry.GimmickSequenceId == sequenceId,
            () => new EntityIUserGimmickSequence
            {
                UserId = userId,
                GimmickSequenceScheduleId = scheduleId,
                GimmickSequenceId = sequenceId
            });

        if (sequence.IsGimmickSequenceCleared)
        {
            return (false, []);
        }

        sequence.IsGimmickSequenceCleared = true;
        sequence.ClearDatetime = nowMs;

        List<GimmickReward> rewards = [];
        EntityMGimmickSequence? masterSequence = _masterDb.EntityMGimmickSequence
            .FirstOrDefault(entry => entry.GimmickSequenceId == sequenceId);
        if (masterSequence is null || masterSequence.GimmickSequenceRewardGroupId == 0)
        {
            return (true, rewards);
        }

        foreach (EntityMGimmickSequenceRewardGroup reward in _masterDb.EntityMGimmickSequenceRewardGroup
                     .Where(entry => entry.GimmickSequenceRewardGroupId == masterSequence.GimmickSequenceRewardGroupId)
                     .OrderBy(entry => entry.GroupIndex))
        {
            if (reward.PossessionType == PossessionType.UNKNOWN || reward.PossessionId == 0 || reward.Count == 0)
            {
                continue;
            }

            PossessionHelper.Apply(userDb, reward.PossessionType, reward.PossessionId, reward.Count, _masterDb, nowMs);
            rewards.Add(new GimmickReward
            {
                PossessionType = (int)reward.PossessionType,
                PossessionId = reward.PossessionId,
                Count = reward.Count
            });
        }

        return (true, rewards);
    }

    /// <summary>
    /// The Lost Archive a cage-memory gimmick opens. Master data doesn't link them; the n-th Lost Archive gimmick of a
    /// route (by id) opens the n-th archive of that route's season (owner agreed, 2026-10-26).
    /// </summary>
    private int? ResolveCageMemoryImportantItemId(EntityMGimmick masterGimmick)
    {
        int chapterId = _masterDb.EntityMGimmickOrnament
            .Where(entry => entry.GimmickOrnamentGroupId == masterGimmick.GimmickOrnamentGroupId)
            .Select(entry => entry.ChapterId)
            .FirstOrDefault();
        if (chapterId == 0)
        {
            return null;
        }

        EntityMMainQuestChapter? chapter = _masterDb.EntityMMainQuestChapter
            .FirstOrDefault(entry => entry.MainQuestChapterId == chapterId);
        if (chapter is null)
        {
            return null;
        }

        EntityMMainQuestRoute? route = _masterDb.EntityMMainQuestRoute
            .FirstOrDefault(entry => entry.MainQuestRouteId == chapter.MainQuestRouteId);
        if (route is null)
        {
            return null;
        }

        List<int> routeGimmickIds = [.. _masterDb.EntityMGimmick
            .Where(entry => entry.GimmickType == GimmickType.CAGE_MEMORY)
            .Where(entry => _masterDb.EntityMGimmickOrnament.Any(ornament =>
                ornament.GimmickOrnamentGroupId == entry.GimmickOrnamentGroupId
                && _masterDb.EntityMMainQuestChapter.Any(routeChapter =>
                    routeChapter.MainQuestChapterId == ornament.ChapterId
                    && routeChapter.MainQuestRouteId == route.MainQuestRouteId)))
            .Select(entry => entry.GimmickId)
            .Distinct()
            .OrderBy(entry => entry)];

        int gimmickIndex = routeGimmickIds.IndexOf(masterGimmick.GimmickId);
        if (gimmickIndex < 0)
        {
            return null;
        }

        List<int> seasonCageMemoryIds = [.. _masterDb.EntityMCageMemory
            .Where(entry => entry.MainQuestSeasonId == route.MainQuestSeasonId)
            .Select(entry => entry.CageMemoryId)
            .OrderBy(entry => entry)];
        if (gimmickIndex >= seasonCageMemoryIds.Count)
        {
            return null;
        }

        int cageMemoryId = seasonCageMemoryIds[gimmickIndex];
        return _masterDb.EntityMImportantItem
            .Where(entry => entry.ImportantItemType == 4 && entry.CageMemoryId == cageMemoryId)
            .Select(entry => (int?)entry.ImportantItemId)
            .FirstOrDefault();
    }
}
