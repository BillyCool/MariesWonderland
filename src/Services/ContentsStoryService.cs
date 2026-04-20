using Grpc.Core;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Models.Entities;
using MariesWonderland.Proto.ContentsStory;

namespace MariesWonderland.Services;

public class ContentsStoryService(UserDataStore store)
    : MariesWonderland.Proto.ContentsStory.ContentsstoryService.ContentsstoryServiceBase
{
    private readonly UserDataStore _store = store;

    /// <summary>Records that a contents story entry has been played, tracking the play timestamp.</summary>
    public override Task<RegisterPlayedResponse> RegisterPlayed(RegisterPlayedRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);
        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        EntityIUserContentsStory? existing = userDb.EntityIUserContentsStory
            .FirstOrDefault(s => s.ContentsStoryId == request.ContentsStoryId);

        if (existing is null)
        {
            userDb.EntityIUserContentsStory.Add(new EntityIUserContentsStory
            {
                UserId = userId,
                ContentsStoryId = request.ContentsStoryId,
                PlayDatetime = nowMs
            });
        }
        else
        {
            existing.PlayDatetime = nowMs;
        }

        return Task.FromResult(new RegisterPlayedResponse());
    }
}
