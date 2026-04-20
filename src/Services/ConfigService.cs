using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MariesWonderland.Proto.Config;

namespace MariesWonderland.Services;

public class ConfigService : MariesWonderland.Proto.Config.ConfigService.ConfigServiceBase
{
    /// <summary>Returns an empty response. Review server configuration not yet implemented.</summary>
    public override Task<GetReviewServerConfigResponse> GetReviewServerConfig(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new GetReviewServerConfigResponse());
    }
}
