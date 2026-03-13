using MariesWonderland.Proto.Gacha;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace MariesWonderland.Services;

public class GachaService : MariesWonderland.Proto.Gacha.GachaService.GachaServiceBase
{
    public override Task<GetGachaListResponse> GetGachaList(GetGachaListRequest request, ServerCallContext context)
    {
        return Task.FromResult(new GetGachaListResponse());
    }

    public override Task<GetGachaResponse> GetGacha(GetGachaRequest request, ServerCallContext context)
    {
        return Task.FromResult(new GetGachaResponse());
    }

    public override Task<DrawResponse> Draw(DrawRequest request, ServerCallContext context)
    {
        return Task.FromResult(new DrawResponse());
    }

    public override Task<ResetBoxGachaResponse> ResetBoxGacha(ResetBoxGachaRequest request, ServerCallContext context)
    {
        return Task.FromResult(new ResetBoxGachaResponse());
    }

    public override Task<GetRewardGachaResponse> GetRewardGacha(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new GetRewardGachaResponse());
    }

    public override Task<RewardDrawResponse> RewardDraw(RewardDrawRequest request, ServerCallContext context)
    {
        return Task.FromResult(new RewardDrawResponse());
    }
}
