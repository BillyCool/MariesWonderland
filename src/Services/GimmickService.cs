using MariesWonderland.Proto.Gimmick;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace MariesWonderland.Services;

public class GimmickService : MariesWonderland.Proto.Gimmick.GimmickService.GimmickServiceBase
{
    public override Task<UpdateSequenceResponse> UpdateSequence(UpdateSequenceRequest request, ServerCallContext context)
    {
        return Task.FromResult(new UpdateSequenceResponse());
    }

    public override Task<UpdateGimmickProgressResponse> UpdateGimmickProgress(UpdateGimmickProgressRequest request, ServerCallContext context)
    {
        return Task.FromResult(new UpdateGimmickProgressResponse());
    }

    public override Task<InitSequenceScheduleResponse> InitSequenceSchedule(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new InitSequenceScheduleResponse());
    }

    public override Task<UnlockResponse> Unlock(UnlockRequest request, ServerCallContext context)
    {
        return Task.FromResult(new UnlockResponse());
    }
}
