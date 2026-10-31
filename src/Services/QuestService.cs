using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Helpers;
using MariesWonderland.Missions;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Quest;
using MariesWonderland.Quests;

namespace MariesWonderland.Services;

public class QuestService(UserDataStore store, DarkMasterMemoryDatabase masterDb, QuestEngine questEngine, QuestCampaignHelper questCampaignHelper, MissionTracker missions, TimeProvider timeProvider) : MariesWonderland.Proto.Quest.QuestService.QuestServiceBase
{
    // Most runs one auto-orbit loop may be set to. The client's picker stops here; master data has no config for it.
    private const int MaxAutoOrbitLoops = 10;

    private readonly UserDataStore _store = store;
    private readonly DarkMasterMemoryDatabase _masterDb = masterDb;
    private readonly QuestEngine _questEngine = questEngine;
    private readonly QuestCampaignHelper _questCampaignHelper = questCampaignHelper;
    private readonly MissionTracker _missions = missions;
    private readonly TimeProvider _timeProvider = timeProvider;

    /// <summary>Advances the main story flow to the specified quest scene and updates route/progress tracking.</summary>
    public override Task<UpdateMainFlowSceneProgressResponse> UpdateMainFlowSceneProgress(UpdateMainFlowSceneProgressRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);
        long nowMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();

        int questSceneId = request.QuestSceneId;
        MainQuestSceneContext? sceneContext = ResolveMainQuestSceneContext(questSceneId);

        // Update flow status to indicate the player is in the main story flow
        EntityIUserMainQuestFlowStatus flowStatus = userDb.EntityIUserMainQuestFlowStatus.GetOrCreate(userId);
        flowStatus.CurrentQuestFlowType = QuestFlowType.MAIN_FLOW;

        ApplyMainFlowSceneProgress(userDb, userId, questSceneId, sceneContext);

        // Clear side story active progress if the user was in a side story
        EntityIUserSideStoryQuestSceneProgressStatus sideStoryProgress = userDb.EntityIUserSideStoryQuestSceneProgressStatus.GetOrCreate(userId);
        if (sideStoryProgress.CurrentSideStoryQuestId != 0)
        {
            sideStoryProgress.CurrentSideStoryQuestId = 0;
            sideStoryProgress.CurrentSideStoryQuestSceneId = 0;
        }

        EntityIUserPortalCageStatus portalCageStatus = userDb.EntityIUserPortalCageStatus.GetOrCreate(userId);
        portalCageStatus.IsCurrentProgress = false;

        EntityMQuestScene? scene = _masterDb.EntityMQuestScene.FirstOrDefault(s => s.QuestSceneId == questSceneId);
        if (scene != null)
        {
            EntitySQuestSession? session = userDb.EntitySQuestSession.FirstOrDefault(s => s.UserId == userId && s.QuestId == scene.QuestId);
            if (session != null)
                UpdateQuestSessionState(session, questSceneId, GetHeadSceneId(session.HeadQuestSceneId, questSceneId), QuestFlowType.MAIN_FLOW);
        }

        ApplySceneGrants(userDb, questSceneId, nowMs);

