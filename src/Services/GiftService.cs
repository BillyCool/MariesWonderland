using MariesWonderland.Proto.Gift;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace MariesWonderland.Services;

public class GiftService : MariesWonderland.Proto.Gift.GiftService.GiftServiceBase
{
    public override Task<ReceiveGiftResponse> ReceiveGift(ReceiveGiftRequest request, ServerCallContext context)
    {
        return Task.FromResult(new ReceiveGiftResponse());
    }

    public override Task<GetGiftListResponse> GetGiftList(GetGiftListRequest request, ServerCallContext context)
    {
        return Task.FromResult(new GetGiftListResponse());
    }

    public override Task<GetGiftReceiveHistoryListResponse> GetGiftReceiveHistoryList(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new GetGiftReceiveHistoryListResponse());
    }
}
