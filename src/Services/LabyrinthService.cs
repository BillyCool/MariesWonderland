using Grpc.Core;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Helpers;
using MariesWonderland.Models.Entities;
using MariesWonderland.Proto.Labyrinth;
using MariesWonderland.Quests;

namespace MariesWonderland.Services;

/// <summary>Fate Board (labyrinth): seasons, stage treasures and stage mission milestones.</summary>
public class LabyrinthService(UserDataStore store, DarkMasterMemoryDatabase masterDb, TimeProvider timeProvider) : MariesWonderland.Proto.Labyrinth.LabyrinthService.LabyrinthServiceBase
{
    private readonly UserDataStore _store = store;
    private readonly DarkMasterMemoryDatabase _masterDb = masterDb;
    private readonly TimeProvider _timeProvider = timeProvider;

    /// <summary>
    /// Opening a Fate Board: pays the last season the user joined there if it has ended (the client then plays the
    /// reach reward result), and joins the current season.
    /// </summary>
    public override Task<UpdateSeasonDataResponse> UpdateSeasonData(UpdateSeasonDataRequest request, ServerCallContext context)
    {
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(context.GetUserId());
        long nowMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();

        UpdateSeasonDataResponse response = new();
        EntityIUserEventQuestLabyrinthSeason? row = userDb.EntityIUserEventQuestLabyrinthSeason
            .FirstOrDefault(s => s.EventQuestChapterId == request.EventQuestChapterId);
        if (row != null && FateBoardHelper.PayEndedSeason(userDb, _masterDb, row, nowMs) is { } result)
        {
            response.SeasonResult.Add(result);
        }

        FateBoardHelper.JoinCurrentSeason(userDb, _masterDb, request.EventQuestChapterId, nowMs);
        return Task.FromResult(response);
    }

    /// <summary>The stage's treasure: its clear reward, sent to the gift box once, after every quest of the stage is cleared.</summary>
    public override Task<ReceiveStageClearRewardResponse> ReceiveStageClearReward(ReceiveStageClearRewardRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);
        long nowMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();

        EntityMEventQuestLabyrinthStage? stage = _masterDb.EntityMEventQuestLabyrinthStage
            .FirstOrDefault(s => s.EventQuestChapterId == request.EventQuestChapterId && s.StageOrder == request.StageOrder);
        if (stage is null || stage.StageClearRewardGroupId == 0)
        {
            return Task.FromResult(new ReceiveStageClearRewardResponse());
        }

        List<int> stageQuestIds = FateBoardHelper.StageQuestIds(_masterDb, stage);
        if (stageQuestIds.Count == 0 || !stageQuestIds.All(q => FateBoardHelper.IsQuestCleared(userDb, q)))
        {
            return Task.FromResult(new ReceiveStageClearRewardResponse());
        }

        EntityIUserEventQuestLabyrinthStage stageState = GetOrCreateLabyrinthStage(userDb, request.EventQuestChapterId, request.StageOrder);
        if (stageState.IsReceivedStageClearReward)
        {
            return Task.FromResult(new ReceiveStageClearRewardResponse());
        }

        foreach (EntityMEventQuestLabyrinthRewardGroup reward in _masterDb.EntityMEventQuestLabyrinthRewardGroup
            .Where(r => r.EventQuestLabyrinthRewardGroupId == stage.StageClearRewardGroupId)
            .OrderBy(r => r.SortOrder))
        {
            GiftBoxHelper.Add(userDb, reward.PossessionType, reward.PossessionId, reward.Count, nowMs);
        }

        stageState.IsReceivedStageClearReward = true;

        return Task.FromResult(new ReceiveStageClearRewardResponse());
    }

    /// <summary>
    /// The stage's mission milestones: every tier up to the count the client asks for, as far as the user's cleared
    /// quest missions in the stage reach, each tier once, sent to the gift box.
    /// </summary>
    public override Task<ReceiveStageAccumulationRewardResponse> ReceiveStageAccumulationReward(ReceiveStageAccumulationRewardRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);
        long nowMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();

        EntityMEventQuestLabyrinthStage? stage = _masterDb.EntityMEventQuestLabyrinthStage
            .FirstOrDefault(s => s.EventQuestChapterId == request.EventQuestChapterId && s.StageOrder == request.StageOrder);
        if (stage is null || stage.StageAccumulationRewardGroupId == 0)
        {
            return Task.FromResult(new ReceiveStageAccumulationRewardResponse());
        }

        int clearedCount = FateBoardHelper.ClearedQuestMissionCount(userDb, _masterDb, FateBoardHelper.StageQuestIds(_masterDb, stage));
        int claimUpTo = Math.Min(request.QuestMissionClearCount, clearedCount);
        EntityIUserEventQuestLabyrinthStage stageState = GetOrCreateLabyrinthStage(userDb, request.EventQuestChapterId, request.StageOrder);

        List<EntityMEventQuestLabyrinthStageAccumulationRewardGroup> eligibleRewards = [.. _masterDb.EntityMEventQuestLabyrinthStageAccumulationRewardGroup
            .Where(r => r.EventQuestLabyrinthStageAccumulationRewardGroupId == stage.StageAccumulationRewardGroupId
                && r.QuestMissionClearCount > stageState.AccumulationRewardReceivedQuestMissionCount
                && r.QuestMissionClearCount <= claimUpTo)
            .OrderBy(r => r.QuestMissionClearCount)];

        foreach (EntityMEventQuestLabyrinthStageAccumulationRewardGroup tier in eligibleRewards)
        {
            foreach (EntityMEventQuestLabyrinthRewardGroup reward in _masterDb.EntityMEventQuestLabyrinthRewardGroup
                .Where(r => r.EventQuestLabyrinthRewardGroupId == tier.EventQuestLabyrinthRewardGroupId)
                .OrderBy(r => r.SortOrder))
            {
                GiftBoxHelper.Add(userDb, reward.PossessionType, reward.PossessionId, reward.Count, nowMs);
            }
        }

        if (eligibleRewards.Count > 0)
        {
            stageState.AccumulationRewardReceivedQuestMissionCount = eligibleRewards[^1].QuestMissionClearCount;
        }

        return Task.FromResult(new ReceiveStageAccumulationRewardResponse());
    }

    private static EntityIUserEventQuestLabyrinthStage GetOrCreateLabyrinthStage(DarkUserMemoryDatabase userDb, int eventQuestChapterId, int stageOrder)
        => userDb.EntityIUserEventQuestLabyrinthStage.GetOrCreate(
            s => s.EventQuestChapterId == eventQuestChapterId && s.StageOrder == stageOrder,
            () => new EntityIUserEventQuestLabyrinthStage
            {
                UserId = userDb.UserId,
                EventQuestChapterId = eventQuestChapterId,
                StageOrder = stageOrder
            });
}
