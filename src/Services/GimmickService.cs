using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Models.Entities;
using MariesWonderland.Proto.Gimmick;

namespace MariesWonderland.Services;

public class GimmickService(UserDataStore store) : MariesWonderland.Proto.Gimmick.GimmickService.GimmickServiceBase
{
    private readonly UserDataStore _store = store;

    public override Task<UpdateSequenceResponse> UpdateSequence(UpdateSequenceRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);
        Dictionary<string, string> before = UserDataDiffBuilder.Snapshot(userDb);

        // Ensure the sequence row exists so the client can track it
        if (!userDb.EntityIUserGimmickSequence.Any(s =>
            s.UserId == userId &&
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

        UpdateSequenceResponse response = new();
        foreach (var (k, v) in UserDataDiffBuilder.Delta(before, userDb)) response.DiffUserData[k] = v;

        return Task.FromResult(response);
    }

    public override Task<UpdateGimmickProgressResponse> UpdateGimmickProgress(UpdateGimmickProgressRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);
        Dictionary<string, string> before = UserDataDiffBuilder.Snapshot(userDb);

        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        // Upsert the base gimmick progress row
        EntityIUserGimmick? gimmick = userDb.EntityIUserGimmick.FirstOrDefault(g =>
            g.UserId == userId &&
            g.GimmickSequenceScheduleId == request.GimmickSequenceScheduleId &&
            g.GimmickSequenceId == request.GimmickSequenceId &&
            g.GimmickId == request.GimmickId);

        if (gimmick == null)
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

        // Upsert the ornament progress row, recording the progress bit from the client
        EntityIUserGimmickOrnamentProgress? ornament = userDb.EntityIUserGimmickOrnamentProgress.FirstOrDefault(o =>
            o.UserId == userId &&
            o.GimmickSequenceScheduleId == request.GimmickSequenceScheduleId &&
            o.GimmickSequenceId == request.GimmickSequenceId &&
            o.GimmickId == request.GimmickId &&
            o.GimmickOrnamentIndex == request.GimmickOrnamentIndex);

        if (ornament == null)
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

        UpdateGimmickProgressResponse response = new()
        {
            IsSequenceCleared = false,
        };
        foreach (var (k, v) in UserDataDiffBuilder.Delta(before, userDb)) response.DiffUserData[k] = v;

        return Task.FromResult(response);
    }

    public override Task<InitSequenceScheduleResponse> InitSequenceSchedule(Empty request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);
        Dictionary<string, string> before = UserDataDiffBuilder.Snapshot(userDb);

        // Read-only — returns all gimmick tables so the client can initialise its state
        InitSequenceScheduleResponse response = new();
        foreach (var (k, v) in UserDataDiffBuilder.Delta(before, userDb)) response.DiffUserData[k] = v;
        return Task.FromResult(response);
    }

    public override Task<UnlockResponse> Unlock(UnlockRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);
        Dictionary<string, string> before = UserDataDiffBuilder.Snapshot(userDb);

        foreach (GimmickKey key in request.GimmickKey)
        {
            EntityIUserGimmickUnlock? unlock = userDb.EntityIUserGimmickUnlock.FirstOrDefault(u =>
                u.UserId == userId &&
                u.GimmickSequenceScheduleId == key.GimmickSequenceScheduleId &&
                u.GimmickSequenceId == key.GimmickSequenceId &&
                u.GimmickId == key.GimmickId);
            if (unlock == null)
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

        UnlockResponse response = new();
        foreach (var (k, v) in UserDataDiffBuilder.Delta(before, userDb)) response.DiffUserData[k] = v;

        return Task.FromResult(response);
    }
}
