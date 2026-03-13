using MariesWonderland.Proto.SideStoryQuest;
using Grpc.Core;

namespace MariesWonderland.Services;

public class SideStoryQuestService : MariesWonderland.Proto.SideStoryQuest.SidestoryquestService.SidestoryquestServiceBase
{
    public override Task<MoveSideStoryQuestResponse> MoveSideStoryQuestProgress(MoveSideStoryQuestRequest request, ServerCallContext context)
    {
        return Task.FromResult(new MoveSideStoryQuestResponse());
    }

    public override Task<UpdateSideStoryQuestSceneProgressResponse> UpdateSideStoryQuestSceneProgress(UpdateSideStoryQuestSceneProgressRequest request, ServerCallContext context)
    {
        return Task.FromResult(new UpdateSideStoryQuestSceneProgressResponse());
    }
}
