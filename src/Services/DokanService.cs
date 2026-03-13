using MariesWonderland.Proto.Dokan;
using Grpc.Core;

namespace MariesWonderland.Services;

public class DokanService : MariesWonderland.Proto.Dokan.DokanService.DokanServiceBase
{
    public override Task<RegisterDokanConfirmedResponse> RegisterDokanConfirmed(RegisterDokanConfirmedRequest request, ServerCallContext context)
    {
        return Task.FromResult(new RegisterDokanConfirmedResponse());
    }
}
