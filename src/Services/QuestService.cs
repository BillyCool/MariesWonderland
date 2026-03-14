using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Quest;

namespace MariesWonderland.Services;

public class QuestService(UserDataStore store, DarkMasterMemoryDatabase masterDb, ILogger<QuestService> logger) : MariesWonderland.Proto.Quest.QuestService.QuestServiceBase
{
    private readonly UserDataStore _store = store;
    private readonly DarkMasterMemoryDatabase _masterDb = masterDb;
    private readonly ILogger<QuestService> _logger = logger;

    public override Task<UpdateMainFlowSceneProgressResponse> UpdateMainFlowSceneProgress(UpdateMainFlowSceneProgressRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);
        Dictionary<string, string> before = UserDataDiffBuilder.Snapshot(userDb);

        int questSceneId = request.QuestSceneId;

        // Resolve questId from the scene, then walk the master data chain to find the route:
        // QuestScene -> MainQuestSequence (by QuestId) -> MainQuestSequenceGroup (by MainQuestSequenceId)
        // -> MainQuestChapter (by MainQuestSequenceGroupId) -> MainQuestRouteId
        EntityMQuestScene? scene = _masterDb.EntityMQuestScene.FirstOrDefault(s => s.QuestSceneId == questSceneId);
        int routeId = 0;
        if (scene != null)
        {
            EntityMMainQuestSequence? sequence = _masterDb.EntityMMainQuestSequence.FirstOrDefault(s => s.QuestId == scene.QuestId);
            if (sequence != null)
            {
                EntityMMainQuestSequenceGroup? sequenceGroup = _masterDb.EntityMMainQuestSequenceGroup.FirstOrDefault(g => g.MainQuestSequenceId == sequence.MainQuestSequenceId);
                if (sequenceGroup != null)
                {
                    EntityMMainQuestChapter? chapter = _masterDb.EntityMMainQuestChapter.FirstOrDefault(c => c.MainQuestSequenceGroupId == sequenceGroup.MainQuestSequenceGroupId);
                    routeId = chapter?.MainQuestRouteId ?? 0;
                }
            }
        }

        EntityIUserMainQuestFlowStatus flowStatus = userDb.EntityIUserMainQuestFlowStatus.FirstOrDefault(s => s.UserId == userId)
            ?? AddEntity(userDb.EntityIUserMainQuestFlowStatus, new EntityIUserMainQuestFlowStatus { UserId = userId });
        flowStatus.CurrentQuestFlowType = QuestFlowType.MAIN_FLOW;

        EntityIUserMainQuestMainFlowStatus mainFlowStatus = userDb.EntityIUserMainQuestMainFlowStatus.FirstOrDefault(s => s.UserId == userId)
            ?? AddEntity(userDb.EntityIUserMainQuestMainFlowStatus, new EntityIUserMainQuestMainFlowStatus { UserId = userId });
        mainFlowStatus.CurrentQuestSceneId = questSceneId;
        mainFlowStatus.HeadQuestSceneId = Math.Max(mainFlowStatus.HeadQuestSceneId, questSceneId);
        if (routeId != 0) mainFlowStatus.CurrentMainQuestRouteId = routeId;

