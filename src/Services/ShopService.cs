using MariesWonderland.Proto.Shop;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace MariesWonderland.Services;

public class ShopService : MariesWonderland.Proto.Shop.ShopService.ShopServiceBase
{
    public override Task<BuyResponse> Buy(BuyRequest request, ServerCallContext context)
    {
        return Task.FromResult(new BuyResponse());
    }

    public override Task<RefreshResponse> RefreshUserData(RefreshRequest request, ServerCallContext context)
    {
        return Task.FromResult(new RefreshResponse());
    }

    public override Task<GetCesaLimitResponse> GetCesaLimit(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new GetCesaLimitResponse());
    }

    public override Task<CreatePurchaseTransactionResponse> CreatePurchaseTransaction(CreatePurchaseTransactionRequest request, ServerCallContext context)
    {
        return Task.FromResult(new CreatePurchaseTransactionResponse());
    }

    public override Task<PurchaseGooglePlayStoreProductResponse> PurchaseGooglePlayStoreProduct(PurchaseGooglePlayStoreProductRequest request, ServerCallContext context)
    {
        return Task.FromResult(new PurchaseGooglePlayStoreProductResponse());
    }
}
