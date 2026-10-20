using Grpc.Core;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Helpers;
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
        if (questId <= 0)
        {
            ClearActiveProgress(userDb, userId);
            return Task.FromResult(new MoveSideStoryQuestResponse());
        }

        int firstSceneId = GetFirstSceneId(questId);

        EntityIUserSideStoryQuest? existing = userDb.EntityIUserSideStoryQuest
            .FirstOrDefault(s => s.SideStoryQuestId == questId);

        int sceneId = ResolveCurrentSceneId(existing, questId, firstSceneId);

        EntityIUserSideStoryQuestSceneProgressStatus activeProgress = userDb.EntityIUserSideStoryQuestSceneProgressStatus.GetOrCreate(userId);
        activeProgress.CurrentSideStoryQuestId = questId;
        activeProgress.CurrentSideStoryQuestSceneId = sceneId;

        EntityIUserPortalCageStatus? portalCageStatus = userDb.EntityIUserPortalCageStatus
            .FirstOrDefault(s => s.UserId == userId);
        if (portalCageStatus != null)
            portalCageStatus.IsCurrentProgress = false;

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
        else
        {
            existing.HeadSideStoryQuestSceneId = sceneId;
            if (existing.SideStoryQuestStateType == 0)
                existing.SideStoryQuestStateType = 1;
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

        EntityIUserSideStoryQuestSceneProgressStatus activeProgress = userDb.EntityIUserSideStoryQuestSceneProgressStatus.GetOrCreate(userId);
        activeProgress.CurrentSideStoryQuestId = questId;
        activeProgress.CurrentSideStoryQuestSceneId = sceneId;

        EntityIUserSideStoryQuest? progress = userDb.EntityIUserSideStoryQuest
            .FirstOrDefault(s => s.SideStoryQuestId == questId);
        if (progress != null)
        {
            if (progress.SideStoryQuestStateType == 0)
                progress.SideStoryQuestStateType = 1;

            if (IsSceneAhead(questId, sceneId, progress.HeadSideStoryQuestSceneId))
                progress.HeadSideStoryQuestSceneId = sceneId;
        }

        return Task.FromResult(new UpdateSideStoryQuestSceneProgressResponse());
    }

    private void ClearActiveProgress(DarkUserMemoryDatabase userDb, long userId)
    {
        EntityIUserSideStoryQuestSceneProgressStatus? activeProgress = userDb.EntityIUserSideStoryQuestSceneProgressStatus
            .FirstOrDefault(s => s.UserId == userId);
        if (activeProgress == null)
            return;

        activeProgress.CurrentSideStoryQuestId = 0;
        activeProgress.CurrentSideStoryQuestSceneId = 0;
    }

    private int GetFirstSceneId(int questId)
    {
        return _masterDb.EntityMSideStoryQuestScene
            .Where(s => s.SideStoryQuestId == questId)
            .OrderBy(s => s.SortOrder)
            .Select(s => s.SideStoryQuestSceneId)
            .FirstOrDefault();
    }

    private int ResolveCurrentSceneId(EntityIUserSideStoryQuest? existing, int questId, int firstSceneId)
    {
        if (existing == null || existing.HeadSideStoryQuestSceneId <= 0)
            return firstSceneId;

        return IsSceneInQuest(questId, existing.HeadSideStoryQuestSceneId)
            ? existing.HeadSideStoryQuestSceneId
            : firstSceneId;
    }

    private bool IsSceneAhead(int questId, int newSceneId, int currentHeadSceneId)
    {
        if (newSceneId == 0 || !TryGetSceneSortOrder(questId, newSceneId, out int newSortOrder))
            return false;

        if (currentHeadSceneId == 0)
            return true;

        return !TryGetSceneSortOrder(questId, currentHeadSceneId, out int currentSortOrder)
            || newSortOrder > currentSortOrder;
    }

    private bool IsSceneInQuest(int questId, int sceneId)
        => TryGetSceneSortOrder(questId, sceneId, out _);

    private bool TryGetSceneSortOrder(int questId, int sceneId, out int sortOrder)
    {
        EntityMSideStoryQuestScene? scene = _masterDb.EntityMSideStoryQuestScene
            .FirstOrDefault(s => s.SideStoryQuestId == questId && s.SideStoryQuestSceneId == sceneId);
        if (scene == null)
        {
            sortOrder = 0;
            return false;
        }

        sortOrder = scene.SortOrder;
        return true;
    }

}
