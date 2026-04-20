using Grpc.Core;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Proto.Mission;

namespace MariesWonderland.Services;

public class MissionService(UserDataStore store)
    : MariesWonderland.Proto.Mission.MissionService.MissionServiceBase
{
    private readonly UserDataStore _store = store;

    /// <summary>Returns an empty response. Mission progress tracking not yet implemented.</summary>
    public override Task<UpdateMissionProgressResponse> UpdateMissionProgress(UpdateMissionProgressRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        _ = _store.GetOrCreate(userId);

        return Task.FromResult(new UpdateMissionProgressResponse());
    }

    /// <summary>Returns an empty response. Mission reward claiming not yet implemented.</summary>
    public override Task<ReceiveMissionRewardsResponse> ReceiveMissionRewardsById(ReceiveMissionRewardsByIdRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        _ = _store.GetOrCreate(userId);

        return Task.FromResult(new ReceiveMissionRewardsResponse());
    }

    /// <summary>Returns an empty response. Mission pass rewards not yet implemented.</summary>
    public override Task<ReceiveMissionPassRewardsResponse> ReceiveMissionPassRewards(ReceiveMissionPassRewardsRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        _ = _store.GetOrCreate(userId);

        return Task.FromResult(new ReceiveMissionPassRewardsResponse());
    }
}
