using Art.Framework.ApiNetwork.Grpc.Api.Config;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace MariesWonderland.Services;

public class ConfigService : Art.Framework.ApiNetwork.Grpc.Api.Config.ConfigService.ConfigServiceBase
{
    public override Task<GetReviewServerConfigResponse> GetReviewServerConfig(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new GetReviewServerConfigResponse());
    }
}
