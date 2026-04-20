using Grpc.Core;
using MariesWonderland.Proto.Labyrinth;

namespace MariesWonderland.Services;

public class LabyrinthService : MariesWonderland.Proto.Labyrinth.LabyrinthService.LabyrinthServiceBase
{
    /// <summary>Returns an empty response. Labyrinth season data not yet implemented.</summary>
    public override Task<UpdateSeasonDataResponse> UpdateSeasonData(UpdateSeasonDataRequest request, ServerCallContext context)
    {
        return Task.FromResult(new UpdateSeasonDataResponse());
    }

    /// <summary>Returns an empty response. Labyrinth stage clear rewards not yet implemented.</summary>
    public override Task<ReceiveStageClearRewardResponse> ReceiveStageClearReward(ReceiveStageClearRewardRequest request, ServerCallContext context)
    {
        return Task.FromResult(new ReceiveStageClearRewardResponse());
    }

    /// <summary>Returns an empty response. Labyrinth accumulation rewards not yet implemented.</summary>
    public override Task<ReceiveStageAccumulationRewardResponse> ReceiveStageAccumulationReward(ReceiveStageAccumulationRewardRequest request, ServerCallContext context)
    {
        return Task.FromResult(new ReceiveStageAccumulationRewardResponse());
    }
}
