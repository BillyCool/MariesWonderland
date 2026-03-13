using MariesWonderland.Proto.ContentsStory;
using Grpc.Core;

namespace MariesWonderland.Services;

public class ContentsStoryService : MariesWonderland.Proto.ContentsStory.ContentsstoryService.ContentsstoryServiceBase
{
    public override Task<RegisterPlayedResponse> RegisterPlayed(RegisterPlayedRequest request, ServerCallContext context)
    {
        return Task.FromResult(new RegisterPlayedResponse());
    }
}
