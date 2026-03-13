using MariesWonderland.Proto.PortalCage;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace MariesWonderland.Services;

public class PortalCageService : MariesWonderland.Proto.PortalCage.PortalcageService.PortalcageServiceBase
{
    public override Task<UpdatePortalCageSceneProgressResponse> UpdatePortalCageSceneProgress(UpdatePortalCageSceneProgressRequest request, ServerCallContext context)
    {
        return Task.FromResult(new UpdatePortalCageSceneProgressResponse());
    }

    public override Task<GetDropItemResponse> GetDropItem(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new GetDropItemResponse());
    }
}
