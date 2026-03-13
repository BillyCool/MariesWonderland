using MariesWonderland.Proto.Reward;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace MariesWonderland.Services;

public class RewardService : MariesWonderland.Proto.Reward.RewardService.RewardServiceBase
{
    public override Task<ReceivePvpRewardResponse> ReceivePvpReward(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new ReceivePvpRewardResponse());
    }

    public override Task<ReceiveBigHuntRewardResponse> ReceiveBigHuntReward(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new ReceiveBigHuntRewardResponse());
    }

    public override Task<ReceiveLabyrinthSeasonRewardResponse> ReceiveLabyrinthSeasonReward(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new ReceiveLabyrinthSeasonRewardResponse());
    }

    public override Task<ReceiveMissionPassRemainingRewardResponse> ReceiveMissionPassRemainingReward(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new ReceiveMissionPassRemainingRewardResponse());
    }
}
