using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Helpers;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.PortalCage;

namespace MariesWonderland.Services;

/// <summary>
/// Mama's Room (the portal cage). Its tree drops fruit at the interval of the MAP_ONLY_CAGE_INTERVAL_DROP_ITEM gimmick
/// (one per 480 minutes, at most 3); the client shows <c>IUserPortalCageStatus.CurrentDropItemCount</c> fruit and the
/// map gauge counts from <c>DropItemStartDatetime</c>.
/// </summary>
public class PortalCageService(UserDataStore store, DarkMasterMemoryDatabase masterDb, GameConfig gameConfig, TimeProvider timeProvider)
    : MariesWonderland.Proto.PortalCage.PortalcageService.PortalcageServiceBase
{
    private readonly UserDataStore _store = store;
    private readonly DarkMasterMemoryDatabase _masterDb = masterDb;
    private readonly GameConfig _gameConfig = gameConfig;
    private readonly TimeProvider _timeProvider = timeProvider;

    /// <summary>Marks the portal cage scene as in-progress for the current user, and lets the fruit that fell show.</summary>
    public override Task<UpdatePortalCageSceneProgressResponse> UpdatePortalCageSceneProgress(UpdatePortalCageSceneProgressRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);
        long nowMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();

        EntityIUserPortalCageStatus status = userDb.EntityIUserPortalCageStatus.GetOrCreate(userId);
        status.IsCurrentProgress = true;
        RefreshFruit(status, nowMs);

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

    /// <summary>The user picks up the fruit under the tree: grants one roll per fruit and starts the timer again.</summary>
    public override Task<GetDropItemResponse> GetDropItem(Empty request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);
        long nowMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();

        EntityIUserPortalCageStatus status = userDb.EntityIUserPortalCageStatus.GetOrCreate(userId);
        RefreshFruit(status, nowMs);

        GetDropItemResponse response = new();
        if (status.CurrentDropItemCount > 0)
        {
            (PossessionType possessionType, int possessionId, int count) = CageGimmickHelper.PlaceholderLoot(_gameConfig);
            int total = count * status.CurrentDropItemCount;
            PossessionHelper.Apply(userDb, possessionType, possessionId, total, _masterDb, nowMs);
            response.PortalCageDropItem.Add(new PortalCageDropItem { PossessionType = (int)possessionType, PossessionId = possessionId, Count = total });
        }

        status.CurrentDropItemCount = 0;
        status.DropItemStartDatetime = nowMs;
        return Task.FromResult(response);
    }

    /// <summary>
    /// Sets the fruit count from the time since the last pickup. The timer starts the first time the user is in
    /// Mama's Room (a new account doesn't find a full tree straight away).
    /// </summary>
    private void RefreshFruit(EntityIUserPortalCageStatus status, long nowMs)
    {
        if (status.DropItemStartDatetime == 0)
        {
            status.DropItemStartDatetime = nowMs;
        }

        EntityMGimmickInterval? interval = _masterDb.EntityMGimmick
            .Where(g => g.GimmickType == GimmickType.MAP_ONLY_CAGE_INTERVAL_DROP_ITEM)
            .Select(g => _masterDb.EntityMGimmickInterval.FirstOrDefault(i => i.GimmickId == g.GimmickId))
            .FirstOrDefault(i => i != null);
        int count = interval == null ? 0 : CageGimmickHelper.AvailableIntervalCount(interval, status.DropItemStartDatetime, nowMs);
        if (status.CurrentDropItemCount != count)
        {
            status.CurrentDropItemCount = count;
        }
    }
}
