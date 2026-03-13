using MariesWonderland.Proto.IndividualPop;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace MariesWonderland.Services;

public class IndividualPopService : MariesWonderland.Proto.IndividualPop.IndividualpopService.IndividualpopServiceBase
{
    public override Task<GetUnreadPopResponse> GetUnreadPop(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new GetUnreadPopResponse());
    }
}
