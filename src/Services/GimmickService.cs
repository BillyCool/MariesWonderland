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

public class GimmickService(UserDataStore store, DarkMasterMemoryDatabase masterDb, MissionTracker missions, TimeProvider timeProvider)
    : MariesWonderland.Proto.Gimmick.GimmickService.GimmickServiceBase
{
    private readonly UserDataStore _store = store;
    private readonly DarkMasterMemoryDatabase _masterDb = masterDb;
    private readonly MissionTracker _missions = missions;
    private readonly TimeProvider _timeProvider = timeProvider;

    /// <summary>Records that the user has advanced to a gimmick sequence, preventing duplicate entries.</summary>
    public override Task<UpdateSequenceResponse> UpdateSequence(UpdateSequenceRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

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

        return Task.FromResult(new UpdateSequenceResponse());
    }

    /// <summary>Updates progress on a field gimmick and its ornament interaction state.</summary>
    public override Task<UpdateGimmickProgressResponse> UpdateGimmickProgress(UpdateGimmickProgressRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);
        long nowMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();
        List<GimmickReward> ornamentRewards = [];
        List<GimmickReward> sequenceClearRewards = [];
        bool isSequenceCleared = false;

        EntityIUserGimmick? gimmick = userDb.EntityIUserGimmick.FirstOrDefault(g =>
            g.GimmickSequenceScheduleId == request.GimmickSequenceScheduleId &&
            g.GimmickSequenceId == request.GimmickSequenceId &&
            g.GimmickId == request.GimmickId);

        if (gimmick is null)
        {
            gimmick = new EntityIUserGimmick
            {
                UserId = userId,
                GimmickSequenceScheduleId = request.GimmickSequenceScheduleId,
                GimmickSequenceId = request.GimmickSequenceId,
                GimmickId = request.GimmickId,
            };
            userDb.EntityIUserGimmick.Add(gimmick);
        }
        gimmick.StartDatetime = nowMs;

        EntityIUserGimmickOrnamentProgress? ornament = userDb.EntityIUserGimmickOrnamentProgress.FirstOrDefault(o =>
            o.GimmickSequenceScheduleId == request.GimmickSequenceScheduleId &&
            o.GimmickSequenceId == request.GimmickSequenceId &&
            o.GimmickId == request.GimmickId &&
            o.GimmickOrnamentIndex == request.GimmickOrnamentIndex);

        if (ornament is null)
        {
            ornament = new EntityIUserGimmickOrnamentProgress
            {
                UserId = userId,
                GimmickSequenceScheduleId = request.GimmickSequenceScheduleId,
                GimmickSequenceId = request.GimmickSequenceId,
                GimmickId = request.GimmickId,
                GimmickOrnamentIndex = request.GimmickOrnamentIndex,
            };
            userDb.EntityIUserGimmickOrnamentProgress.Add(ornament);
        }
        ornament.ProgressValueBit = request.ProgressValueBit;
        ornament.BaseDatetime = nowMs;

        EntityMGimmick? masterGimmick = _masterDb.EntityMGimmick.FirstOrDefault(g => g.GimmickId == request.GimmickId);
        if (masterGimmick is not null)
        {
            switch (masterGimmick.GimmickType)
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

                case GimmickType.MAP_ONLY_CAGE_TREASURE_HUNT:
                    GimmickReward? hiddenBirdReward = ResolveHiddenBirdReward(masterGimmick.GimmickOrnamentGroupId, request.GimmickOrnamentIndex);
                    if (hiddenBirdReward is not null)
                    {
                        PossessionHelper.Apply(
                            userDb,
                            (PossessionType)hiddenBirdReward.PossessionType,
                            hiddenBirdReward.PossessionId,
                            hiddenBirdReward.Count,
                            _masterDb, nowMs);
                        ornamentRewards.Add(hiddenBirdReward);
                    }

                    gimmick.IsGimmickCleared = true;
                    (isSequenceCleared, sequenceClearRewards) = MarkSequenceClearedOnce(
                        userDb,
                        userId,
                        request.GimmickSequenceScheduleId,
                        request.GimmickSequenceId,
                        nowMs);
                    break;

                case GimmickType.CAGE_MEMORY:
                    int? importantItemId = ResolveCageMemoryImportantItemId(masterGimmick);
                    if (importantItemId is int itemId
                        && !userDb.EntityIUserImportantItem.Any(item => item.ImportantItemId == itemId))
                    {
                        PossessionHelper.Apply(userDb, PossessionType.IMPORTANT_ITEM, itemId, 1, _masterDb, nowMs);
                        ornamentRewards.Add(new GimmickReward
                        {
                            PossessionType = (int)PossessionType.IMPORTANT_ITEM,
                            PossessionId = itemId,
                            Count = 1
                        });
                    }
                    break;
            }
        }

        // A cleared report sequence grants its hidden story (a report important item): "Collect N Hidden Stories".
        if (sequenceClearRewards.Count > 0 || ornamentRewards.Count > 0)
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
    /// Initializes gimmick sequence schedules whose time windows are active and whose release condition is met
    /// (in master data always a quest clear, hidden-story reports and cage memories included).
    /// </summary>
    public override Task<InitSequenceScheduleResponse> InitSequenceSchedule(Empty request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);
        long nowMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();

        List<EntityMGimmickSequenceSchedule> activeSchedules = ResolveActiveSchedules(nowMs);

        userDb.EntityIUserGimmickSequence.RemoveAll(sequence =>
        {
            if (sequence.IsGimmickSequenceCleared)
            {
                return false;
            }

            return !activeSchedules.Any(schedule =>
                schedule.GimmickSequenceScheduleId == sequence.GimmickSequenceScheduleId &&
                schedule.FirstGimmickSequenceId == sequence.GimmickSequenceId);
        });

        // Activate each schedule whose time window includes now and whose release condition is met. An unsupported
        // condition doesn't block (none exist in master data today).
        foreach (EntityMGimmickSequenceSchedule schedule in activeSchedules)
        {
            if (nowMs < schedule.StartDatetime)
            {
                continue;
            }

            if (schedule.ReleaseEvaluateConditionId != 0
                && EvaluateConditionHelper.Evaluate(schedule.ReleaseEvaluateConditionId, userDb, _masterDb) == false)
            {
                continue;
            }

            bool exists = userDb.EntityIUserGimmickSequence.Any(s =>
                s.GimmickSequenceScheduleId == schedule.GimmickSequenceScheduleId &&
                s.GimmickSequenceId == schedule.FirstGimmickSequenceId);

            if (!exists)
            {
                userDb.EntityIUserGimmickSequence.Add(new EntityIUserGimmickSequence
                {
                    UserId = userId,
                    GimmickSequenceScheduleId = schedule.GimmickSequenceScheduleId,
                    GimmickSequenceId = schedule.FirstGimmickSequenceId
                });
            }
        }

        return Task.FromResult(new InitSequenceScheduleResponse());
    }

    /// <summary>Marks one or more gimmicks as unlocked so the player can interact with them in the field.</summary>
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

    private List<EntityMGimmickSequenceSchedule> ResolveActiveSchedules(long nowMs)
    {
        Dictionary<int, EntityMGimmickSequenceSchedule> bestByFirstSequence = [];
        foreach (EntityMGimmickSequenceSchedule schedule in _masterDb.EntityMGimmickSequenceSchedule)
        {
            if (bestByFirstSequence.TryGetValue(schedule.FirstGimmickSequenceId, out EntityMGimmickSequenceSchedule? existing))
            {
                bool existingHasFutureEnd = existing.EndDatetime > nowMs;
                bool candidateHasFutureEnd = schedule.EndDatetime > nowMs;

                if (existingHasFutureEnd != candidateHasFutureEnd)
                {
                    if (existingHasFutureEnd)
                    {
                        continue;
                    }
                }
                else if (existing.StartDatetime < schedule.StartDatetime
                    || (existing.StartDatetime == schedule.StartDatetime
                        && existing.GimmickSequenceScheduleId <= schedule.GimmickSequenceScheduleId))
                {
                    continue;
                }
            }

            bestByFirstSequence[schedule.FirstGimmickSequenceId] = schedule;
        }

        return [.. bestByFirstSequence.Values];
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

    private GimmickReward? ResolveHiddenBirdReward(int gimmickOrnamentGroupId, int gimmickOrnamentIndex)
    {
        EntityMGimmickOrnament? ornament = _masterDb.EntityMGimmickOrnament.FirstOrDefault(entry =>
            entry.GimmickOrnamentGroupId == gimmickOrnamentGroupId
            && entry.GimmickOrnamentIndex == gimmickOrnamentIndex);
        if (ornament is null)
        {
            return null;
        }

        EntityMCageOrnament? cageOrnament = _masterDb.EntityMCageOrnament
            .FirstOrDefault(entry => entry.CageOrnamentId == ornament.GimmickOrnamentViewId);
        if (cageOrnament is null)
        {
            return null;
        }

        EntityMCageOrnamentReward? reward = _masterDb.EntityMCageOrnamentReward
            .FirstOrDefault(entry => entry.CageOrnamentRewardId == cageOrnament.CageOrnamentRewardId);
        if (reward is null)
        {
            return null;
        }

        return new GimmickReward
        {
            PossessionType = (int)reward.PossessionType,
            PossessionId = reward.PossessionId,
            Count = reward.Count
        };
    }

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
