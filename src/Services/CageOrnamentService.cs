using MariesWonderland.Proto.CageOrnament;
using Grpc.Core;

namespace MariesWonderland.Services;

public class CageOrnamentService : MariesWonderland.Proto.CageOrnament.CageornamentService.CageornamentServiceBase
{
    public override Task<ReceiveRewardResponse> ReceiveReward(ReceiveRewardRequest request, ServerCallContext context)
    {
        return Task.FromResult(new ReceiveRewardResponse());
    }

    public override Task<RecordAccessResponse> RecordAccess(RecordAccessRequest request, ServerCallContext context)
    {
        return Task.FromResult(new RecordAccessResponse());
    }
}
