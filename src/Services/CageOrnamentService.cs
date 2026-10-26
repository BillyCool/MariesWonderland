using Grpc.Core;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Helpers;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.CageOrnament;

namespace MariesWonderland.Services;

public class CageOrnamentService(UserDataStore store, DarkMasterMemoryDatabase masterDb, TimeProvider timeProvider)
    : MariesWonderland.Proto.CageOrnament.CageornamentService.CageornamentServiceBase
{
    private readonly UserDataStore _store = store;
    private readonly DarkMasterMemoryDatabase _masterDb = masterDb;
    private readonly TimeProvider _timeProvider = timeProvider;

    /// <summary>
    /// Dispatches a black bird: records it, grants its fixed reward and returns it. Only the first claim grants.
    /// </summary>
    public override Task<ReceiveRewardResponse> ReceiveReward(ReceiveRewardRequest request, ServerCallContext context)
    {
        EntityMCageOrnament? ornament = _masterDb.EntityMCageOrnament
            .FirstOrDefault(o => o.CageOrnamentId == request.CageOrnamentId);

        if (ornament is null)
        {
            return Task.FromResult(new ReceiveRewardResponse());
        }

        EntityMCageOrnamentReward? reward = _masterDb.EntityMCageOrnamentReward
            .FirstOrDefault(r => r.CageOrnamentRewardId == ornament.CageOrnamentRewardId);

        if (reward is null)
        {
            return Task.FromResult(new ReceiveRewardResponse());
        }

        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);
        long nowMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();

        // A black bird is dispatched once: its reward row hides it, and a repeat call grants nothing.
        if (userDb.EntityIUserCageOrnamentReward.Any(r => r.CageOrnamentId == request.CageOrnamentId))
        {
            return Task.FromResult(new ReceiveRewardResponse());
        }

        userDb.EntityIUserCageOrnamentReward.Add(new EntityIUserCageOrnamentReward
        {
            UserId = userId,
            CageOrnamentId = request.CageOrnamentId,
            AcquisitionDatetime = nowMs
        });

        PossessionHelper.Apply(userDb, reward.PossessionType, reward.PossessionId, reward.Count, _masterDb, nowMs);

        ReceiveRewardResponse response = new();
        response.CageOrnamentReward.Add(new CageOrnamentReward
        {
            PossessionType = (int)reward.PossessionType,
            PossessionId = reward.PossessionId,
            Count = reward.Count
        });

        return Task.FromResult(response);
    }

    /// <summary>
    /// Records that the user has accessed this cage ornament, creating an access entry if one does not yet exist.
    /// </summary>
    public override Task<RecordAccessResponse> RecordAccess(RecordAccessRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);
        long nowMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();

        // Create access record if it doesn't exist
        bool exists = userDb.EntityIUserCageOrnamentReward
            .Any(r => r.CageOrnamentId == request.CageOrnamentId);

        if (!exists)
        {
            userDb.EntityIUserCageOrnamentReward.Add(new EntityIUserCageOrnamentReward
            {
                UserId = userId,
                CageOrnamentId = request.CageOrnamentId,
                AcquisitionDatetime = nowMs
            });
        }

        return Task.FromResult(new RecordAccessResponse());
    }
}