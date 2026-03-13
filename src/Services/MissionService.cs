using MariesWonderland.Proto.Mission;
using Grpc.Core;

namespace MariesWonderland.Services;

public class MissionService : MariesWonderland.Proto.Mission.MissionService.MissionServiceBase
{
    public override Task<ReceiveMissionRewardsResponse> ReceiveMissionRewardsById(ReceiveMissionRewardsByIdRequest request, ServerCallContext context)
    {
        return Task.FromResult(new ReceiveMissionRewardsResponse());
    }

    public override Task<UpdateMissionProgressResponse> UpdateMissionProgress(UpdateMissionProgressRequest request, ServerCallContext context)
    {
        return Task.FromResult(new UpdateMissionProgressResponse());
    }

    public override Task<ReceiveMissionPassRewardsResponse> ReceiveMissionPassRewards(ReceiveMissionPassRewardsRequest request, ServerCallContext context)
    {
        return Task.FromResult(new ReceiveMissionPassRewardsResponse());
    }
}
