using MariesWonderland.Proto.Omikuji;
using Grpc.Core;

namespace MariesWonderland.Services;

public class OmikujiService : MariesWonderland.Proto.Omikuji.OmikujiService.OmikujiServiceBase
{
    public override Task<OmikujiDrawResponse> OmikujiDraw(OmikujiDrawRequest request, ServerCallContext context)
    {
        return Task.FromResult(new OmikujiDrawResponse());
    }
}