        UpdateMainFlowSceneProgressResponse response = new();
        foreach (var (k, v) in UserDataDiffBuilder.Delta(before, userDb)) response.DiffUserData[k] = v;
        return Task.FromResult(response);
    }

    /// <summary>Adds an entity to a list and returns it (convenience for inline new-entity seeding).</summary>
    private static T AddEntity<T>(List<T> list, T entity)
    {
        list.Add(entity);
        return entity;
    }

    public override Task<UpdateReplayFlowSceneProgressResponse> UpdateReplayFlowSceneProgress(UpdateReplayFlowSceneProgressRequest request, ServerCallContext context)
    {
        return Task.FromResult(new UpdateReplayFlowSceneProgressResponse());
    }

    public override Task<UpdateMainQuestSceneProgressResponse> UpdateMainQuestSceneProgress(UpdateMainQuestSceneProgressRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);
        Dictionary<string, string> before = UserDataDiffBuilder.Snapshot(userDb);

        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        int questSceneId = request.QuestSceneId;

        EntityMQuestScene? scene = _masterDb.EntityMQuestScene.FirstOrDefault(s => s.QuestSceneId == questSceneId);
        EntityMQuest? quest = scene != null ? _masterDb.EntityMQuest.FirstOrDefault(q => q.QuestId == scene.QuestId) : null;

        // IUserMainQuestMainFlowStatus — tracks current scene position and whether we've reached the last scene
        EntityIUserMainQuestMainFlowStatus mainFlowStatus = userDb.EntityIUserMainQuestMainFlowStatus.FirstOrDefault(s => s.UserId == userId)
            ?? AddEntity(userDb.EntityIUserMainQuestMainFlowStatus, new EntityIUserMainQuestMainFlowStatus { UserId = userId });
        mainFlowStatus.CurrentQuestSceneId = questSceneId;
        mainFlowStatus.HeadQuestSceneId = Math.Max(mainFlowStatus.HeadQuestSceneId, questSceneId);
        if (scene?.QuestResultType is QuestResultType.HALF_RESULT or QuestResultType.FULL_RESULT)
            mainFlowStatus.IsReachedLastQuestScene = true;

        // IUserMainQuestFlowStatus and IUserMainQuestProgressStatus — sub-flow tracking for playable quests
        bool isPlayable = quest != null && !quest.IsRunInTheBackground && quest.IsCountedAsQuest;
        if (isPlayable)
        {
            EntityIUserMainQuestFlowStatus flowStatus = userDb.EntityIUserMainQuestFlowStatus.FirstOrDefault(s => s.UserId == userId)
                ?? AddEntity(userDb.EntityIUserMainQuestFlowStatus, new EntityIUserMainQuestFlowStatus { UserId = userId });
            flowStatus.CurrentQuestFlowType = QuestFlowType.SUB_FLOW;

            EntityIUserMainQuestProgressStatus progressStatus = userDb.EntityIUserMainQuestProgressStatus.FirstOrDefault(s => s.UserId == userId)
                ?? AddEntity(userDb.EntityIUserMainQuestProgressStatus, new EntityIUserMainQuestProgressStatus { UserId = userId });
            progressStatus.CurrentQuestSceneId = questSceneId;
            progressStatus.HeadQuestSceneId = Math.Max(progressStatus.HeadQuestSceneId, questSceneId);
            progressStatus.CurrentQuestFlowType = QuestFlowType.SUB_FLOW;

            // Reaching a result scene clears the quest; a full-result scene also increments clear counts
            if (scene?.QuestResultType is QuestResultType.HALF_RESULT or QuestResultType.FULL_RESULT)
            {
                EntityIUserQuest userQuest = userDb.EntityIUserQuest.FirstOrDefault(q => q.UserId == userId && q.QuestId == quest!.QuestId)
                    ?? AddEntity(userDb.EntityIUserQuest, new EntityIUserQuest { UserId = userId, QuestId = quest!.QuestId });
                userQuest.QuestStateType = (int)QuestStateType.CLEARED;

                if (scene.QuestResultType == QuestResultType.FULL_RESULT)
                {
                    userQuest.ClearCount++;
                    userQuest.DailyClearCount++;
                    userQuest.LastClearDatetime = nowMs;
                }
            }
        }

        UpdateMainQuestSceneProgressResponse response = new();
        foreach (var (k, v) in UserDataDiffBuilder.Delta(before, userDb)) response.DiffUserData[k] = v;
        return Task.FromResult(response);
    }

    public override Task<UpdateExtraQuestSceneProgressResponse> UpdateExtraQuestSceneProgress(UpdateExtraQuestSceneProgressRequest request, ServerCallContext context)
    {
        return Task.FromResult(new UpdateExtraQuestSceneProgressResponse());
    }

    public override Task<UpdateEventQuestSceneProgressResponse> UpdateEventQuestSceneProgress(UpdateEventQuestSceneProgressRequest request, ServerCallContext context)
    {
        return Task.FromResult(new UpdateEventQuestSceneProgressResponse());
    }

    public override Task<StartMainQuestResponse> StartMainQuest(StartMainQuestRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);
        Dictionary<string, string> before = UserDataDiffBuilder.Snapshot(userDb);

        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        int questId = request.QuestId;

        EntityMQuest? quest = _masterDb.EntityMQuest.FirstOrDefault(q => q.QuestId == questId);

        // Ensure user quest and mission rows exist before any state transitions.
        EntityIUserQuest userQuest = userDb.EntityIUserQuest.FirstOrDefault(q => q.UserId == userId && q.QuestId == questId)
            ?? AddEntity(userDb.EntityIUserQuest, new EntityIUserQuest { UserId = userId, QuestId = questId });

        if (quest != null && quest.QuestMissionGroupId != 0)
        {
            List<int> missionIds = [.. _masterDb.EntityMQuestMissionGroup
                .Where(g => g.QuestMissionGroupId == quest.QuestMissionGroupId)
                .Select(g => g.QuestMissionId)];

            foreach (int missionId in missionIds)
            {
                if (!userDb.EntityIUserQuestMission.Any(m => m.UserId == userId && m.QuestId == questId && m.QuestMissionId == missionId))
                    userDb.EntityIUserQuestMission.Add(new EntityIUserQuestMission { UserId = userId, QuestId = questId, QuestMissionId = missionId });
            }
        }

        // Don't touch state on already-cleared quests (e.g. replaying a finished story quest).
        if (userQuest.QuestStateType == (int)QuestStateType.CLEARED)
        {
            StartMainQuestResponse earlyResponse = new();
            foreach (var (k, v) in UserDataDiffBuilder.Delta(before, userDb)) earlyResponse.DiffUserData[k] = v;
            return Task.FromResult(earlyResponse);
        }

        userQuest.IsBattleOnly = request.IsBattleOnly;

        // Background quests (cutscenes, non-combat) have no playable content — auto-clear them immediately.
        // Playable quests transition to ACTIVE so the client knows a quest session is in progress.
        bool isPlayable = quest != null && !quest.IsRunInTheBackground && quest.IsCountedAsQuest;
        if (isPlayable)
        {
            userQuest.QuestStateType = (int)QuestStateType.ACTIVE;
            userQuest.LatestStartDatetime = nowMs;
        }
        else
        {
            userQuest.QuestStateType = (int)QuestStateType.CLEARED;
            userQuest.ClearCount++;
            userQuest.DailyClearCount++;
            userQuest.LastClearDatetime = nowMs;
        }

        StartMainQuestResponse response = new();
        foreach (var (k, v) in UserDataDiffBuilder.Delta(before, userDb)) response.DiffUserData[k] = v;

        return Task.FromResult(response);
    }

    public override Task<RestartMainQuestResponse> RestartMainQuest(RestartMainQuestRequest request, ServerCallContext context)
    {
        return Task.FromResult(new RestartMainQuestResponse());
    }

    public override Task<FinishMainQuestResponse> FinishMainQuest(FinishMainQuestRequest request, ServerCallContext context)
    {
        return Task.FromResult(new FinishMainQuestResponse());
    }

    public override Task<StartExtraQuestResponse> StartExtraQuest(StartExtraQuestRequest request, ServerCallContext context)
    {
        return Task.FromResult(new StartExtraQuestResponse());
    }

    public override Task<RestartExtraQuestResponse> RestartExtraQuest(RestartExtraQuestRequest request, ServerCallContext context)
    {
        return Task.FromResult(new RestartExtraQuestResponse());
    }

    public override Task<FinishExtraQuestResponse> FinishExtraQuest(FinishExtraQuestRequest request, ServerCallContext context)
    {
        return Task.FromResult(new FinishExtraQuestResponse());
    }

    public override Task<StartEventQuestResponse> StartEventQuest(StartEventQuestRequest request, ServerCallContext context)
    {
        return Task.FromResult(new StartEventQuestResponse());
    }

    public override Task<RestartEventQuestResponse> RestartEventQuest(RestartEventQuestRequest request, ServerCallContext context)
    {
        return Task.FromResult(new RestartEventQuestResponse());
    }

    public override Task<FinishEventQuestResponse> FinishEventQuest(FinishEventQuestRequest request, ServerCallContext context)
    {
        return Task.FromResult(new FinishEventQuestResponse());
    }

    public override Task<FinishAutoOrbitResponse> FinishAutoOrbit(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new FinishAutoOrbitResponse());
    }

    public override Task<SetRouteResponse> SetRoute(SetRouteRequest request, ServerCallContext context)
    {
        return Task.FromResult(new SetRouteResponse());
    }

    public override Task<SetQuestSceneChoiceResponse> SetQuestSceneChoice(SetQuestSceneChoiceRequest request, ServerCallContext context)
    {
        return Task.FromResult(new SetQuestSceneChoiceResponse());
    }

    public override Task<ReceiveTowerAccumulationRewardResponse> ReceiveTowerAccumulationReward(ReceiveTowerAccumulationRewardRequest request, ServerCallContext context)
    {
        return Task.FromResult(new ReceiveTowerAccumulationRewardResponse());
    }

    public override Task<SkipQuestResponse> SkipQuest(SkipQuestRequest request, ServerCallContext context)
    {
        return Task.FromResult(new SkipQuestResponse());
    }

    public override Task<SkipQuestBulkResponse> SkipQuestBulk(SkipQuestBulkRequest request, ServerCallContext context)
    {
        return Task.FromResult(new SkipQuestBulkResponse());
    }

    public override Task<SetAutoSaleSettingResponse> SetAutoSaleSetting(SetAutoSaleSettingRequest request, ServerCallContext context)
    {
        return Task.FromResult(new SetAutoSaleSettingResponse());
    }

    public override Task<StartGuerrillaFreeOpenResponse> StartGuerrillaFreeOpen(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new StartGuerrillaFreeOpenResponse());
    }

    public override Task<ResetLimitContentQuestProgressResponse> ResetLimitContentQuestProgress(ResetLimitContentQuestProgressRequest request, ServerCallContext context)
    {
        return Task.FromResult(new ResetLimitContentQuestProgressResponse());
    }

    public override Task<ReceiveDailyQuestGroupCompleteRewardResponse> ReceiveDailyQuestGroupCompleteReward(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new ReceiveDailyQuestGroupCompleteRewardResponse());
    }
}
