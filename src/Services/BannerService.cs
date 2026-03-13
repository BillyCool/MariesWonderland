using MariesWonderland.Proto.Banner;
using Grpc.Core;

namespace MariesWonderland.Services;

public class BannerService : MariesWonderland.Proto.Banner.BannerService.BannerServiceBase
{
    public override Task<GetMamaBannerResponse> GetMamaBanner(GetMamaBannerRequest request, ServerCallContext context)
    {
        return Task.FromResult(new GetMamaBannerResponse());
    }
}
