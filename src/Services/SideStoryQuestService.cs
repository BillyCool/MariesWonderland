using Grpc.Core;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Models.Entities;
using MariesWonderland.Proto.SideStoryQuest;

namespace MariesWonderland.Services;

public class SideStoryQuestService(UserDataStore store, DarkMasterMemoryDatabase masterDb) : MariesWonderland.Proto.SideStoryQuest.SidestoryquestService.SidestoryquestServiceBase
{
    private readonly UserDataStore _store = store;
    private readonly DarkMasterMemoryDatabase _masterDb = masterDb;

    /// <summary>Activates a side story quest: sets the current scene progress and creates the quest tracking record if new.</summary>
    public override Task<MoveSideStoryQuestResponse> MoveSideStoryQuestProgress(MoveSideStoryQuestRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        int questId = request.SideStoryQuestId;
        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        int firstSceneId = _masterDb.EntityMSideStoryQuestScene
            .Where(s => s.SideStoryQuestId == questId)
            .OrderBy(s => s.SortOrder)
            .Select(s => s.SideStoryQuestSceneId)
            .FirstOrDefault();

        EntityIUserSideStoryQuest? existing = userDb.EntityIUserSideStoryQuest
            .FirstOrDefault(s => s.SideStoryQuestId == questId);

        int sceneId = (existing != null && existing.HeadSideStoryQuestSceneId > 0)
            ? existing.HeadSideStoryQuestSceneId
            : firstSceneId;

        EntityIUserSideStoryQuestSceneProgressStatus activeProgress = userDb.EntityIUserSideStoryQuestSceneProgressStatus
            .FirstOrDefault(s => s.UserId == userId)
            ?? AddEntity(userDb.EntityIUserSideStoryQuestSceneProgressStatus, new EntityIUserSideStoryQuestSceneProgressStatus { UserId = userId });
        activeProgress.CurrentSideStoryQuestId = questId;
        activeProgress.CurrentSideStoryQuestSceneId = sceneId;

        if (existing == null)
        {
            userDb.EntityIUserSideStoryQuest.Add(new EntityIUserSideStoryQuest
            {
                UserId = userId,
                SideStoryQuestId = questId,
                HeadSideStoryQuestSceneId = firstSceneId,
                SideStoryQuestStateType = 1, // Active
            });
        }

        return Task.FromResult(new MoveSideStoryQuestResponse());
    }

    /// <summary>Advances the player's current scene within a side story quest, updating the high-water mark.</summary>
    public override Task<UpdateSideStoryQuestSceneProgressResponse> UpdateSideStoryQuestSceneProgress(UpdateSideStoryQuestSceneProgressRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        int questId = request.SideStoryQuestId;
        int sceneId = request.SideStoryQuestSceneId;
        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        EntityIUserSideStoryQuestSceneProgressStatus activeProgress = userDb.EntityIUserSideStoryQuestSceneProgressStatus
            .FirstOrDefault(s => s.UserId == userId)
            ?? AddEntity(userDb.EntityIUserSideStoryQuestSceneProgressStatus, new EntityIUserSideStoryQuestSceneProgressStatus { UserId = userId });
        activeProgress.CurrentSideStoryQuestSceneId = sceneId;

        EntityIUserSideStoryQuest? progress = userDb.EntityIUserSideStoryQuest
            .FirstOrDefault(s => s.SideStoryQuestId == questId);
        if (progress != null)
        {
            if (sceneId > progress.HeadSideStoryQuestSceneId)
            {
                progress.HeadSideStoryQuestSceneId = sceneId;
            }
        }

        return Task.FromResult(new UpdateSideStoryQuestSceneProgressResponse());
    }

    /// <summary>Adds an entity to a list and returns it, enabling inline initialization with null-coalescing.</summary>
    private static T AddEntity<T>(List<T> list, T entity)
    {
        list.Add(entity);
        return entity;
    }
}
