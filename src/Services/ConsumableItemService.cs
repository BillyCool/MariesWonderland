using MariesWonderland.Proto.ConsumableItem;
using Grpc.Core;

namespace MariesWonderland.Services;

public class ConsumableItemService : MariesWonderland.Proto.ConsumableItem.ConsumableitemService.ConsumableitemServiceBase
{
    public override Task<UseEffectItemResponse> UseEffectItem(UseEffectItemRequest request, ServerCallContext context)
    {
        return Task.FromResult(new UseEffectItemResponse());
    }

    public override Task<SellResponse> Sell(SellRequest request, ServerCallContext context)
    {
        return Task.FromResult(new SellResponse());
    }
}
