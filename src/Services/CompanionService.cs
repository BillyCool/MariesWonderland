using MariesWonderland.Proto.Companion;
using Grpc.Core;

namespace MariesWonderland.Services;

public class CompanionService : MariesWonderland.Proto.Companion.CompanionService.CompanionServiceBase
{
    public override Task<EnhanceResponse> Enhance(EnhanceRequest request, ServerCallContext context)
    {
        return Task.FromResult(new EnhanceResponse());
    }
}
