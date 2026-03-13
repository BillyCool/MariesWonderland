using MariesWonderland.Proto.Tutorial;
using Grpc.Core;

namespace MariesWonderland.Services;

public class TutorialService : MariesWonderland.Proto.Tutorial.TutorialService.TutorialServiceBase
{
    public override Task<SetTutorialProgressResponse> SetTutorialProgress(SetTutorialProgressRequest request, ServerCallContext context)
    {
        return Task.FromResult(new SetTutorialProgressResponse());
    }

    public override Task<SetTutorialProgressAndReplaceDeckResponse> SetTutorialProgressAndReplaceDeck(SetTutorialProgressAndReplaceDeckRequest request, ServerCallContext context)
    {
        return Task.FromResult(new SetTutorialProgressAndReplaceDeckResponse());
    }
}
