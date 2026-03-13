using MariesWonderland.Proto.Material;
using Grpc.Core;

namespace MariesWonderland.Services;

public class MaterialService : MariesWonderland.Proto.Material.MaterialService.MaterialServiceBase
{
    public override Task<SellResponse> Sell(SellRequest request, ServerCallContext context)
    {
        return Task.FromResult(new SellResponse());
    }
}
