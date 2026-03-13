using MariesWonderland.Proto.Labyrinth;
using Grpc.Core;

namespace MariesWonderland.Services;

public class LabyrinthService : MariesWonderland.Proto.Labyrinth.LabyrinthService.LabyrinthServiceBase
{
    public override Task<UpdateSeasonDataResponse> UpdateSeasonData(UpdateSeasonDataRequest request, ServerCallContext context)
    {
        return Task.FromResult(new UpdateSeasonDataResponse());
    }

    public override Task<ReceiveStageClearRewardResponse> ReceiveStageClearReward(ReceiveStageClearRewardRequest request, ServerCallContext context)
    {
        return Task.FromResult(new ReceiveStageClearRewardResponse());
    }

    public override Task<ReceiveStageAccumulationRewardResponse> ReceiveStageAccumulationReward(ReceiveStageAccumulationRewardRequest request, ServerCallContext context)
    {
        return Task.FromResult(new ReceiveStageAccumulationRewardResponse());
    }
}