        return Task.FromResult(new UpdateMainFlowSceneProgressResponse());
    }

    /// <summary>Advances the replay flow to the specified quest scene.</summary>
    public override Task<UpdateReplayFlowSceneProgressResponse> UpdateReplayFlowSceneProgress(UpdateReplayFlowSceneProgressRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        int questSceneId = request.QuestSceneId;

        EntityIUserMainQuestReplayFlowStatus replayFlowStatus = userDb.EntityIUserMainQuestReplayFlowStatus.GetOrCreate(userId);
        replayFlowStatus.CurrentQuestSceneId = questSceneId;
        if (IsSceneAhead(questSceneId, replayFlowStatus.CurrentHeadQuestSceneId))
            replayFlowStatus.CurrentHeadQuestSceneId = questSceneId;

        EntityIUserMainQuestFlowStatus flowStatus = userDb.EntityIUserMainQuestFlowStatus.GetOrCreate(userId);
        QuestFlowType replayFlowType = flowStatus.CurrentQuestFlowType == QuestFlowType.ANOTHER_ROUTE_REPLAY_FLOW
            ? QuestFlowType.ANOTHER_ROUTE_REPLAY_FLOW
            : QuestFlowType.REPLAY_FLOW;

        EntityMQuestScene? scene = _masterDb.EntityMQuestScene.FirstOrDefault(s => s.QuestSceneId == questSceneId);
        if (scene != null)
        {
            // A story-only point is replayed with no StartMainQuest, so this call is where the replay flow begins.
            if (flowStatus.CurrentQuestFlowType != QuestFlowType.ANOTHER_ROUTE_REPLAY_FLOW)
            {
                replayFlowType = ResolveReplayFlowType(userDb, userId, scene.QuestId);
            }

            EntitySQuestSession session = GetOrCreateQuestSession(userDb, userId, scene.QuestId);
            UpdateQuestSessionState(session, questSceneId, replayFlowStatus.CurrentHeadQuestSceneId, replayFlowType);
        }

        flowStatus.CurrentQuestFlowType = replayFlowType;

        EntityIUserSideStoryQuestSceneProgressStatus sideStoryProgress = userDb.EntityIUserSideStoryQuestSceneProgressStatus.GetOrCreate(userId);
        if (sideStoryProgress.CurrentSideStoryQuestId != 0)
        {
            sideStoryProgress.CurrentSideStoryQuestId = 0;
            sideStoryProgress.CurrentSideStoryQuestSceneId = 0;
        }

        EntityIUserPortalCageStatus portalCageStatus = userDb.EntityIUserPortalCageStatus.GetOrCreate(userId);
        portalCageStatus.IsCurrentProgress = false;

        return Task.FromResult(new UpdateReplayFlowSceneProgressResponse());
    }

    /// <summary>Advances sub-flow (playable quest) scene progress or main-flow (cutscene) position.</summary>
    public override Task<UpdateMainQuestSceneProgressResponse> UpdateMainQuestSceneProgress(UpdateMainQuestSceneProgressRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        int questSceneId = request.QuestSceneId;

        EntityMQuestScene? scene = _masterDb.EntityMQuestScene.FirstOrDefault(s => s.QuestSceneId == questSceneId);
        EntityMQuest? quest = scene != null ? _questEngine.FindQuest(scene.QuestId) : null;
        MainQuestSceneContext? sceneContext = scene != null ? ResolveMainQuestSceneContext(questSceneId) : null;

        long nowMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();
        if (quest != null)
        {
            FinalizeChainPreviousQuest(userDb, userId, quest.QuestId, nowMs);
        }

        // The client reports the scenes of a replayed battle here too; they must not turn the replay into a sub flow
        // (its finish would then pay first-clear and mission rewards) or move the story position.
        EntityIUserMainQuestFlowStatus currentFlowStatus = userDb.EntityIUserMainQuestFlowStatus.GetOrCreate(userId);
        QuestFlowType currentFlowType = currentFlowStatus.CurrentQuestFlowType;
        bool isReplay = currentFlowType is QuestFlowType.REPLAY_FLOW or QuestFlowType.ANOTHER_ROUTE_REPLAY_FLOW;

        bool isPlayable = QuestEngine.IsPlayable(quest);
        if (isPlayable)
        {
            QuestFlowType flowType = isReplay ? currentFlowType : QuestFlowType.SUB_FLOW;
            currentFlowStatus.CurrentQuestFlowType = flowType;

            EntityIUserMainQuestProgressStatus progressStatus = userDb.EntityIUserMainQuestProgressStatus.GetOrCreate(userId);
            progressStatus.CurrentQuestSceneId = questSceneId;
            if (IsSceneAhead(questSceneId, progressStatus.HeadQuestSceneId))
                progressStatus.HeadQuestSceneId = questSceneId;
            progressStatus.CurrentQuestFlowType = flowType;

            EntitySQuestSession session = GetOrCreateQuestSession(userDb, userId, scene!.QuestId);
            UpdateQuestSessionState(session, questSceneId, progressStatus.HeadQuestSceneId, flowType);
        }
        else if (scene != null)
        {
            // Non-playable quests (background, cutscene, etc.) update the main flow scene position,
            // except during a menu pick or a replay, which never move the story position.
            EntitySQuestSession? session = userDb.EntitySQuestSession.FirstOrDefault(s => s.UserId == userId && s.QuestId == scene.QuestId);
            if (session?.IsMenuPick != true && !isReplay)
                ApplyMainFlowSceneProgress(userDb, userId, questSceneId, sceneContext);

            if (session != null)
                UpdateQuestSessionState(session, questSceneId, GetHeadSceneId(session.HeadQuestSceneId, questSceneId), isReplay ? currentFlowType : QuestFlowType.MAIN_FLOW);
        }

        if (isReplay && scene != null)
        {
            EntityIUserMainQuestReplayFlowStatus replayFlowStatus = userDb.EntityIUserMainQuestReplayFlowStatus.GetOrCreate(userId);
            replayFlowStatus.CurrentQuestSceneId = questSceneId;
            if (IsSceneAhead(questSceneId, replayFlowStatus.CurrentHeadQuestSceneId))
                replayFlowStatus.CurrentHeadQuestSceneId = questSceneId;
        }

        UpdateMainQuestSceneProgressResponse response = new();
        return Task.FromResult(response);
    }

    /// <summary>Advances extra quest scene progress and applies scene-based grants.</summary>
    public override Task<UpdateExtraQuestSceneProgressResponse> UpdateExtraQuestSceneProgress(UpdateExtraQuestSceneProgressRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        int questSceneId = request.QuestSceneId;
        long nowMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();

        EntityIUserExtraQuestProgressStatus progressStatus = userDb.EntityIUserExtraQuestProgressStatus.GetOrCreate(userId);
        progressStatus.CurrentQuestSceneId = questSceneId;
        if (IsSceneAhead(questSceneId, progressStatus.HeadQuestSceneId))
            progressStatus.HeadQuestSceneId = questSceneId;

        EntityMQuestScene? scene = _masterDb.EntityMQuestScene.FirstOrDefault(s => s.QuestSceneId == questSceneId);
        if (scene != null)
        {
            EntitySQuestSession session = GetOrCreateQuestSession(userDb, userId, scene.QuestId);
            UpdateQuestSessionState(session, questSceneId, progressStatus.HeadQuestSceneId, QuestFlowType.SUB_FLOW);
        }

        ApplySceneGrants(userDb, questSceneId, nowMs);

        return Task.FromResult(new UpdateExtraQuestSceneProgressResponse());
    }

    /// <summary>Advances event quest scene progress.</summary>
    public override Task<UpdateEventQuestSceneProgressResponse> UpdateEventQuestSceneProgress(UpdateEventQuestSceneProgressRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        int questSceneId = request.QuestSceneId;
        long nowMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();

        EntityIUserEventQuestProgressStatus progressStatus = userDb.EntityIUserEventQuestProgressStatus.GetOrCreate(userId);
        progressStatus.CurrentQuestSceneId = questSceneId;
        if (IsSceneAhead(questSceneId, progressStatus.HeadQuestSceneId))
            progressStatus.HeadQuestSceneId = questSceneId;

        EntityMQuestScene? scene = _masterDb.EntityMQuestScene.FirstOrDefault(s => s.QuestSceneId == questSceneId);
        if (scene != null)
        {
            EntitySQuestSession session = GetOrCreateQuestSession(userDb, userId, scene.QuestId);
            UpdateQuestSessionState(session, questSceneId, progressStatus.HeadQuestSceneId, QuestFlowType.SUB_FLOW, progressStatus.CurrentEventQuestChapterId);
        }

        ApplySceneGrants(userDb, questSceneId, nowMs);

        return Task.FromResult(new UpdateEventQuestSceneProgressResponse());
    }

    /// <summary>Initializes a main quest: creates quest/mission records, sets flow statuses, and returns battle drop rewards.</summary>
    public override Task<StartMainQuestResponse> StartMainQuest(StartMainQuestRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        long nowMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();
        int questId = request.QuestId;

        EntityMQuest? quest = _questEngine.FindQuest(questId);

        // Leaving The Cage for a quest reports what was measured there since the last report.
        if (request.CageMeasurableValues is { } cage)
        {
            _missions.RecordCageValues(userDb, cage.RunningDistanceMeters, cage.MamaTappedCount, nowMs);
        }

        _questEngine.ConsumeStamina(userDb, quest, _questCampaignHelper.GetMainQuestTarget(questId), 1, nowMs);

        // Replay quests have their own ids and may never have been cleared; they keep no mission rows.
        if (request.IsReplayFlow)
        {
            EntityIUserQuest replayQuest = QuestEngine.GetOrCreateUserQuest(userDb, questId);

            EntityIUserMainQuestFlowStatus replayFlow = userDb.EntityIUserMainQuestFlowStatus.GetOrCreate(userId);
            QuestFlowType replayFlowType = replayFlow.CurrentQuestFlowType == QuestFlowType.ANOTHER_ROUTE_REPLAY_FLOW
                ? QuestFlowType.ANOTHER_ROUTE_REPLAY_FLOW
                : ResolveReplayFlowType(userDb, userId, questId);
            replayFlow.CurrentQuestFlowType = replayFlowType;

            // The replay position is kept between replays: the map places its chapter points from it (the real save holds
            // the last scene there, never 0).
            userDb.EntityIUserMainQuestReplayFlowStatus.GetOrCreate(userId);

            QuestEngine.MarkActive(replayQuest, nowMs);
            replayQuest.IsBattleOnly = request.IsBattleOnly;

            // A story-only quest has no battle, so no finish call ever comes: it counts as cleared when its replay starts.
            if (!QuestEngine.IsPlayable(quest))
            {
                if (replayQuest.ClearCount == 0)
                {
                    QuestEngine.MarkCleared(replayQuest, 1, nowMs);
                }
                else
                {
                    replayQuest.QuestStateType = (int)QuestStateType.CLEARED;
                }
            }

            EntitySQuestSession replaySession = GetOrCreateQuestSession(userDb, userId, questId);
            _questEngine.SetSessionDeck(replaySession, request.UserDeckNumber);
            replaySession.IsBattleOnly = request.IsBattleOnly;
            replaySession.CurrentQuestFlowType = replayFlowType;
            int replayStartSceneId = _questEngine.GetQuestStartSceneId(questId, request.IsBattleOnly);
            replaySession.CurrentQuestSceneId = replayStartSceneId;
            replaySession.HeadQuestSceneId = replayStartSceneId;

            StartQuestAutoOrbit(userDb, userId, QuestType.MAIN_QUEST, 0, questId, request.MaxAutoOrbitCount, nowMs);

            StartMainQuestResponse replayResponse = new();
            replayResponse.BattleDropReward.AddRange(_questEngine.BuildBattleDropRewards(questId));
            return Task.FromResult(replayResponse);
        }

        // Ensure user quest and mission rows exist before any state transitions.
        EntityIUserQuest userQuest = _questEngine.EnsureQuestRows(userDb, questId);

        if (!request.IsMainFlow)
        {
            StartMenuPickQuest(userDb, userId, quest, userQuest, request, nowMs);

            StartMainQuestResponse menuPickResponse = new();
            menuPickResponse.BattleDropReward.AddRange(_questEngine.BuildBattleDropRewards(questId));
            return Task.FromResult(menuPickResponse);
        }

        // Already-cleared quests start nothing new.
        if (userQuest.QuestStateType == (int)QuestStateType.CLEARED)
        {
            StartQuestAutoOrbit(userDb, userId, QuestType.MAIN_QUEST, 0, questId, request.MaxAutoOrbitCount, nowMs);

            StartMainQuestResponse earlyResponse = new();
            earlyResponse.BattleDropReward.AddRange(_questEngine.BuildBattleDropRewards(questId));
            return Task.FromResult(earlyResponse);
        }

        userQuest.IsBattleOnly = request.IsBattleOnly;

        EntitySQuestSession startSession = GetOrCreateQuestSession(userDb, userId, questId);
        _questEngine.SetSessionDeck(startSession, request.UserDeckNumber);
        startSession.IsBattleOnly = request.IsBattleOnly;

        bool isPlayable = QuestEngine.IsPlayable(quest);
        int startSceneId = _questEngine.GetQuestStartSceneId(questId, request.IsBattleOnly);
        startSession.CurrentQuestFlowType = isPlayable ? QuestFlowType.SUB_FLOW : QuestFlowType.MAIN_FLOW;
        startSession.CurrentQuestSceneId = startSceneId;
        startSession.HeadQuestSceneId = startSceneId;
        if (isPlayable)
        {
            EntityIUserMainQuestFlowStatus flowStatus = userDb.EntityIUserMainQuestFlowStatus.GetOrCreate(userId);
            flowStatus.CurrentQuestFlowType = QuestFlowType.MAIN_FLOW;

            QuestEngine.MarkActive(userQuest, nowMs);

            // Ensure ProgressStatus singleton exists so it always appears in the DiffInterceptor delta.
            EntityIUserMainQuestProgressStatus progressStatus = userDb.EntityIUserMainQuestProgressStatus.GetOrCreate(userId);

            // For battle-only quests the client skips UpdateMainQuestSceneProgress, so ProgressStatus
            // would never have the battle scene set. Explicitly set it here.
            if (request.IsBattleOnly && startSceneId != 0)
            {
                progressStatus.CurrentQuestSceneId = startSceneId;
                progressStatus.HeadQuestSceneId = GetHeadSceneId(progressStatus.HeadQuestSceneId, startSceneId);
                progressStatus.CurrentQuestFlowType = QuestFlowType.SUB_FLOW;
            }

            // Ensure ReplayFlowStatus singleton exists so it is always included in the delta.
            EntityIUserMainQuestReplayFlowStatus replayFlowStatus = userDb.EntityIUserMainQuestReplayFlowStatus.GetOrCreate(userId);

            // Ensure MainFlowStatus singleton exists and always appears in the delta, even when unchanged.
            EntityIUserMainQuestMainFlowStatus mainFlowStatus = userDb.EntityIUserMainQuestMainFlowStatus.GetOrCreate(userId);
        }
        else
        {
            // Background quests (cutscenes, non-combat) — auto-clear immediately.
            QuestEngine.MarkCleared(userQuest, 1, nowMs);

            // Set scene IDs from the first scene of this quest
            if (startSceneId != 0)
            {
                EntityIUserMainQuestFlowStatus flowStatus = userDb.EntityIUserMainQuestFlowStatus.GetOrCreate(userId);
                flowStatus.CurrentQuestFlowType = QuestFlowType.MAIN_FLOW;
                ApplyMainFlowSceneProgress(userDb, userId, startSceneId, ResolveMainQuestSceneContext(startSceneId));
            }
        }

        StartQuestAutoOrbit(userDb, userId, QuestType.MAIN_QUEST, 0, questId, request.MaxAutoOrbitCount, nowMs);

        StartMainQuestResponse response = new();
        response.BattleDropReward.AddRange(_questEngine.BuildBattleDropRewards(questId));
        return Task.FromResult(response);
    }

    /// <summary>Restarts a main quest: resets flow to MAIN_FLOW and returns battle drop rewards.</summary>
    public override Task<RestartMainQuestResponse> RestartMainQuest(RestartMainQuestRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        long nowMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();
        int questId = request.QuestId;

        EntityIUserQuest userQuest = QuestEngine.GetOrCreateUserQuest(userDb, questId);
        MarkRestarted(userDb, userQuest, questId, nowMs);

        EntitySQuestSession restartSession = GetOrCreateQuestSession(userDb, userId, questId);
        userQuest.IsBattleOnly = restartSession.IsBattleOnly;
        int restoredSceneId = restartSession.CurrentQuestSceneId != 0
            ? restartSession.CurrentQuestSceneId
            : _questEngine.GetQuestStartSceneId(questId, restartSession.IsBattleOnly);
        int restoredHeadSceneId = restartSession.HeadQuestSceneId != 0
            ? restartSession.HeadQuestSceneId
            : restoredSceneId;

        EntityIUserMainQuestFlowStatus flowStatus = userDb.EntityIUserMainQuestFlowStatus.GetOrCreate(userId);
        EntityMQuest? quest = _questEngine.FindQuest(questId);
        bool isPlayableMainQuest = QuestEngine.IsPlayable(quest);
        QuestFlowType restoredFlowType = restartSession.CurrentQuestFlowType;
        if (restoredFlowType == QuestFlowType.UNKNOWN)
            restoredFlowType = isPlayableMainQuest ? QuestFlowType.SUB_FLOW : QuestFlowType.MAIN_FLOW;
        flowStatus.CurrentQuestFlowType = restoredFlowType;

        if (isPlayableMainQuest)
        {
            if (restoredFlowType == QuestFlowType.REPLAY_FLOW || restoredFlowType == QuestFlowType.ANOTHER_ROUTE_REPLAY_FLOW)
            {
                EntityIUserMainQuestReplayFlowStatus replayFlowStatus = userDb.EntityIUserMainQuestReplayFlowStatus.GetOrCreate(userId);
                replayFlowStatus.CurrentQuestSceneId = restoredSceneId;
                replayFlowStatus.CurrentHeadQuestSceneId = restoredHeadSceneId;

                EntityIUserMainQuestProgressStatus progressStatus = userDb.EntityIUserMainQuestProgressStatus.GetOrCreate(userId);
                progressStatus.CurrentQuestSceneId = 0;
                progressStatus.HeadQuestSceneId = 0;
                progressStatus.CurrentQuestFlowType = QuestFlowType.UNKNOWN;
            }
            else
            {
                EntityIUserMainQuestProgressStatus progressStatus = userDb.EntityIUserMainQuestProgressStatus.GetOrCreate(userId);
                progressStatus.CurrentQuestSceneId = restoredSceneId;
                progressStatus.HeadQuestSceneId = restoredHeadSceneId;
                progressStatus.CurrentQuestFlowType = QuestFlowType.SUB_FLOW;
            }

            MainQuestSceneContext? sceneContext = ResolveMainQuestSceneContext(restoredSceneId);
            if (sceneContext != null && !restartSession.IsMenuPick)
            {
                EntityIUserMainQuestMainFlowStatus mainFlowStatus = userDb.EntityIUserMainQuestMainFlowStatus.GetOrCreate(userId);
                ApplyMainQuestRoute(userDb, userId, mainFlowStatus, sceneContext.RouteId, sceneContext.SeasonId);
            }
        }
        else if (restoredSceneId != 0 && !restartSession.IsMenuPick)
        {
            flowStatus.CurrentQuestFlowType = QuestFlowType.MAIN_FLOW;
            ApplyMainFlowSceneProgress(userDb, userId, restoredSceneId, ResolveMainQuestSceneContext(restoredSceneId));
        }

        UpdateQuestSessionState(restartSession, restoredSceneId, restoredHeadSceneId, flowStatus.CurrentQuestFlowType);

        RestartMainQuestResponse response = new();
        response.BattleDropReward.AddRange(_questEngine.BuildBattleDropRewards(questId));
        response.BattleBinary = LastBattleBinary(userDb, userId, questId);
        response.DeckNumber = restartSession.UserDeckNumber;
        return Task.FromResult(response);
    }

    /// <summary>Completes a main quest: evaluates rewards, applies EXP/gold/possessions, updates quest state, and returns results.</summary>
    public override Task<FinishMainQuestResponse> FinishMainQuest(FinishMainQuestRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        if (!_store.TryGet(userId, out DarkUserMemoryDatabase userDb)) return Task.FromResult(new FinishMainQuestResponse());

        long nowMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();
        int questId = request.QuestId;
        bool isWin = !request.IsRetired && !request.IsAnnihilated;
        if (!isWin)
        {
            _missions.RecordQuestBattleLoss(userDb, request.IsRetired, request.IsAnnihilated, nowMs);
        }

        EntityMQuest? quest = _questEngine.FindQuest(questId);
        if (quest == null) return Task.FromResult(new FinishMainQuestResponse());

        // Replay flow rewards, granted on every replay win
        List<QuestReward> replayFlowFirstClearRewards = [];
        EntityIUserMainQuestFlowStatus? currentFlowStatus = userDb.EntityIUserMainQuestFlowStatus.FirstOrDefault(s => s.UserId == userId);
        bool wasReplayFlow = currentFlowStatus?.CurrentQuestFlowType is QuestFlowType.REPLAY_FLOW or QuestFlowType.ANOTHER_ROUTE_REPLAY_FLOW;
        if (isWin && wasReplayFlow && quest.QuestReplayFlowRewardGroupId > 0)
        {
            foreach (EntityMQuestReplayFlowRewardGroup reward in _masterDb.EntityMQuestReplayFlowRewardGroup
                .Where(r => r.QuestReplayFlowRewardGroupId == quest.QuestReplayFlowRewardGroupId))
            {
                replayFlowFirstClearRewards.Add(new QuestReward
                {
                    PossessionType = (int)reward.PossessionType,
                    PossessionId = reward.PossessionId,
                    Count = reward.Count,
                    RewardEffectId = RewardEffectHelper.EffectId(_masterDb, reward.PossessionType, reward.PossessionId)
                });
            }
        }

        EntitySQuestSession? finishSession = userDb.EntitySQuestSession.FirstOrDefault(s => s.UserId == userId && s.QuestId == questId);
        bool wasMenuPick = finishSession?.IsMenuPick == true;

        QuestFinishResult result = _questEngine.Finish(userDb, quest, _questCampaignHelper.GetMainQuestTarget(questId), isWin, nowMs, isReplay: wasReplayFlow, battles: BattlesOfRun(userDb, userId, questId));

        if (isWin)
        {
            foreach (QuestReward replayReward in replayFlowFirstClearRewards)
            {
                PossessionHelper.Apply(userDb, (PossessionType)replayReward.PossessionType, replayReward.PossessionId, replayReward.Count, _masterDb, nowMs);
            }

            bool isPlayableMainQuest = QuestEngine.IsPlayable(quest);
            if (isPlayableMainQuest && !wasReplayFlow && !wasMenuPick)
            {
                int lastSceneId = GetLastSceneIdForQuest(questId);
                if (lastSceneId != 0)
                {
                    ApplyMainFlowSceneProgress(userDb, userId, lastSceneId, ResolveMainQuestSceneContext(lastSceneId));
                }
            }
        }

        // === PHASE C: Reset flow and progress (always, even on retire) ===
        EntityIUserMainQuestProgressStatus? progressStatus = userDb.EntityIUserMainQuestProgressStatus.FirstOrDefault(s => s.UserId == userId);
        if (progressStatus != null)
        {
            progressStatus.CurrentQuestSceneId = 0;
            progressStatus.HeadQuestSceneId = 0;
            progressStatus.CurrentQuestFlowType = QuestFlowType.UNKNOWN;
        }

        // After a replay the flow stays a replay, as in the real save: the client resumes into the map from it.
        if (currentFlowStatus != null && !wasReplayFlow)
        {
            currentFlowStatus.CurrentQuestFlowType = QuestFlowType.UNKNOWN;
        }

        if (wasMenuPick)
        {
            RestoreMenuPickContext(userDb, userId, finishSession!, nowMs);
        }

        // Build response with all reward categories and clean up the server-side quest session
        FinishMainQuestResponse response = new() { IsBigWin = result.IsBigWin };
        response.DropReward.AddRange(result.DropRewards);
        response.FirstClearReward.AddRange(result.FirstClearRewards);
        response.MissionClearReward.AddRange(result.MissionClearRewards);
        response.MissionClearCompleteReward.AddRange(result.MissionClearCompleteRewards);
        response.ReplayFlowFirstClearReward.AddRange(replayFlowFirstClearRewards);
        if (result.IsBigWin)
        {
            response.BigWinClearedQuestMissionIdList.Add(result.BigWinMissionId);
        }

        response.AutoOrbitReward = AdvanceQuestAutoOrbit(
            userDb,
            userId,
            request.IsAutoOrbit,
            request.IsRetired,
            request.IsAnnihilated,
            QuestType.MAIN_QUEST,
            0,
            questId,
            result.DropRewards,
            nowMs);

        userDb.EntitySQuestSession.RemoveAll(s => s.QuestId == questId);

        return Task.FromResult(response);
    }

    /// <summary>
    /// A restart while the quest still has its session is the player coming back to a run the app was closed in: the run
    /// keeps its start, so the battles already fought still count for its missions. Without a session it's a new run.
    /// </summary>
    private static void MarkRestarted(DarkUserMemoryDatabase userDb, EntityIUserQuest userQuest, int questId, long nowMs)
    {
        if (userDb.EntitySQuestSession.Any(s => s.QuestId == questId))
        {
            userQuest.QuestStateType = (int)QuestStateType.ACTIVE;
            return;
        }

        QuestEngine.MarkActive(userQuest, nowMs);
    }

    /// <summary>The battles recorded for the quest's current run: those since the quest was last started.</summary>
    private IReadOnlyList<EntitySBattleDetail> BattlesOfRun(DarkUserMemoryDatabase userDb, long userId, int questId)
    {
        long startMs = userDb.EntityIUserQuest.FirstOrDefault(q => q.QuestId == questId)?.LatestStartDatetime ?? 0;
        return _store.BattleDetails(userId, questId, DateTimeOffset.FromUnixTimeMilliseconds(startMs).UtcDateTime);
    }

    /// <summary>
    /// The battle state the client sent when it finished the run's last wave, handed back on a restart so the
    /// client continues with the next wave. Empty when no wave of the run was finished: it starts at wave 1.
    /// </summary>
    private ByteString LastBattleBinary(DarkUserMemoryDatabase userDb, long userId, int questId)
    {
        string? battleBinary = BattlesOfRun(userDb, userId, questId).LastOrDefault(b => b.BattleBinary.Length > 0)?.BattleBinary;
        return battleBinary == null ? ByteString.Empty : ByteString.FromBase64(battleBinary);
    }

    /// <summary>Grants scene-based possessions from master data for the given quest scene.</summary>
    private void ApplySceneGrants(DarkUserMemoryDatabase userDb, int questSceneId, long nowMs)
    {
        List<EntityMUserQuestSceneGrantPossession> grants = [.. _masterDb.EntityMUserQuestSceneGrantPossession
            .Where(g => g.QuestSceneId == questSceneId && !g.IsDebug)];

        foreach (EntityMUserQuestSceneGrantPossession grant in grants)
            PossessionHelper.Apply(userDb, grant.PossessionType, grant.PossessionId, grant.Count, _masterDb, nowMs);
    }

    /// <summary>Initializes an extra quest: creates quest/mission records and returns battle drop rewards.</summary>
    public override Task<StartExtraQuestResponse> StartExtraQuest(StartExtraQuestRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        long nowMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();
        int questId = request.QuestId;

        EntityIUserQuest userQuest = _questEngine.EnsureQuestRows(userDb, questId);
        _questEngine.ConsumeStamina(userDb, _questEngine.FindQuest(questId), _questCampaignHelper.GetExtraQuestTarget(questId), 1, nowMs);

        EntitySQuestSession session = GetOrCreateQuestSession(userDb, userId, questId);
        _questEngine.SetSessionDeck(session, request.UserDeckNumber);

        QuestEngine.MarkActive(userQuest, nowMs);

        EntityIUserExtraQuestProgressStatus progressStatus = userDb.EntityIUserExtraQuestProgressStatus.GetOrCreate(userId);
        progressStatus.CurrentQuestId = questId;

        // Set initial scene IDs from the first scene of this quest
        int firstSceneId = _questEngine.GetQuestStartSceneId(questId);
        if (firstSceneId != 0)
        {
            progressStatus.CurrentQuestSceneId = firstSceneId;
            progressStatus.HeadQuestSceneId = firstSceneId;
        }
        UpdateQuestSessionState(session, firstSceneId, firstSceneId, QuestFlowType.SUB_FLOW);

        StartExtraQuestResponse response = new();
        response.BattleDropReward.AddRange(_questEngine.BuildBattleDropRewards(questId));
        return Task.FromResult(response);
    }

    /// <summary>Restarts an extra quest and returns battle drop rewards.</summary>
    public override Task<RestartExtraQuestResponse> RestartExtraQuest(RestartExtraQuestRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        long nowMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();
        int questId = request.QuestId;

        EntityIUserQuest userQuest = QuestEngine.GetOrCreateUserQuest(userDb, questId);
        MarkRestarted(userDb, userQuest, questId, nowMs);

        EntitySQuestSession session = GetOrCreateQuestSession(userDb, userId, questId);
        userQuest.IsBattleOnly = session.IsBattleOnly;

        EntityIUserExtraQuestProgressStatus progressStatus = userDb.EntityIUserExtraQuestProgressStatus.GetOrCreate(userId);
        progressStatus.CurrentQuestId = questId;
        int restoredSceneId = session.CurrentQuestSceneId != 0 ? session.CurrentQuestSceneId : _questEngine.GetQuestStartSceneId(questId);
        int restoredHeadSceneId = session.HeadQuestSceneId != 0 ? session.HeadQuestSceneId : restoredSceneId;
        progressStatus.CurrentQuestSceneId = restoredSceneId;
        progressStatus.HeadQuestSceneId = restoredHeadSceneId;
        UpdateQuestSessionState(session, restoredSceneId, restoredHeadSceneId, QuestFlowType.SUB_FLOW);

        RestartExtraQuestResponse response = new();
        response.BattleDropReward.AddRange(_questEngine.BuildBattleDropRewards(questId));
        response.BattleBinary = LastBattleBinary(userDb, userId, questId);
        response.DeckNumber = session.UserDeckNumber;
        return Task.FromResult(response);
    }

    /// <summary>Completes an extra quest: evaluates rewards, applies possessions, updates quest state, and returns results.</summary>
    public override Task<FinishExtraQuestResponse> FinishExtraQuest(FinishExtraQuestRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        if (!_store.TryGet(userId, out DarkUserMemoryDatabase userDb)) return Task.FromResult(new FinishExtraQuestResponse());

        long nowMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();
        int questId = request.QuestId;

        EntityMQuest? quest = _questEngine.FindQuest(questId);
        if (quest == null) return Task.FromResult(new FinishExtraQuestResponse());

        bool isWin = !request.IsRetired && !request.IsAnnihilated;
        if (!isWin)
        {
            _missions.RecordQuestBattleLoss(userDb, request.IsRetired, request.IsAnnihilated, nowMs);
        }
        QuestFinishResult result = _questEngine.Finish(userDb, quest, _questCampaignHelper.GetExtraQuestTarget(questId), isWin, nowMs, battles: BattlesOfRun(userDb, userId, questId));

        // Reset extra quest progress (always)
        EntityIUserExtraQuestProgressStatus? extraProgress = userDb.EntityIUserExtraQuestProgressStatus
            .FirstOrDefault(s => s.UserId == userId);
        if (extraProgress != null)
        {
            extraProgress.CurrentQuestId = 0;
            extraProgress.CurrentQuestSceneId = 0;
            extraProgress.HeadQuestSceneId = 0;
        }

        FinishExtraQuestResponse response = new() { IsBigWin = result.IsBigWin };
        response.DropReward.AddRange(result.DropRewards);
        response.FirstClearReward.AddRange(result.FirstClearRewards);
        response.MissionClearReward.AddRange(result.MissionClearRewards);
        response.MissionClearCompleteReward.AddRange(result.MissionClearCompleteRewards);
        if (result.IsBigWin)
        {
            response.BigWinClearedQuestMissionIdList.Add(result.BigWinMissionId);
        }

        userDb.EntitySQuestSession.RemoveAll(s => s.QuestId == questId);

        return Task.FromResult(response);
    }

    /// <summary>Initializes an event quest: creates quest/mission records and returns battle drop rewards.</summary>
    public override Task<StartEventQuestResponse> StartEventQuest(StartEventQuestRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        long nowMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();
        int questId = request.QuestId;
        int chapterId = request.EventQuestChapterId;

        EntityIUserQuest userQuest = _questEngine.EnsureQuestRows(userDb, questId);
        userQuest.IsBattleOnly = request.IsBattleOnly;
        _questEngine.ConsumeStamina(userDb, _questEngine.FindQuest(questId), _questCampaignHelper.GetEventQuestTarget(questId, chapterId), 1, nowMs);

        EntitySQuestSession eventSession = GetOrCreateQuestSession(userDb, userId, questId);
        _questEngine.SetSessionDeck(eventSession, request.UserDeckNumber, chapterId);
        eventSession.IsBattleOnly = request.IsBattleOnly;
        eventSession.CurrentEventQuestChapterId = chapterId;

        QuestEngine.MarkActive(userQuest, nowMs);

        EntityIUserEventQuestProgressStatus progressStatus = userDb.EntityIUserEventQuestProgressStatus.GetOrCreate(userId);
        progressStatus.CurrentEventQuestChapterId = chapterId;
        progressStatus.CurrentQuestId = questId;

        // Set initial scene IDs from the first scene of this quest
        int firstSceneId = _questEngine.GetQuestStartSceneId(questId, request.IsBattleOnly);
        if (firstSceneId != 0)
        {
            progressStatus.CurrentQuestSceneId = firstSceneId;
            progressStatus.HeadQuestSceneId = firstSceneId;
        }
        UpdateQuestSessionState(eventSession, firstSceneId, firstSceneId, QuestFlowType.SUB_FLOW, chapterId, request.IsBattleOnly);

        StartQuestAutoOrbit(userDb, userId, QuestType.EVENT_QUEST, chapterId, questId, request.MaxAutoOrbitCount, nowMs);

        StartEventQuestResponse response = new();
        response.BattleDropReward.AddRange(_questEngine.BuildBattleDropRewards(questId));
        return Task.FromResult(response);
    }

    /// <summary>Restarts an event quest.</summary>
    public override Task<RestartEventQuestResponse> RestartEventQuest(RestartEventQuestRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        long nowMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();
        int questId = request.QuestId;

        EntityIUserQuest userQuest = QuestEngine.GetOrCreateUserQuest(userDb, questId);
        MarkRestarted(userDb, userQuest, questId, nowMs);

        EntitySQuestSession session = GetOrCreateQuestSession(userDb, userId, questId);
        userQuest.IsBattleOnly = session.IsBattleOnly;

        EntityIUserEventQuestProgressStatus progressStatus = userDb.EntityIUserEventQuestProgressStatus.GetOrCreate(userId);
        int restoredSceneId = session.CurrentQuestSceneId != 0 ? session.CurrentQuestSceneId : _questEngine.GetQuestStartSceneId(questId, session.IsBattleOnly);
        int restoredHeadSceneId = session.HeadQuestSceneId != 0 ? session.HeadQuestSceneId : restoredSceneId;
        int restoredChapterId = session.CurrentEventQuestChapterId != 0 ? session.CurrentEventQuestChapterId : request.EventQuestChapterId;
        progressStatus.CurrentEventQuestChapterId = restoredChapterId;
        progressStatus.CurrentQuestId = questId;
        progressStatus.CurrentQuestSceneId = restoredSceneId;
        progressStatus.HeadQuestSceneId = restoredHeadSceneId;
        UpdateQuestSessionState(session, restoredSceneId, restoredHeadSceneId, QuestFlowType.SUB_FLOW, restoredChapterId, session.IsBattleOnly);

        RestartEventQuestResponse response = new();
        response.BattleDropReward.AddRange(_questEngine.BuildBattleDropRewards(questId));
        response.BattleBinary = LastBattleBinary(userDb, userId, questId);
        response.DeckNumber = session.UserDeckNumber;
        return Task.FromResult(response);
    }

    /// <summary>Completes an event quest: evaluates rewards, applies possessions, updates quest state, and returns results.</summary>
    public override Task<FinishEventQuestResponse> FinishEventQuest(FinishEventQuestRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        if (!_store.TryGet(userId, out DarkUserMemoryDatabase userDb)) return Task.FromResult(new FinishEventQuestResponse());

        long nowMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();
        int questId = request.QuestId;
        bool isWin = !request.IsRetired && !request.IsAnnihilated;
        if (!isWin)
        {
            _missions.RecordQuestBattleLoss(userDb, request.IsRetired, request.IsAnnihilated, nowMs);
        }

        EntityMQuest? quest = _questEngine.FindQuest(questId);
        if (quest == null) return Task.FromResult(new FinishEventQuestResponse());

        QuestFinishResult result = _questEngine.Finish(userDb, quest, ResolveEventQuestCampaignTarget(userDb, userId, questId), isWin, nowMs, battles: BattlesOfRun(userDb, userId, questId));

        // Reset event quest progress (always)
        EntityIUserEventQuestProgressStatus? progressStatus = userDb.EntityIUserEventQuestProgressStatus
            .FirstOrDefault(s => s.UserId == userId);
        if (progressStatus != null)
        {
            progressStatus.CurrentQuestId = 0;
            progressStatus.CurrentQuestSceneId = 0;
            progressStatus.HeadQuestSceneId = 0;
        }

        FinishEventQuestResponse response = new() { IsBigWin = result.IsBigWin };
        response.DropReward.AddRange(result.DropRewards);
        response.FirstClearReward.AddRange(result.FirstClearRewards);
        response.MissionClearReward.AddRange(result.MissionClearRewards);
        response.MissionClearCompleteReward.AddRange(result.MissionClearCompleteRewards);
        if (result.IsBigWin)
        {
            response.BigWinClearedQuestMissionIdList.Add(result.BigWinMissionId);
        }

        response.AutoOrbitReward = AdvanceQuestAutoOrbit(
            userDb,
            userId,
            request.IsAutoOrbit,
            request.IsRetired,
            request.IsAnnihilated,
            QuestType.EVENT_QUEST,
            request.EventQuestChapterId,
            questId,
            result.DropRewards,
            nowMs);

        if (isWin)
        {
            RecordLimitContentClear(userDb, request.EventQuestChapterId, questId, nowMs);
        }

        userDb.EntitySQuestSession.RemoveAll(s => s.QuestId == questId);

        return Task.FromResult(response);
    }

    /// <summary>
    /// A won Recollections of Dusk quest: marked cleared, with the run's deck locked for the rest of its floor
    /// (lifted once the whole floor is cleared). Must run before the quest session is removed.
    /// </summary>
    private void RecordLimitContentClear(DarkUserMemoryDatabase userDb, int chapterId, int questId, long nowMs)
    {
        if (!RecollectionsHelper.IsRecollectionsChapter(_masterDb, chapterId))
        {
            return;
        }

        RecollectionsHelper.RecordClear(_masterDb, userDb, chapterId, questId, QuestEngine.SessionDeck(userDb, questId), nowMs);
    }

    /// <summary>
    /// Ends the auto-orbit loop: returns the drops collected across its cleared runs (already granted run by run)
    /// and clears the loop's counters and collected drops.
    /// </summary>
    public override Task<FinishAutoOrbitResponse> FinishAutoOrbit(Empty request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        FinishAutoOrbitResponse response = new() { AutoOrbitReward = new QuestAutoOrbitResult() };
        if (_store.TryGet(userId, out DarkUserMemoryDatabase? userDb))
        {
            response.AutoOrbitReward.DropReward.AddRange(CollectedAutoOrbitDrops(userDb));
            userDb.EntityIUserQuestAutoOrbit.RemoveAll(s => s.UserId == userId);
            userDb.EntitySQuestAutoOrbit.Clear();
        }

        return Task.FromResult(response);
    }

    /// <summary>Sets the active main quest route and updates season route tracking.</summary>
    public override Task<SetRouteResponse> SetRoute(SetRouteRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        int routeId = request.MainQuestRouteId;

        EntityMMainQuestRoute? route = _masterDb.EntityMMainQuestRoute.FirstOrDefault(r => r.MainQuestRouteId == routeId);

        EntityIUserMainQuestMainFlowStatus mainFlowStatus = userDb.EntityIUserMainQuestMainFlowStatus.GetOrCreate(userId);
        ApplyMainQuestRoute(userDb, userId, mainFlowStatus, routeId, route?.MainQuestSeasonId ?? 0);

        EntityIUserPortalCageStatus portalCageStatus = userDb.EntityIUserPortalCageStatus.GetOrCreate(userId);
        portalCageStatus.IsCurrentProgress = false;

        return Task.FromResult(new SetRouteResponse());
    }

    /// <summary>Stores the selected quest-scene choice and updates replay-route state when applicable.</summary>
    public override Task<SetQuestSceneChoiceResponse> SetQuestSceneChoice(SetQuestSceneChoiceRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);
        long nowMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();

        QuestFlowType requestedFlowType = System.Enum.IsDefined(typeof(QuestFlowType), request.QuestFlowType)
            ? (QuestFlowType)request.QuestFlowType
            : QuestFlowType.UNKNOWN;

        EntityMQuestSceneChoice? masterChoice = ResolveQuestSceneChoice(request.QuestSceneId, request.ChoiceNumber, requestedFlowType);
        if (masterChoice == null)
            return Task.FromResult(new SetQuestSceneChoiceResponse());

        EntityMQuestSceneChoiceEffect? choiceEffect = _masterDb.EntityMQuestSceneChoiceEffect
            .FirstOrDefault(e => e.QuestSceneChoiceEffectId == masterChoice.QuestSceneChoiceEffectId);
        if (choiceEffect != null)
        {
            bool isFirstChoiceEffectSelection = !userDb.EntityIUserQuestSceneChoiceHistory.Any(
                c => c.UserId == userId && c.QuestSceneChoiceEffectId == choiceEffect.QuestSceneChoiceEffectId);

            EntityIUserQuestSceneChoice userChoice = userDb.EntityIUserQuestSceneChoice.GetOrCreate(
                c => c.UserId == userId && c.QuestSceneChoiceGroupingId == choiceEffect.QuestSceneChoiceGroupingId,
                () => new EntityIUserQuestSceneChoice
                {
                    UserId = userId,
                    QuestSceneChoiceGroupingId = choiceEffect.QuestSceneChoiceGroupingId
                });
            userChoice.QuestSceneChoiceEffectId = choiceEffect.QuestSceneChoiceEffectId;

            EntityIUserQuestSceneChoiceHistory userChoiceHistory = userDb.EntityIUserQuestSceneChoiceHistory.GetOrCreate(
                c => c.UserId == userId && c.QuestSceneChoiceEffectId == choiceEffect.QuestSceneChoiceEffectId,
                () => new EntityIUserQuestSceneChoiceHistory
                {
                    UserId = userId,
                    QuestSceneChoiceEffectId = choiceEffect.QuestSceneChoiceEffectId
                });
            userChoiceHistory.ChoiceDatetime = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();

            if (isFirstChoiceEffectSelection)
                ApplyQuestSceneChoiceSideEffects(userDb, choiceEffect, nowMs);
        }

        if (requestedFlowType == QuestFlowType.ANOTHER_ROUTE_REPLAY_FLOW)
        {
            EntityIUserMainQuestFlowStatus flowStatus = userDb.EntityIUserMainQuestFlowStatus.GetOrCreate(userId);
            flowStatus.CurrentQuestFlowType = QuestFlowType.ANOTHER_ROUTE_REPLAY_FLOW;

            MainQuestSceneContext? sceneContext = ResolveMainQuestSceneContext(request.QuestSceneId);
            if (sceneContext != null)
            {
                EntityIUserMainQuestMainFlowStatus mainFlowStatus = userDb.EntityIUserMainQuestMainFlowStatus.GetOrCreate(userId);
                ApplyMainQuestRoute(userDb, userId, mainFlowStatus, sceneContext.RouteId, sceneContext.SeasonId);
            }

            EntityMQuestScene? scene = _masterDb.EntityMQuestScene.FirstOrDefault(s => s.QuestSceneId == request.QuestSceneId);
            if (scene != null)
            {
                EntitySQuestSession? session = userDb.EntitySQuestSession.FirstOrDefault(s => s.UserId == userId && s.QuestId == scene.QuestId);
                if (session != null)
                    session.CurrentQuestFlowType = QuestFlowType.ANOTHER_ROUTE_REPLAY_FLOW;
            }
        }

        return Task.FromResult(new SetQuestSceneChoiceResponse());
    }

    /// <summary>
    /// Abyss Tower mission milestones: every tier up to the count the client asks for, as far as the user's cleared quest
    /// missions in the chapter (all difficulties) reach, each tier once.
    /// </summary>
    public override Task<ReceiveTowerAccumulationRewardResponse> ReceiveTowerAccumulationReward(ReceiveTowerAccumulationRewardRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);
        long nowMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();
        EntityIUserEventQuestTowerAccumulationReward towerRewardState = userDb.EntityIUserEventQuestTowerAccumulationReward.GetOrCreate(
            r => r.EventQuestChapterId == request.EventQuestChapterId,
            () => new EntityIUserEventQuestTowerAccumulationReward
            {
                UserId = userDb.UserId,
                EventQuestChapterId = request.EventQuestChapterId
            });

        int accumulationRewardGroupId = _masterDb.EntityMEventQuestTowerAccumulationReward
            .Where(r => r.EventQuestChapterId == request.EventQuestChapterId)
            .Select(r => r.EventQuestTowerAccumulationRewardGroupId)
            .FirstOrDefault();
        if (accumulationRewardGroupId == 0)
        {
            return Task.FromResult(new ReceiveTowerAccumulationRewardResponse());
        }

        int clearedCount = FateBoardHelper.ClearedQuestMissionCount(userDb, _masterDb,
            FateBoardHelper.ChapterQuests(_masterDb, request.EventQuestChapterId, difficulty: null).Select(q => q.QuestId));
        int claimUpTo = Math.Min(request.TargetMissionClearCount, clearedCount);
        List<EntityMEventQuestTowerAccumulationRewardGroup> eligibleRewards = [.. _masterDb.EntityMEventQuestTowerAccumulationRewardGroup
            .Where(r => r.EventQuestTowerAccumulationRewardGroupId == accumulationRewardGroupId
                && r.QuestMissionClearCount > towerRewardState.LatestRewardReceiveQuestMissionClearCount
                && r.QuestMissionClearCount <= claimUpTo)
            .OrderBy(r => r.QuestMissionClearCount)];

        int highestClaimedCount = towerRewardState.LatestRewardReceiveQuestMissionClearCount;
        foreach (EntityMEventQuestTowerAccumulationRewardGroup tier in eligibleRewards)
        {
            foreach (EntityMEventQuestTowerRewardGroup reward in _masterDb.EntityMEventQuestTowerRewardGroup
                .Where(r => r.EventQuestTowerRewardGroupId == tier.EventQuestTowerRewardGroupId)
                .OrderBy(r => r.SortOrder))
            {
                PossessionHelper.Apply(userDb, reward.PossessionType, reward.PossessionId, reward.Count, _masterDb, nowMs);
            }

            highestClaimedCount = Math.Max(highestClaimedCount, tier.QuestMissionClearCount);
        }

        if (highestClaimedCount > towerRewardState.LatestRewardReceiveQuestMissionClearCount)
        {
            towerRewardState.LatestRewardReceiveQuestMissionClearCount = highestClaimedCount;
        }

        return Task.FromResult(new ReceiveTowerAccumulationRewardResponse());
    }

    /// <summary>Skips a quest using consumable items: applies EXP/gold, marks cleared, and returns drop rewards.</summary>
    public override Task<SkipQuestResponse> SkipQuest(SkipQuestRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        long nowMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();
        int questId = request.QuestId;

        // Consume skip tickets
        foreach (UseEffectItem item in request.UseEffectItem)
        {
            EntityIUserConsumableItem? consumable = userDb.EntityIUserConsumableItem.FirstOrDefault(c => c.ConsumableItemId == item.ConsumableItemId);
            if (consumable != null)
            {
                consumable.Count = Math.Max(consumable.Count - item.Count, 0);
            }
        }

        EntityMQuest? quest = _questEngine.FindQuest(questId);
        EntityIUserQuest userQuest = QuestEngine.GetOrCreateUserQuest(userDb, questId);

        // Simulate clearing the quest SkipCount times, awarding EXP/gold on each iteration
        List<QuestReward> dropRewards = [];
        if (quest != null)
        {
            _questEngine.ConsumeStamina(userDb, quest, ResolveEventQuestCampaignTarget(userDb, userId, questId), request.SkipCount, nowMs);
            for (int i = 0; i < request.SkipCount; i++)
            {
                dropRewards.AddRange(_questEngine.ApplySkippedClear(userDb, quest, ResolveEventQuestCampaignTarget(userDb, userId, questId), nowMs));
            }

            QuestEngine.ResetStaleDailyClearCount(userQuest, nowMs);
            userQuest.ClearCount += request.SkipCount;
            userQuest.DailyClearCount += request.SkipCount;
            userQuest.LastClearDatetime = nowMs;
        }

        SkipQuestResponse response = new();
        response.DropReward.AddRange(dropRewards);
        return Task.FromResult(response);
    }

    /// <summary>Stub for bulk quest skip; returns empty response.</summary>
    public override Task<SkipQuestBulkResponse> SkipQuestBulk(SkipQuestBulkRequest request, ServerCallContext context)
    {
        return Task.FromResult(new SkipQuestBulkResponse());
    }

    /// <summary>Replaces the user's auto-sale settings with the provided item filter list.</summary>
    public override Task<SetAutoSaleSettingResponse> SetAutoSaleSetting(SetAutoSaleSettingRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        userDb.EntityIUserAutoSaleSettingDetail.RemoveAll(s => s.UserId == userId);

        foreach ((int itemType, string itemValue) in request.AutoSaleSettingItem)
        {
            userDb.EntityIUserAutoSaleSettingDetail.Add(new EntityIUserAutoSaleSettingDetail
            {
                UserId = userId,
                PossessionAutoSaleItemType = itemType,
                PossessionAutoSaleItemValue = itemValue,
            });
        }

        return Task.FromResult(new SetAutoSaleSettingResponse());
    }

    /// <summary>Opens a guerrilla free quest event window for the user.</summary>
    public override Task<StartGuerrillaFreeOpenResponse> StartGuerrillaFreeOpen(Empty request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        long nowMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();

        EntityIUserEventQuestGuerrillaFreeOpen freeOpen = userDb.EntityIUserEventQuestGuerrillaFreeOpen.GetOrCreate(userId);
        freeOpen.StartDatetime = nowMs;
        freeOpen.OpenMinutes = 60;
        freeOpen.DailyOpenedCount++;

        return Task.FromResult(new StartGuerrillaFreeOpenResponse());
    }

    /// <summary>
    /// Resets a Recollections of Dusk floor (the floor of the request's quest): no quest cleared, nothing locked. Side-story
    /// rows are left alone: the client reads only the current-scene row, and the floor stays the one being played.
    /// </summary>
    public override Task<ResetLimitContentQuestProgressResponse> ResetLimitContentQuestProgress(ResetLimitContentQuestProgressRequest request, ServerCallContext context)
    {
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(context.GetUserId());
        RecollectionsHelper.ResetFloor(_masterDb, userDb, request.EventQuestChapterId, request.QuestId);
        return Task.FromResult(new ResetLimitContentQuestProgressResponse());
    }

    /// <summary>Stub for daily quest group reward; returns empty response.</summary>
    public override Task<ReceiveDailyQuestGroupCompleteRewardResponse> ReceiveDailyQuestGroupCompleteReward(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new ReceiveDailyQuestGroupCompleteRewardResponse());
    }

    private static void StartQuestAutoOrbit(
        DarkUserMemoryDatabase userDb,
        long userId,
        QuestType questType,
        int chapterId,
        int questId,
        int maxCount,
        long nowMs)
    {
        if (maxCount <= 0)
        {
            userDb.EntityIUserQuestAutoOrbit.RemoveAll(s => s.UserId == userId);
            userDb.EntitySQuestAutoOrbit.Clear();
            return;
        }

        maxCount = Math.Min(maxCount, MaxAutoOrbitLoops);
        EntityIUserQuestAutoOrbit state = userDb.EntityIUserQuestAutoOrbit.GetOrCreate(
            s => s.UserId == userId,
            () => new EntityIUserQuestAutoOrbit { UserId = userId });

        if (state.MaxAutoOrbitCount > 0
            && state.QuestType == questType
            && state.ChapterId == chapterId
            && state.QuestId == questId
            && state.MaxAutoOrbitCount == maxCount)
        {
            return;
        }

        state.QuestType = questType;
        state.ChapterId = chapterId;
        state.QuestId = questId;
        state.MaxAutoOrbitCount = maxCount;
        state.ClearedAutoOrbitCount = 0;
        state.LastClearDatetime = 0;
        userDb.EntitySQuestAutoOrbit.Clear();
    }

    /// <summary>
    /// Counts a finished run of the tracked auto-orbit loop and collects its drops. When this run ends the loop
    /// (last run, retire, annihilation or auto-orbit switched off), the result lists every collected drop;
    /// otherwise it is empty. The drops stay collected for <c>FinishAutoOrbit</c>.
    /// </summary>
    private QuestAutoOrbitResult AdvanceQuestAutoOrbit(
        DarkUserMemoryDatabase userDb,
        long userId,
        bool isAutoOrbit,
        bool isRetired,
        bool isAnnihilated,
        QuestType questType,
        int chapterId,
        int questId,
        IEnumerable<QuestReward> drops,
        long nowMs)
    {
        QuestAutoOrbitResult result = new();
        EntityIUserQuestAutoOrbit? state = userDb.EntityIUserQuestAutoOrbit.FirstOrDefault(s => s.UserId == userId);
        if (state == null
            || state.MaxAutoOrbitCount <= 0
            || state.QuestType != questType
            || state.ChapterId != chapterId
            || state.QuestId != questId)
        {
            return result;
        }

        if (!isRetired && !isAnnihilated)
        {
            state.ClearedAutoOrbitCount++;
            EntitySQuestAutoOrbit collected = userDb.EntitySQuestAutoOrbit.GetOrCreate(
                s => s.UserId == userId,
                () => new EntitySQuestAutoOrbit { UserId = userId });
            collected.Drops.AddRange(drops.Select(d => new EntitySQuestAutoOrbitDrop
            {
                PossessionType = d.PossessionType,
                PossessionId = d.PossessionId,
                Count = d.Count,
                IsAutoSale = d.IsAutoSale
            }));
        }

        state.LastClearDatetime = nowMs;

        bool loopEnded = !isAutoOrbit || isRetired || isAnnihilated || state.ClearedAutoOrbitCount >= state.MaxAutoOrbitCount;
        if (loopEnded)
        {
            result.DropReward.AddRange(CollectedAutoOrbitDrops(userDb));
        }

        return result;
    }

    private IEnumerable<QuestReward> CollectedAutoOrbitDrops(DarkUserMemoryDatabase userDb)
        => userDb.EntitySQuestAutoOrbit
            .SelectMany(s => s.Drops)
            .Select(d => new QuestReward
            {
                PossessionType = d.PossessionType,
                PossessionId = d.PossessionId,
                Count = d.Count,
                IsAutoSale = d.IsAutoSale,
                RewardEffectId = RewardEffectHelper.EffectId(_masterDb, (PossessionType)d.PossessionType, d.PossessionId)
            });

    /// <summary>Returns true if <paramref name="newSceneId"/> is further along in narrative order than <paramref name="currentHeadId"/>, comparing by SortOrder (not scene ID).</summary>
    private bool IsSceneAhead(int newSceneId, int currentHeadId)
    {
        if (currentHeadId == 0) return true;
        EntityMQuestScene? newScene = _masterDb.EntityMQuestScene.FirstOrDefault(s => s.QuestSceneId == newSceneId);
        EntityMQuestScene? headScene = _masterDb.EntityMQuestScene.FirstOrDefault(s => s.QuestSceneId == currentHeadId);
        if (newScene == null || headScene == null) return false;
        return newScene.SortOrder > headScene.SortOrder;
    }

    /// <summary>
    /// Returns the last scene ID (by SortOrder) of the last quest in the same chapter group as <paramref name="questId"/>.
    /// Falls back to the last scene of <paramref name="questId"/> itself if chapter data is unavailable.
    /// </summary>
    private int GetChapterLastSceneId(int questId)
    {
        EntityMMainQuestSequence? seq = _masterDb.EntityMMainQuestSequence.FirstOrDefault(s => s.QuestId == questId);
        if (seq == null) return GetLastSceneIdForQuest(questId);

        EntityMMainQuestSequenceGroup? seqGroup = _masterDb.EntityMMainQuestSequenceGroup
            .FirstOrDefault(g => g.MainQuestSequenceId == seq.MainQuestSequenceId);
        if (seqGroup == null) return GetLastSceneIdForQuest(questId);

        int groupId = seqGroup.MainQuestSequenceGroupId;

        HashSet<int> seqIdsInGroup = [.. _masterDb.EntityMMainQuestSequenceGroup
            .Where(g => g.MainQuestSequenceGroupId == groupId)
            .Select(g => g.MainQuestSequenceId)];

        IOrderedEnumerable<EntityMMainQuestSequence> questsInChapter = _masterDb.EntityMMainQuestSequence
            .Where(s => seqIdsInGroup.Contains(s.MainQuestSequenceId))
            .OrderByDescending(s => s.SortOrder);

        foreach (EntityMMainQuestSequence sequence in questsInChapter)
        {
            int lastScene = GetLastSceneIdForQuest(sequence.QuestId);
            if (lastScene != 0) return lastScene;
        }

        return GetLastSceneIdForQuest(questId);
    }

    /// <summary>Returns the highest SortOrder scene ID for the given quest, or 0 if none exist.</summary>
    private int GetLastSceneIdForQuest(int questId)
    {
        return _masterDb.EntityMQuestScene
            .Where(s => s.QuestId == questId)
            .OrderByDescending(s => s.SortOrder)
            .FirstOrDefault()?.QuestSceneId ?? 0;
    }

    /// <summary>
    /// When scene progress moves into another quest, clears the quest the progress status was in if it is
    /// a background quest that never got cleared. Playable quests are left alone: their own finish call
    /// clears them.
    /// </summary>
    private void FinalizeChainPreviousQuest(DarkUserMemoryDatabase userDb, long userId, int questId, long nowMs)
    {
        EntityIUserMainQuestProgressStatus? progressStatus = userDb.EntityIUserMainQuestProgressStatus.FirstOrDefault(s => s.UserId == userId);
        if (progressStatus == null || progressStatus.CurrentQuestSceneId == 0)
        {
            return;
        }

        EntityMQuestScene? previousScene = _masterDb.EntityMQuestScene.FirstOrDefault(s => s.QuestSceneId == progressStatus.CurrentQuestSceneId);
        if (previousScene == null || previousScene.QuestId == questId)
        {
            return;
        }

        EntityMQuest? previousQuest = _questEngine.FindQuest(previousScene.QuestId);
        if (previousQuest == null || QuestEngine.IsPlayable(previousQuest))
        {
            return;
        }

        EntityIUserQuest previousUserQuest = _questEngine.EnsureQuestRows(userDb, previousQuest.QuestId);
        if (previousUserQuest.QuestStateType == (int)QuestStateType.CLEARED)
        {
            return;
        }

        _questEngine.FinalizePassedQuest(userDb, previousQuest, nowMs);
    }

    /// <summary>
    /// Starts a quest picked from the quest menu. Only the progress status points at the quest; the story
    /// position (main-flow status, route, season) is never touched. Flow type, portal-cage and side-story
    /// state are saved in the session and put back at finish.
    /// </summary>
    private void StartMenuPickQuest(DarkUserMemoryDatabase userDb, long userId, EntityMQuest? quest, EntityIUserQuest userQuest, StartMainQuestRequest request, long nowMs)
    {
        EntityIUserMainQuestFlowStatus flowStatus = userDb.EntityIUserMainQuestFlowStatus.GetOrCreate(userId);
        EntityIUserPortalCageStatus portalCageStatus = userDb.EntityIUserPortalCageStatus.GetOrCreate(userId);
        EntityIUserSideStoryQuestSceneProgressStatus sideStoryProgress = userDb.EntityIUserSideStoryQuestSceneProgressStatus.GetOrCreate(userId);

        EntitySQuestSession session = GetOrCreateQuestSession(userDb, userId, request.QuestId);
        if (!session.IsMenuPick)
        {
            session.IsMenuPick = true;
            session.SavedQuestFlowType = flowStatus.CurrentQuestFlowType;
            session.SavedPortalCageInProgress = portalCageStatus.IsCurrentProgress;
            session.SavedSideStoryQuestId = sideStoryProgress.CurrentSideStoryQuestId;
            session.SavedSideStoryQuestSceneId = sideStoryProgress.CurrentSideStoryQuestSceneId;
        }

        int startSceneId = _questEngine.GetQuestStartSceneId(request.QuestId, request.IsBattleOnly);
        _questEngine.SetSessionDeck(session, request.UserDeckNumber);
        session.IsBattleOnly = request.IsBattleOnly;
        session.CurrentQuestFlowType = QuestFlowType.MAIN_FLOW;
        session.CurrentQuestSceneId = startSceneId;
        session.HeadQuestSceneId = startSceneId;

        EntityIUserMainQuestProgressStatus progressStatus = userDb.EntityIUserMainQuestProgressStatus.GetOrCreate(userId);
        progressStatus.CurrentQuestSceneId = startSceneId;
        progressStatus.HeadQuestSceneId = startSceneId;
        progressStatus.CurrentQuestFlowType = QuestFlowType.MAIN_FLOW;

        portalCageStatus.IsCurrentProgress = false;
        sideStoryProgress.CurrentSideStoryQuestId = 0;
        sideStoryProgress.CurrentSideStoryQuestSceneId = 0;

        userQuest.IsBattleOnly = request.IsBattleOnly;
        bool isPlayable = QuestEngine.IsPlayable(quest);
        if (isPlayable)
        {
            QuestEngine.MarkActive(userQuest, nowMs);
        }
        else if (userQuest.ClearCount == 0)
        {
            QuestEngine.MarkCleared(userQuest, 1, nowMs);
        }

        StartQuestAutoOrbit(userDb, userId, QuestType.MAIN_QUEST, 0, request.QuestId, request.MaxAutoOrbitCount, nowMs);
    }

    /// <summary>Puts back the flow type, portal-cage and side-story state saved when a menu pick started.</summary>
    private static void RestoreMenuPickContext(DarkUserMemoryDatabase userDb, long userId, EntitySQuestSession session, long nowMs)
    {
        EntityIUserMainQuestFlowStatus flowStatus = userDb.EntityIUserMainQuestFlowStatus.GetOrCreate(userId);
        flowStatus.CurrentQuestFlowType = session.SavedQuestFlowType;

        EntityIUserPortalCageStatus portalCageStatus = userDb.EntityIUserPortalCageStatus.GetOrCreate(userId);
        portalCageStatus.IsCurrentProgress = session.SavedPortalCageInProgress;

        EntityIUserSideStoryQuestSceneProgressStatus sideStoryProgress = userDb.EntityIUserSideStoryQuestSceneProgressStatus.GetOrCreate(userId);
        sideStoryProgress.CurrentSideStoryQuestId = session.SavedSideStoryQuestId;
        sideStoryProgress.CurrentSideStoryQuestSceneId = session.SavedSideStoryQuestSceneId;
    }

    private static EntitySQuestSession GetOrCreateQuestSession(DarkUserMemoryDatabase userDb, long userId, int questId)
    {
        return userDb.EntitySQuestSession.GetOrCreate(
            s => s.UserId == userId && s.QuestId == questId,
            () => new EntitySQuestSession { UserId = userId, QuestId = questId });
    }

    private void UpdateQuestSessionState(
        EntitySQuestSession session,
        int currentQuestSceneId,
        int headQuestSceneId,
        QuestFlowType questFlowType,
        int currentEventQuestChapterId = 0,
        bool? isBattleOnly = null)
    {
        session.CurrentQuestSceneId = currentQuestSceneId;
        session.HeadQuestSceneId = headQuestSceneId != 0 ? headQuestSceneId : currentQuestSceneId;
        session.CurrentQuestFlowType = questFlowType;
        if (currentEventQuestChapterId != 0)
            session.CurrentEventQuestChapterId = currentEventQuestChapterId;
        if (isBattleOnly.HasValue)
            session.IsBattleOnly = isBattleOnly.Value;
    }

    private int GetHeadSceneId(int currentHeadSceneId, int newQuestSceneId)
    {
        if (newQuestSceneId == 0)
            return currentHeadSceneId;
        return IsSceneAhead(newQuestSceneId, currentHeadSceneId) ? newQuestSceneId : currentHeadSceneId;
    }

    private void ApplyMainFlowSceneProgress(
        DarkUserMemoryDatabase userDb,
        long userId,
        int questSceneId,
        MainQuestSceneContext? sceneContext)
    {
        // The story position only moves forward; a scene behind the head leaves it, and the route, alone.
        EntityIUserMainQuestMainFlowStatus mainFlowStatus = userDb.EntityIUserMainQuestMainFlowStatus.GetOrCreate(userId);
        if (!IsSceneAhead(questSceneId, mainFlowStatus.HeadQuestSceneId))
        {
            return;
        }

        mainFlowStatus.CurrentQuestSceneId = questSceneId;
        mainFlowStatus.HeadQuestSceneId = questSceneId;

        if (sceneContext == null)
        {
            return;
        }

        mainFlowStatus.IsReachedLastQuestScene = questSceneId == GetChapterLastSceneId(sceneContext.Scene.QuestId);
        ApplyMainQuestRoute(userDb, userId, mainFlowStatus, sceneContext.RouteId, sceneContext.SeasonId);
    }

    private void ApplyMainQuestRoute(
        DarkUserMemoryDatabase userDb,
        long userId,
        EntityIUserMainQuestMainFlowStatus mainFlowStatus,
        int routeId,
        int seasonId)
    {
        if (routeId == 0)
        {
            return;
        }

        mainFlowStatus.CurrentMainQuestRouteId = routeId;

        // Each season records the first route taken there (only season 2 has two routes); it is never
        // overwritten. Deviation: a real save has no season-1 row, but we write one too so every season is
        // treated the same.
        if (seasonId > 0 && !userDb.EntityIUserMainQuestSeasonRoute.Any(r => r.UserId == userId && r.MainQuestSeasonId == seasonId))
        {
            userDb.EntityIUserMainQuestSeasonRoute.AddNew(new EntityIUserMainQuestSeasonRoute { UserId = userId, MainQuestSeasonId = seasonId, MainQuestRouteId = routeId });
        }
    }

    /// <summary>
    /// A replay of a quest on a route other than the one first taken in its season is an another-route replay.
    /// </summary>
    private QuestFlowType ResolveReplayFlowType(DarkUserMemoryDatabase userDb, long userId, int questId)
    {
        MainQuestSceneContext? sceneContext = ResolveMainQuestSceneContext(_questEngine.GetQuestStartSceneId(questId, false));
        if (sceneContext == null || sceneContext.RouteId == 0)
        {
            return QuestFlowType.REPLAY_FLOW;
        }

        EntityIUserMainQuestSeasonRoute? seasonRoute = userDb.EntityIUserMainQuestSeasonRoute
            .FirstOrDefault(r => r.UserId == userId && r.MainQuestSeasonId == sceneContext.SeasonId);
        return seasonRoute != null && seasonRoute.MainQuestRouteId != sceneContext.RouteId
            ? QuestFlowType.ANOTHER_ROUTE_REPLAY_FLOW
            : QuestFlowType.REPLAY_FLOW;
    }

    private MainQuestSceneContext? ResolveMainQuestSceneContext(int questSceneId)
    {
        EntityMQuestScene? scene = _masterDb.EntityMQuestScene.FirstOrDefault(s => s.QuestSceneId == questSceneId);
        if (scene == null)
        {
            return null;
        }

        int routeId = 0;
        int seasonId = 0;

        EntityMMainQuestSequence? sequence = _masterDb.EntityMMainQuestSequence.FirstOrDefault(s => s.QuestId == scene.QuestId);
        if (sequence != null)
        {
            EntityMMainQuestSequenceGroup? sequenceGroup = _masterDb.EntityMMainQuestSequenceGroup
                .FirstOrDefault(g => g.MainQuestSequenceId == sequence.MainQuestSequenceId);
            if (sequenceGroup != null)
            {
                EntityMMainQuestChapter? chapter = _masterDb.EntityMMainQuestChapter
                    .FirstOrDefault(c => c.MainQuestSequenceGroupId == sequenceGroup.MainQuestSequenceGroupId);
                routeId = chapter?.MainQuestRouteId ?? 0;
                if (routeId != 0)
                {
                    seasonId = _masterDb.EntityMMainQuestRoute
                        .FirstOrDefault(r => r.MainQuestRouteId == routeId)?.MainQuestSeasonId ?? 0;
                }
            }
        }

        return new MainQuestSceneContext(scene, routeId, seasonId);
    }

    private QuestCampaignTarget ResolveEventQuestCampaignTarget(DarkUserMemoryDatabase userDb, long userId, int questId)
    {
        int eventQuestChapterId = userDb.EntitySQuestSession
            .FirstOrDefault(session => session.UserId == userId && session.QuestId == questId)
            ?.CurrentEventQuestChapterId
            ?? userDb.EntityIUserEventQuestProgressStatus.FirstOrDefault(status => status.UserId == userId)?.CurrentEventQuestChapterId
            ?? 0;

        return _questCampaignHelper.GetEventQuestTarget(questId, eventQuestChapterId);
    }

    private EntityMQuestSceneChoice? ResolveQuestSceneChoice(int questSceneId, int choiceNumber, QuestFlowType flowType)
    {
        IEnumerable<QuestFlowType> candidateFlowTypes = flowType switch
        {
            QuestFlowType.ANOTHER_ROUTE_REPLAY_FLOW => [QuestFlowType.ANOTHER_ROUTE_REPLAY_FLOW, QuestFlowType.REPLAY_FLOW, QuestFlowType.MAIN_FLOW],
            QuestFlowType.UNKNOWN => [QuestFlowType.MAIN_FLOW],
            _ => [flowType, QuestFlowType.MAIN_FLOW]
        };

        foreach (QuestFlowType candidateFlowType in candidateFlowTypes.Distinct())
        {
            EntityMQuestSceneChoice? choice = _masterDb.EntityMQuestSceneChoice.FirstOrDefault(c =>
                c.MainFlowQuestSceneId == questSceneId &&
                c.QuestFlowType == candidateFlowType &&
                c.ChoiceNumber == choiceNumber);
            if (choice != null)
                return choice;
        }

        return null;
    }

    private void ApplyQuestSceneChoiceSideEffects(DarkUserMemoryDatabase userDb, EntityMQuestSceneChoiceEffect choiceEffect, long nowMs)
    {
        // REVIEW: if quest-scene choice effects are supposed to compensate already-owned gear with duplicate rewards,
        // the current repo has no quest-choice-specific conversion model for that. Skip duplicate grants for safety.
        foreach (EntityMQuestSceneChoiceCostumeEffectGroup costumeEffect in _masterDb.EntityMQuestSceneChoiceCostumeEffectGroup
            .Where(g => g.QuestSceneChoiceCostumeEffectGroupId == choiceEffect.QuestSceneChoiceCostumeEffectGroupId)
            .OrderBy(g => g.SortOrder))
        {
            if (!userDb.EntityIUserCostume.Any(c => c.CostumeId == costumeEffect.CostumeId))
                PossessionHelper.GrantCostume(userDb, costumeEffect.CostumeId, _masterDb, nowMs);
        }

        foreach (EntityMQuestSceneChoiceWeaponEffectGroup weaponEffect in _masterDb.EntityMQuestSceneChoiceWeaponEffectGroup
            .Where(g => g.QuestSceneChoiceWeaponEffectGroupId == choiceEffect.QuestSceneChoiceWeaponEffectGroupId)
            .OrderBy(g => g.SortOrder))
        {
            if (!userDb.EntityIUserWeapon.Any(w => w.WeaponId == weaponEffect.WeaponId))
                WeaponHelper.GrantWeapon(userDb, weaponEffect.WeaponId, _masterDb, nowMs);
        }
    }

    private sealed record MainQuestSceneContext(EntityMQuestScene Scene, int RouteId, int SeasonId);
}
