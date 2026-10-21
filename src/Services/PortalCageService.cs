using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Helpers;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.PortalCage;

namespace MariesWonderland.Services;

public class PortalCageService(UserDataStore store)
    : MariesWonderland.Proto.PortalCage.PortalcageService.PortalcageServiceBase
{
    private readonly UserDataStore _store = store;

    /// <summary>Marks the portal cage scene as in-progress for the current user.</summary>
    public override Task<UpdatePortalCageSceneProgressResponse> UpdatePortalCageSceneProgress(UpdatePortalCageSceneProgressRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        EntityIUserPortalCageStatus status = userDb.EntityIUserPortalCageStatus.GetOrCreate(userId);
        status.IsCurrentProgress = true;

        EntityIUserSideStoryQuestSceneProgressStatus? sideStoryProgress = userDb.EntityIUserSideStoryQuestSceneProgressStatus
            .FirstOrDefault(s => s.UserId == userId);
        if (sideStoryProgress != null)
        {
            sideStoryProgress.CurrentSideStoryQuestId = 0;
            sideStoryProgress.CurrentSideStoryQuestSceneId = 0;
        }

        EntityIUserMainQuestFlowStatus mainQuestFlowStatus = userDb.EntityIUserMainQuestFlowStatus.GetOrCreate(userId);
        if (mainQuestFlowStatus.CurrentQuestFlowType != QuestFlowType.MAIN_FLOW)
        {
            mainQuestFlowStatus.CurrentQuestFlowType = QuestFlowType.MAIN_FLOW;
        }

        return Task.FromResult(new UpdatePortalCageSceneProgressResponse());
    }

    /// <summary>Returns an empty response. Portal cage drop items not yet implemented.</summary>
    public override Task<GetDropItemResponse> GetDropItem(Empty request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        _ = _store.GetOrCreate(userId);

        return Task.FromResult(new GetDropItemResponse());
    }
}
