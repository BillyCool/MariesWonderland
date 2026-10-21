using Grpc.Core;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Helpers;
using MariesWonderland.Models.Entities;
using MariesWonderland.Proto.Labyrinth;

namespace MariesWonderland.Services;

public class LabyrinthService(UserDataStore store, DarkMasterMemoryDatabase masterDb, TimeProvider timeProvider) : MariesWonderland.Proto.Labyrinth.LabyrinthService.LabyrinthServiceBase
{
    private readonly UserDataStore _store = store;
    private readonly DarkMasterMemoryDatabase _masterDb = masterDb;
    private readonly TimeProvider _timeProvider = timeProvider;

    public override Task<UpdateSeasonDataResponse> UpdateSeasonData(UpdateSeasonDataRequest request, ServerCallContext context)
    {
        UpdateSeasonDataResponse response = new();

        int seasonRewardGroupId = _masterDb.EntityMEventQuestLabyrinthSeason
            .Where(s => s.EventQuestChapterId == request.EventQuestChapterId)
            .Select(s => s.SeasonRewardGroupId)
            .FirstOrDefault();
        if (seasonRewardGroupId == 0)
        {
            return Task.FromResult(response);
        }

        Dictionary<int, LabyrinthReward[]> rewardsByGroup = _masterDb.EntityMEventQuestLabyrinthRewardGroup
            .GroupBy(r => r.EventQuestLabyrinthRewardGroupId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(r => r.SortOrder)
                    .Select(r => new LabyrinthReward
                    {
                        PossessionType = (int)r.PossessionType,
                        PossessionId = r.PossessionId,
                        Count = r.Count
                    })
                    .ToArray());

        List<EntityMEventQuestLabyrinthSeasonRewardGroup> milestoneRows = [.. _masterDb.EntityMEventQuestLabyrinthSeasonRewardGroup
            .Where(r => r.EventQuestLabyrinthSeasonRewardGroupId == seasonRewardGroupId)];
        Dictionary<int, int> stageOrderByRewardGroup = [];
        int nextStageOrder = 1;
        foreach (int rewardGroupId in milestoneRows.Select(r => r.EventQuestLabyrinthRewardGroupId).Distinct().Order())
        {
            stageOrderByRewardGroup[rewardGroupId] = nextStageOrder++;
        }

        foreach (EntityMEventQuestLabyrinthSeasonRewardGroup row in milestoneRows)
        {
            LabyrinthSeasonResult seasonResult = new()
            {
                EventQuestChapterId = request.EventQuestChapterId,
                HeadQuestId = row.HeadQuestId,
                HeadStageOrder = stageOrderByRewardGroup[row.EventQuestLabyrinthRewardGroupId]
            };

            if (rewardsByGroup.TryGetValue(row.EventQuestLabyrinthRewardGroupId, out LabyrinthReward[]? rewards))
            {
                seasonResult.SeasonReward.AddRange(rewards);
            }

            response.SeasonResult.Add(seasonResult);
        }

        return Task.FromResult(response);
    }

    public override Task<ReceiveStageClearRewardResponse> ReceiveStageClearReward(ReceiveStageClearRewardRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);
        long nowMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();
        EntityIUserEventQuestLabyrinthStage stageState = GetOrCreateLabyrinthStage(userDb, request.EventQuestChapterId, request.StageOrder);

        if (stageState.IsReceivedStageClearReward)
        {
            return Task.FromResult(new ReceiveStageClearRewardResponse());
        }

        EntityMEventQuestLabyrinthStage? stage = _masterDb.EntityMEventQuestLabyrinthStage
            .FirstOrDefault(s => s.EventQuestChapterId == request.EventQuestChapterId && s.StageOrder == request.StageOrder);
        if (stage is null || stage.StageClearRewardGroupId == 0)
        {
            return Task.FromResult(new ReceiveStageClearRewardResponse());
        }

        foreach (EntityMEventQuestLabyrinthRewardGroup reward in _masterDb.EntityMEventQuestLabyrinthRewardGroup
            .Where(r => r.EventQuestLabyrinthRewardGroupId == stage.StageClearRewardGroupId)
            .OrderBy(r => r.SortOrder))
        {
            PossessionHelper.Apply(userDb, reward.PossessionType, reward.PossessionId, reward.Count, _masterDb, nowMs);
        }

        stageState.IsReceivedStageClearReward = true;

        return Task.FromResult(new ReceiveStageClearRewardResponse());
    }

    public override Task<ReceiveStageAccumulationRewardResponse> ReceiveStageAccumulationReward(ReceiveStageAccumulationRewardRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);
        long nowMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();
        EntityIUserEventQuestLabyrinthStage stageState = GetOrCreateLabyrinthStage(userDb, request.EventQuestChapterId, request.StageOrder);

        EntityMEventQuestLabyrinthStage? stage = _masterDb.EntityMEventQuestLabyrinthStage
            .FirstOrDefault(s => s.EventQuestChapterId == request.EventQuestChapterId && s.StageOrder == request.StageOrder);
        if (stage is null || stage.StageAccumulationRewardGroupId == 0)
        {
            return Task.FromResult(new ReceiveStageAccumulationRewardResponse());
        }

        List<EntityMEventQuestLabyrinthStageAccumulationRewardGroup> eligibleRewards = [.. _masterDb.EntityMEventQuestLabyrinthStageAccumulationRewardGroup
            .Where(r => r.EventQuestLabyrinthStageAccumulationRewardGroupId == stage.StageAccumulationRewardGroupId
                && r.QuestMissionClearCount > stageState.AccumulationRewardReceivedQuestMissionCount
                && r.QuestMissionClearCount <= request.QuestMissionClearCount)
            .OrderBy(r => r.QuestMissionClearCount)];

        int highestClaimedCount = stageState.AccumulationRewardReceivedQuestMissionCount;
        foreach (EntityMEventQuestLabyrinthStageAccumulationRewardGroup tier in eligibleRewards)
        {
            foreach (EntityMEventQuestLabyrinthRewardGroup reward in _masterDb.EntityMEventQuestLabyrinthRewardGroup
                .Where(r => r.EventQuestLabyrinthRewardGroupId == tier.EventQuestLabyrinthRewardGroupId)
                .OrderBy(r => r.SortOrder))
            {
                PossessionHelper.Apply(userDb, reward.PossessionType, reward.PossessionId, reward.Count, _masterDb, nowMs);
            }

            highestClaimedCount = Math.Max(highestClaimedCount, tier.QuestMissionClearCount);
        }

        if (highestClaimedCount > stageState.AccumulationRewardReceivedQuestMissionCount)
        {
            stageState.AccumulationRewardReceivedQuestMissionCount = highestClaimedCount;
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
