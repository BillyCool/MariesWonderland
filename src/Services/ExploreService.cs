using MariesWonderland.Proto.Explore;
using Grpc.Core;

namespace MariesWonderland.Services;

public class ExploreService : MariesWonderland.Proto.Explore.ExploreService.ExploreServiceBase
{
    public override Task<StartExploreResponse> StartExplore(StartExploreRequest request, ServerCallContext context)
    {
        return Task.FromResult(new StartExploreResponse());
    }

    public override Task<FinishExploreResponse> FinishExplore(FinishExploreRequest request, ServerCallContext context)
    {
        return Task.FromResult(new FinishExploreResponse());
    }

    public override Task<RetireExploreResponse> RetireExplore(RetireExploreRequest request, ServerCallContext context)
    {
        return Task.FromResult(new RetireExploreResponse());
    }
}
