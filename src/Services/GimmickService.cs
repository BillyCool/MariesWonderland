using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Gimmick;

namespace MariesWonderland.Services;

public class GimmickService(UserDataStore store, DarkMasterMemoryDatabase masterDb)
    : MariesWonderland.Proto.Gimmick.GimmickService.GimmickServiceBase
{
    private readonly UserDataStore _store = store;
    private readonly DarkMasterMemoryDatabase _masterDb = masterDb;

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
        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

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

        return Task.FromResult(new UpdateGimmickProgressResponse { IsSequenceCleared = false });
    }

    /// <summary>Initializes gimmick sequence schedules whose time windows are active and quest prerequisites are met.</summary>
    public override Task<InitSequenceScheduleResponse> InitSequenceSchedule(Empty request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);
        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        Dictionary<int, long> conditionToQuestId = BuildConditionQuestMap();

        // Collect all cleared quest IDs for prerequisite checks
        HashSet<int> clearedQuests = [];
        foreach (EntityIUserQuest quest in userDb.EntityIUserQuest)
        {
            if (quest.QuestStateType == (int)QuestStateType.CLEARED)
            {
                clearedQuests.Add(quest.QuestId);
            }
        }

        // Activate each schedule whose time window includes now and whose quest prerequisite is met
        foreach (EntityMGimmickSequenceSchedule schedule in _masterDb.EntityMGimmickSequenceSchedule)
        {
            if (nowMs < schedule.StartDatetime || nowMs > schedule.EndDatetime)
            {
                continue;
            }

            if (schedule.ReleaseEvaluateConditionId != 0)
            {
                if (conditionToQuestId.TryGetValue(schedule.ReleaseEvaluateConditionId, out long requiredQuestId)
                    && !clearedQuests.Contains((int)requiredQuestId))
                {
                    continue;
                }
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
                    GimmickSequenceId = schedule.FirstGimmickSequenceId,
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

    /// <summary>Builds a mapping from EvaluateConditionId to required quest ID for QUEST_CLEAR conditions.</summary>
    private Dictionary<int, long> BuildConditionQuestMap()    {
        Dictionary<(int GroupId, int GroupIndex), long> vgByKey = [];
        foreach (EntityMEvaluateConditionValueGroup vg in _masterDb.EntityMEvaluateConditionValueGroup)
        {
            vgByKey[(vg.EvaluateConditionValueGroupId, vg.GroupIndex)] = vg.Value;
        }

        Dictionary<int, long> conditionToQuestId = [];
        foreach (EntityMEvaluateCondition cond in _masterDb.EntityMEvaluateCondition)
        {
            if (cond.EvaluateConditionFunctionType != EvaluateConditionFunctionType.QUEST_CLEAR)
            {
                continue;
            }
            if (cond.EvaluateConditionEvaluateType != EvaluateConditionEvaluateType.ID_CONTAIN)
            {
                continue;
            }
            if (vgByKey.TryGetValue((cond.EvaluateConditionValueGroupId, 1), out long questId))
            {
                conditionToQuestId[cond.EvaluateConditionId] = questId;
            }
        }

        return conditionToQuestId;
    }
}
