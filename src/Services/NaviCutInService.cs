using MariesWonderland.Proto.NaviCutIn;
using Grpc.Core;

namespace MariesWonderland.Services;

public class NaviCutInService : MariesWonderland.Proto.NaviCutIn.NavicutinService.NavicutinServiceBase
{
    public override Task<RegisterPlayedResponse> RegisterPlayed(RegisterPlayedRequest request, ServerCallContext context)
    {
        return Task.FromResult(new RegisterPlayedResponse());
    }
}
