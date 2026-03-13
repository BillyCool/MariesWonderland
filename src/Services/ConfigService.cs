using MariesWonderland.Proto.Config;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace MariesWonderland.Services;

public class ConfigService : MariesWonderland.Proto.Config.ConfigService.ConfigServiceBase
{
    public override Task<GetReviewServerConfigResponse> GetReviewServerConfig(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new GetReviewServerConfigResponse());
    }
}
