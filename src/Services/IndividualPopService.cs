using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MariesWonderland.Proto.IndividualPop;

namespace MariesWonderland.Services;

public class IndividualPopService : MariesWonderland.Proto.IndividualPop.IndividualpopService.IndividualpopServiceBase
{
    public override Task<GetUnreadPopResponse> GetUnreadPop(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new GetUnreadPopResponse());
    }
}
