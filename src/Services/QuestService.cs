using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Helpers;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Quest;

namespace MariesWonderland.Services;

public class QuestService(UserDataStore store, DarkMasterMemoryDatabase masterDb) : MariesWonderland.Proto.Quest.QuestService.QuestServiceBase
{
    private readonly UserDataStore _store = store;
    private readonly DarkMasterMemoryDatabase _masterDb = masterDb;

    /// <summary>Advances the main story flow to the specified quest scene and updates route/progress tracking.</summary>
    public override Task<UpdateMainFlowSceneProgressResponse> UpdateMainFlowSceneProgress(UpdateMainFlowSceneProgressRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        int questSceneId = request.QuestSceneId;
        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        // Resolve quest ID from the scene, then walk the master data chain to find the route:
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

        // Update flow status to indicate the player is in the main story flow
        EntityIUserMainQuestFlowStatus flowStatus = userDb.EntityIUserMainQuestFlowStatus.GetOrCreate(userId);
        flowStatus.CurrentQuestFlowType = QuestFlowType.MAIN_FLOW;

        // Advance the main flow scene position and track the furthest scene reached
        EntityIUserMainQuestMainFlowStatus mainFlowStatus = userDb.EntityIUserMainQuestMainFlowStatus.GetOrCreate(userId);
        mainFlowStatus.CurrentQuestSceneId = questSceneId;
        if (IsSceneAhead(questSceneId, mainFlowStatus.HeadQuestSceneId))
            mainFlowStatus.HeadQuestSceneId = questSceneId;
        if (routeId != 0) mainFlowStatus.CurrentMainQuestRouteId = routeId;
        if (scene != null)
            mainFlowStatus.IsReachedLastQuestScene = questSceneId == GetChapterLastSceneId(scene.QuestId);

        // Clear side story active progress if the user was in a side story
        EntityIUserSideStoryQuestSceneProgressStatus sideStoryProgress = userDb.EntityIUserSideStoryQuestSceneProgressStatus.GetOrCreate(userId);
        if (sideStoryProgress.CurrentSideStoryQuestId != 0)
        {
            sideStoryProgress.CurrentSideStoryQuestId = 0;
            sideStoryProgress.CurrentSideStoryQuestSceneId = 0;
        }

        EntityIUserPortalCageStatus portalCageStatus = userDb.EntityIUserPortalCageStatus.GetOrCreate(userId);
        portalCageStatus.IsCurrentProgress = false;

        ApplySceneGrants(userDb, questSceneId, userId);

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

        return Task.FromResult(new UpdateReplayFlowSceneProgressResponse());
    }

    /// <summary>Advances sub-flow (playable quest) scene progress or main-flow (cutscene) position.</summary>
    public override Task<UpdateMainQuestSceneProgressResponse> UpdateMainQuestSceneProgress(UpdateMainQuestSceneProgressRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        int questSceneId = request.QuestSceneId;

        EntityMQuestScene? scene = _masterDb.EntityMQuestScene.FirstOrDefault(s => s.QuestSceneId == questSceneId);
        EntityMQuest? quest = scene != null ? _masterDb.EntityMQuest.FirstOrDefault(q => q.QuestId == scene.QuestId) : null;

        EntityIUserMainQuestMainFlowStatus mainFlowStatus = userDb.EntityIUserMainQuestMainFlowStatus.GetOrCreate(userId);

        bool isPlayable = quest != null && !quest.IsRunInTheBackground && quest.IsCountedAsQuest;
        if (isPlayable)
        {
            long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            // On half-result scenes, auto-clear all quest missions before the battle phase ends
            if (scene!.QuestResultType == QuestResultType.HALF_RESULT && quest!.QuestMissionGroupId != 0)
            {
                foreach (EntityMQuestMissionGroup missionGroupRow in _masterDb.EntityMQuestMissionGroup
                    .Where(m => m.QuestMissionGroupId == quest.QuestMissionGroupId))
                {
                    EntityIUserQuestMission userMission = userDb.EntityIUserQuestMission.GetOrCreate(
                        m => m.QuestId == scene.QuestId && m.QuestMissionId == missionGroupRow.QuestMissionId,
                        () => new EntityIUserQuestMission { UserId = userId, QuestId = scene.QuestId, QuestMissionId = missionGroupRow.QuestMissionId });
                    userMission.IsClear = true;
                    userMission.ProgressValue = 1;
                    userMission.LatestClearDatetime = nowMs;
                }
            }

            EntityIUserMainQuestFlowStatus flowStatus = userDb.EntityIUserMainQuestFlowStatus.GetOrCreate(userId);
            flowStatus.CurrentQuestFlowType = QuestFlowType.SUB_FLOW;

            EntityIUserMainQuestProgressStatus progressStatus = userDb.EntityIUserMainQuestProgressStatus.GetOrCreate(userId);
            progressStatus.CurrentQuestSceneId = questSceneId;
            if (IsSceneAhead(questSceneId, progressStatus.HeadQuestSceneId))
                progressStatus.HeadQuestSceneId = questSceneId;
            progressStatus.CurrentQuestFlowType = QuestFlowType.SUB_FLOW;
        }
        else if (scene != null)
        {
            // Non-playable quests (background, cutscene, etc.) update the main flow scene position
            mainFlowStatus.CurrentQuestSceneId = questSceneId;
            if (IsSceneAhead(questSceneId, mainFlowStatus.HeadQuestSceneId))
                mainFlowStatus.HeadQuestSceneId = questSceneId;
            mainFlowStatus.IsReachedLastQuestScene = questSceneId == GetChapterLastSceneId(scene.QuestId);
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
        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        EntityIUserExtraQuestProgressStatus progressStatus = userDb.EntityIUserExtraQuestProgressStatus.GetOrCreate(userId);
        progressStatus.CurrentQuestSceneId = questSceneId;
        if (IsSceneAhead(questSceneId, progressStatus.HeadQuestSceneId))
            progressStatus.HeadQuestSceneId = questSceneId;

        ApplySceneGrants(userDb, questSceneId, userId);

        EntityMQuestScene? scene = _masterDb.EntityMQuestScene.FirstOrDefault(s => s.QuestSceneId == questSceneId);
        if (scene != null && scene.QuestResultType == QuestResultType.HALF_RESULT)
        {
            ClearQuestMissions(userDb, scene.QuestId, userId, nowMs);
        }

        return Task.FromResult(new UpdateExtraQuestSceneProgressResponse());
    }

    /// <summary>Advances event quest scene progress.</summary>
    public override Task<UpdateEventQuestSceneProgressResponse> UpdateEventQuestSceneProgress(UpdateEventQuestSceneProgressRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        int questSceneId = request.QuestSceneId;
        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        EntityIUserEventQuestProgressStatus progressStatus = userDb.EntityIUserEventQuestProgressStatus.GetOrCreate(userId);
        progressStatus.CurrentQuestSceneId = questSceneId;
        if (IsSceneAhead(questSceneId, progressStatus.HeadQuestSceneId))
            progressStatus.HeadQuestSceneId = questSceneId;

        ApplySceneGrants(userDb, questSceneId, userId);

        EntityMQuestScene? scene = _masterDb.EntityMQuestScene.FirstOrDefault(s => s.QuestSceneId == questSceneId);
        if (scene != null && scene.QuestResultType == QuestResultType.HALF_RESULT)
        {
            ClearQuestMissions(userDb, scene.QuestId, userId, nowMs);
        }

        return Task.FromResult(new UpdateEventQuestSceneProgressResponse());
    }

    /// <summary>Initializes a main quest: creates quest/mission records, sets flow statuses, and returns battle drop rewards.</summary>
    public override Task<StartMainQuestResponse> StartMainQuest(StartMainQuestRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        int questId = request.QuestId;

        EntityMQuest? quest = _masterDb.EntityMQuest.FirstOrDefault(q => q.QuestId == questId);

        // Ensure user quest and mission rows exist before any state transitions.
        EntityIUserQuest userQuest = userDb.EntityIUserQuest.GetOrCreate(
            q => q.QuestId == questId,
            () => new EntityIUserQuest { UserId = userId, QuestId = questId });

        if (quest != null && quest.QuestMissionGroupId != 0)
        {
            List<int> missionIds = [.. _masterDb.EntityMQuestMissionGroup
                .Where(g => g.QuestMissionGroupId == quest.QuestMissionGroupId)
                .Select(g => g.QuestMissionId)];

            foreach (int missionId in missionIds)
            {
                if (!userDb.EntityIUserQuestMission.Any(m => m.QuestId == questId && m.QuestMissionId == missionId))
                    userDb.EntityIUserQuestMission.Add(new EntityIUserQuestMission { UserId = userId, QuestId = questId, QuestMissionId = missionId });
            }
        }

        // Already-cleared quests: handle replay flow or early-return.
        if (userQuest.QuestStateType == (int)QuestStateType.CLEARED)
        {
            if (request.IsReplayFlow)
            {
                EntityIUserMainQuestFlowStatus replayFlow = userDb.EntityIUserMainQuestFlowStatus.GetOrCreate(userId);
                replayFlow.CurrentQuestFlowType = QuestFlowType.REPLAY_FLOW;

                EntityIUserMainQuestReplayFlowStatus replayFlowStatus = userDb.EntityIUserMainQuestReplayFlowStatus.GetOrCreate(userId);
                replayFlowStatus.CurrentQuestSceneId = 0;
                replayFlowStatus.CurrentHeadQuestSceneId = 0;

                userQuest.QuestStateType = (int)QuestStateType.ACTIVE;
                userQuest.LatestStartDatetime = nowMs;
                userQuest.IsBattleOnly = request.IsBattleOnly;

                EntitySQuestSession replaySession = userDb.EntitySQuestSession.GetOrCreate(
                    s => s.QuestId == questId,
                    () => new EntitySQuestSession { UserId = userId, QuestId = questId });
                replaySession.UserDeckNumber = request.UserDeckNumber;
            }

            StartMainQuestResponse earlyResponse = new();
            earlyResponse.BattleDropReward.AddRange(BuildBattleDropRewards(questId));
            return Task.FromResult(earlyResponse);
        }

        userQuest.IsBattleOnly = request.IsBattleOnly;

        EntitySQuestSession startSession = userDb.EntitySQuestSession.GetOrCreate(
            s => s.QuestId == questId,
            () => new EntitySQuestSession { UserId = userId, QuestId = questId });
        startSession.UserDeckNumber = request.UserDeckNumber;

        bool isPlayable = quest != null && !quest.IsRunInTheBackground && quest.IsCountedAsQuest;
        if (isPlayable)
        {
            EntityIUserMainQuestFlowStatus flowStatus = userDb.EntityIUserMainQuestFlowStatus.GetOrCreate(userId);
            flowStatus.CurrentQuestFlowType = QuestFlowType.MAIN_FLOW;

            userQuest.QuestStateType = (int)QuestStateType.ACTIVE;
            userQuest.LatestStartDatetime = nowMs;

            // Ensure ProgressStatus singleton exists so it always appears in the DiffInterceptor delta.
            EntityIUserMainQuestProgressStatus progressStatus = userDb.EntityIUserMainQuestProgressStatus.GetOrCreate(userId);

            // For battle-only quests the client skips UpdateMainQuestSceneProgress, so ProgressStatus
            // would never have the battle scene set. Explicitly set it here.
            if (request.IsBattleOnly)
            {
                EntityMQuestScene? battleScene = _masterDb.EntityMQuestScene.FirstOrDefault(s => s.QuestId == questId && s.IsBattleOnlyTarget);
                if (battleScene != null)
                {
                    progressStatus.CurrentQuestSceneId = battleScene.QuestSceneId;
                    progressStatus.HeadQuestSceneId = Math.Max(progressStatus.HeadQuestSceneId, battleScene.QuestSceneId);
                    progressStatus.CurrentQuestFlowType = QuestFlowType.SUB_FLOW;
                }
            }

            // Ensure ReplayFlowStatus singleton exists so it is always included in the delta.
            EntityIUserMainQuestReplayFlowStatus replayFlowStatus = userDb.EntityIUserMainQuestReplayFlowStatus.GetOrCreate(userId);

            // Ensure MainFlowStatus singleton exists and always appears in the delta, even when unchanged.
            EntityIUserMainQuestMainFlowStatus mainFlowStatus = userDb.EntityIUserMainQuestMainFlowStatus.GetOrCreate(userId);

            // Ensure SeasonRoute singleton exists (season 1 is the default for all new users).
            if (!userDb.EntityIUserMainQuestSeasonRoute.Any(r => r.UserId == userId))
                userDb.EntityIUserMainQuestSeasonRoute.AddNew(new EntityIUserMainQuestSeasonRoute { UserId = userId, MainQuestSeasonId = 1, MainQuestRouteId = 1 });
        }
        else
        {
            // Background quests (cutscenes, non-combat) — auto-clear immediately.
            userQuest.QuestStateType = (int)QuestStateType.CLEARED;
            userQuest.ClearCount++;
            userQuest.DailyClearCount++;
            userQuest.LastClearDatetime = nowMs;

            // Set scene IDs from the first scene of this quest
            EntityMQuestScene? firstScene = _masterDb.EntityMQuestScene
                .Where(s => s.QuestId == questId)
                .OrderBy(s => s.SortOrder)
                .FirstOrDefault();
            if (firstScene != null)
            {
                EntityIUserMainQuestMainFlowStatus mainFlowStatus = userDb.EntityIUserMainQuestMainFlowStatus.GetOrCreate(userId);
                mainFlowStatus.CurrentQuestSceneId = firstScene.QuestSceneId;
                if (IsSceneAhead(firstScene.QuestSceneId, mainFlowStatus.HeadQuestSceneId))
                    mainFlowStatus.HeadQuestSceneId = firstScene.QuestSceneId;

                EntityIUserMainQuestFlowStatus flowStatus = userDb.EntityIUserMainQuestFlowStatus.GetOrCreate(userId);
                flowStatus.CurrentQuestFlowType = QuestFlowType.MAIN_FLOW;

                mainFlowStatus.IsReachedLastQuestScene = firstScene.QuestSceneId == GetChapterLastSceneId(questId);
            }
        }

        StartMainQuestResponse response = new();
        response.BattleDropReward.AddRange(BuildBattleDropRewards(questId));
        return Task.FromResult(response);
    }

    /// <summary>Restarts a main quest: resets flow to MAIN_FLOW, clears mission progress, and returns battle drop rewards.</summary>
    public override Task<RestartMainQuestResponse> RestartMainQuest(RestartMainQuestRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        int questId = request.QuestId;

        EntityIUserQuest userQuest = userDb.EntityIUserQuest.GetOrCreate(
            q => q.QuestId == questId,
            () => new EntityIUserQuest { UserId = userId, QuestId = questId });

        userQuest.QuestStateType = (int)QuestStateType.ACTIVE;
        userQuest.IsBattleOnly = false;
        userQuest.LatestStartDatetime = nowMs;

        EntitySQuestSession restartSession = userDb.EntitySQuestSession.GetOrCreate(
            s => s.QuestId == questId,
            () => new EntitySQuestSession { UserId = userId, QuestId = questId });

        // Reset flow to MAIN_FLOW on retry so the client re-enters the quest from the beginning.
        EntityIUserMainQuestFlowStatus flowStatus = userDb.EntityIUserMainQuestFlowStatus.GetOrCreate(userId);
        flowStatus.CurrentQuestFlowType = QuestFlowType.MAIN_FLOW;

        // Reset all mission progress for this quest so they can be re-evaluated during battle.
        foreach (EntityIUserQuestMission mission in userDb.EntityIUserQuestMission.Where(m => m.QuestId == questId))
        {
            mission.IsClear = false;
            mission.ProgressValue = 0;
            mission.LatestClearDatetime = 0;
        }

        RestartMainQuestResponse response = new();
        response.BattleDropReward.AddRange(BuildBattleDropRewards(questId));
        response.DeckNumber = restartSession.UserDeckNumber;
        return Task.FromResult(response);
    }

    /// <summary>Completes a main quest: evaluates rewards, applies EXP/gold/possessions, updates quest state, and returns results.</summary>
    public override Task<FinishMainQuestResponse> FinishMainQuest(FinishMainQuestRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        if (!_store.TryGet(userId, out DarkUserMemoryDatabase userDb)) return Task.FromResult(new FinishMainQuestResponse());

        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        int questId = request.QuestId;
        bool isRetired = request.IsRetired;

        EntityMQuest? quest = _masterDb.EntityMQuest.FirstOrDefault(q => q.QuestId == questId);
        if (quest == null) return Task.FromResult(new FinishMainQuestResponse());

        EntityIUserQuest userQuest = userDb.EntityIUserQuest.GetOrCreate(
            q => q.QuestId == questId,
            () => new EntityIUserQuest { UserId = userId, QuestId = questId });

        // === PHASE A: Evaluate outcome ===
        List<QuestReward> firstClearRewards = [];
        List<QuestReward> replayFlowFirstClearRewards = [];
        List<QuestReward> missionClearRewards = [];
        List<QuestReward> bigWinRewards = [];
        bool isBigWin = false;

        bool isFirstClear = userQuest.ClearCount == 0;

        List<EntityMQuestFirstClearRewardGroup> firstClearRewardRows = [];
        if (isFirstClear && quest.QuestFirstClearRewardGroupId != 0)
        {
            int rewardGroupId = quest.QuestFirstClearRewardGroupId;
            EntityMQuestFirstClearRewardSwitch? switchRow = _masterDb.EntityMQuestFirstClearRewardSwitch
                .FirstOrDefault(s => s.QuestId == questId);
            if (switchRow != null)
            {
                EntityIUserQuest? prereqQuest = userDb.EntityIUserQuest
                    .FirstOrDefault(q => q.QuestId == switchRow.SwitchConditionClearQuestId);
                if (prereqQuest != null && prereqQuest.QuestStateType == (int)QuestStateType.CLEARED)
                {
                    rewardGroupId = switchRow.QuestFirstClearRewardGroupId;
                }
            }

            firstClearRewardRows = [.. _masterDb.EntityMQuestFirstClearRewardGroup
                .Where(r => r.QuestFirstClearRewardGroupId == rewardGroupId)];
            firstClearRewards.AddRange(firstClearRewardRows.Select(r => new QuestReward
            {
                PossessionType = (int)r.PossessionType,
                PossessionId = r.PossessionId,
                Count = r.Count
            }));
        }

        // Build replay flow first-clear rewards if in replay flow
        EntityIUserMainQuestFlowStatus? currentFlowStatus = userDb.EntityIUserMainQuestFlowStatus.FirstOrDefault(s => s.UserId == userId);
        bool wasReplayFlow = currentFlowStatus?.CurrentQuestFlowType == QuestFlowType.REPLAY_FLOW;
        if (wasReplayFlow && quest.QuestReplayFlowRewardGroupId > 0)
        {
            foreach (EntityMQuestReplayFlowRewardGroup reward in _masterDb.EntityMQuestReplayFlowRewardGroup
                .Where(r => r.QuestReplayFlowRewardGroupId == quest.QuestReplayFlowRewardGroupId))
            {
                replayFlowFirstClearRewards.Add(new QuestReward
                {
                    PossessionType = (int)reward.PossessionType,
                    PossessionId = reward.PossessionId,
                    Count = reward.Count
                });
            }
        }

        List<EntityMQuestMissionGroup> missionGroupRows = quest.QuestMissionGroupId != 0
            ? [.. _masterDb.EntityMQuestMissionGroup.Where(m => m.QuestMissionGroupId == quest.QuestMissionGroupId)]
            : [];

        int regularMissionCount = 0;
        int pendingClearCount = 0;
        int bigWinMissionId = 0;

        foreach (EntityMQuestMissionGroup missionGroupRow in missionGroupRows)
        {
            EntityMQuestMission? mission = _masterDb.EntityMQuestMission
                .FirstOrDefault(m => m.QuestMissionId == missionGroupRow.QuestMissionId);
            if (mission == null) continue;

            if (mission.QuestMissionConditionType == QuestMissionConditionType.COMPLETE)
            {
                bigWinMissionId = mission.QuestMissionId;
                continue;
            }

            regularMissionCount++;

            EntityIUserQuestMission? userMission = userDb.EntityIUserQuestMission
                .FirstOrDefault(m => m.QuestId == questId && m.QuestMissionId == mission.QuestMissionId);
            if (userMission == null || !userMission.IsClear)
            {
                pendingClearCount++;
                EntityMQuestMissionReward? missionReward = _masterDb.EntityMQuestMissionReward
                    .FirstOrDefault(r => r.QuestMissionRewardId == mission.QuestMissionRewardId);
                if (missionReward != null)
                {
                    missionClearRewards.Add(new QuestReward
                    {
                        PossessionType = (int)missionReward.PossessionType,
                        PossessionId = missionReward.PossessionId,
                        Count = missionReward.Count
                    });
                }
            }
        }

        bool allRegularWillClear = regularMissionCount > 0 && (regularMissionCount - pendingClearCount + pendingClearCount) == regularMissionCount;
        if (allRegularWillClear && bigWinMissionId != 0)
        {
            EntityIUserQuestMission? bigWinUserMission = userDb.EntityIUserQuestMission
                .FirstOrDefault(m => m.QuestId == questId && m.QuestMissionId == bigWinMissionId);
            if (bigWinUserMission == null || !bigWinUserMission.IsClear)
            {
                isBigWin = true;
                EntityMQuestMission? bigWinMission = _masterDb.EntityMQuestMission
                    .FirstOrDefault(m => m.QuestMissionId == bigWinMissionId);
                if (bigWinMission != null)
                {
                    EntityMQuestMissionReward? bigWinReward = _masterDb.EntityMQuestMissionReward
                        .FirstOrDefault(r => r.QuestMissionRewardId == bigWinMission.QuestMissionRewardId);
                    if (bigWinReward != null)
                    {
                        bigWinRewards.Add(new QuestReward
                        {
                            PossessionType = (int)bigWinReward.PossessionType,
                            PossessionId = bigWinReward.PossessionId,
                            Count = bigWinReward.Count
                        });
                    }
                }
            }
        }

        List<QuestReward> dropRewards = BuildDropRewards(quest);

        // === PHASE B: Apply rewards and update quest state (only on victory) ===
        if (!isRetired)
        {
            if (isFirstClear)
            {
                ApplyExpRewards(userDb, quest, userId, questId);
                ApplyGoldReward(userDb, quest, userId, nowMs);
                ApplyFirstClearPossessions(userDb, firstClearRewardRows, userId, _masterDb);
                ApplyWeaponStoryUnlocks(userDb, questId, userId);
            }

            ApplyDropRewardPossessions(userDb, dropRewards, userId, _masterDb);
            ApplyDropRewardPossessions(userDb, replayFlowFirstClearRewards, userId, _masterDb);

            userQuest.QuestStateType = (int)QuestStateType.CLEARED;
            userQuest.ClearCount++;
            userQuest.DailyClearCount++;
            userQuest.LastClearDatetime = nowMs;
        }

        // === PHASE C: Reset flow and progress (always, even on retire) ===
        EntityIUserMainQuestProgressStatus? progressStatus = userDb.EntityIUserMainQuestProgressStatus.FirstOrDefault(s => s.UserId == userId);
        if (progressStatus != null)
        {
            progressStatus.CurrentQuestSceneId = 0;
            progressStatus.HeadQuestSceneId = 0;
            progressStatus.CurrentQuestFlowType = QuestFlowType.UNKNOWN;
        }

        if (currentFlowStatus != null)
        {
            currentFlowStatus.CurrentQuestFlowType = QuestFlowType.UNKNOWN;
        }

        // Restore main flow state after replay
        if (wasReplayFlow)
        {
            EntityIUserMainQuestReplayFlowStatus? replayFlowStatus = userDb.EntityIUserMainQuestReplayFlowStatus.FirstOrDefault(s => s.UserId == userId);
            if (replayFlowStatus != null)
            {
                replayFlowStatus.CurrentQuestSceneId = 0;
                replayFlowStatus.CurrentHeadQuestSceneId = 0;
            }
        }

        // Mark all missions as cleared (always)
        ClearQuestMissions(userDb, questId, userId, nowMs);

        // Build response with all reward categories and clean up the server-side quest session
        FinishMainQuestResponse response = new() { IsBigWin = isBigWin };
        response.DropReward.AddRange(dropRewards);
        response.FirstClearReward.AddRange(firstClearRewards);
        response.MissionClearReward.AddRange(missionClearRewards);
        response.MissionClearCompleteReward.AddRange(bigWinRewards);
        response.ReplayFlowFirstClearReward.AddRange(replayFlowFirstClearRewards);
        if (isBigWin)
        {
            response.BigWinClearedQuestMissionIdList.Add(bigWinMissionId);
        }

        userDb.EntitySQuestSession.RemoveAll(s => s.QuestId == questId);

        return Task.FromResult(response);
    }

    /// <summary>Applies user, character, and costume EXP from quest completion. Rental quests skip character/costume EXP.</summary>
    private void ApplyExpRewards(DarkUserMemoryDatabase userDb, EntityMQuest quest, long userId, int questId)
    {
        // User EXP is always applied regardless of deck.
        EntityIUserStatus? status = userDb.EntityIUserStatus.FirstOrDefault();
        if (status != null)
        {
            status.Exp += quest.UserExp;
            status.Level = CalculateLevel(1, status.Exp);
        }

        // Rental quests use borrowed characters; EXP is not awarded to the player's own roster.
        if (IsRentalQuest(questId))
            return;

        // Character/costume EXP only applies to units in the selected deck.
        EntitySQuestSession? session = userDb.EntitySQuestSession.FirstOrDefault(s => s.QuestId == questId);
        if (session == null || session.UserDeckNumber == 0)
        {
            return;
        }

        EntityIUserDeck? deck = userDb.EntityIUserDeck.FirstOrDefault(d => d.DeckType == DeckType.QUEST && d.UserDeckNumber == session.UserDeckNumber);
        if (deck == null)
        {
            return;
        }

        // Collect costume UUIDs from the 3 character slots in this deck.
        List<string> deckCharUuids = [deck.UserDeckCharacterUuid01, deck.UserDeckCharacterUuid02, deck.UserDeckCharacterUuid03];
        HashSet<string> costumeUuids = [];
        foreach (string deckCharUuid in deckCharUuids)
        {
            if (string.IsNullOrEmpty(deckCharUuid)) { continue; }
            EntityIUserDeckCharacter? deckChar = userDb.EntityIUserDeckCharacter.FirstOrDefault(c => c.UserDeckCharacterUuid == deckCharUuid);
            if (deckChar == null || string.IsNullOrEmpty(deckChar.UserCostumeUuid)) { continue; }
            costumeUuids.Add(deckChar.UserCostumeUuid);
        }

        // Collect character IDs via master costume data.
        HashSet<int> characterIds = [];
        foreach (string costumeUuid in costumeUuids)
        {
            EntityIUserCostume? userCostume = userDb.EntityIUserCostume.FirstOrDefault(c => c.UserCostumeUuid == costumeUuid);
            if (userCostume == null) { continue; }
            EntityMCostume? masterCostume = _masterDb.EntityMCostume.FirstOrDefault(c => c.CostumeId == userCostume.CostumeId);
            if (masterCostume != null)
            {
                characterIds.Add(masterCostume.CharacterId);
            }
        }

        // Apply CharacterExp only to characters in the selected deck.
        foreach (EntityIUserCharacter character in userDb.EntityIUserCharacter.Where(c => characterIds.Contains(c.CharacterId)))
        {
            character.Exp += quest.CharacterExp;
            character.Level = CalculateLevel(31, character.Exp);
        }

        // Apply CostumeExp only to costumes in the selected deck.
        foreach (EntityIUserCostume costume in userDb.EntityIUserCostume.Where(c => costumeUuids.Contains(c.UserCostumeUuid)))
        {
            EntityMCostume? masterCostume = _masterDb.EntityMCostume.FirstOrDefault(c => c.CostumeId == costume.CostumeId);
            if (masterCostume == null) { continue; }
            EntityMCostumeRarity? rarity = _masterDb.EntityMCostumeRarity.FirstOrDefault(r => r.RarityType == masterCostume.RarityType);
            if (rarity == null) { continue; }
            costume.Exp += quest.CostumeExp;
            costume.Level = CalculateLevel(rarity.RequiredExpForLevelUpNumericalParameterMapId, costume.Exp);
        }
    }

    /// <summary>Determines the level for the given EXP using the numerical parameter map thresholds.</summary>
    private int CalculateLevel(int mapId, int currentExp)
    {
        List<EntityMNumericalParameterMap> rows = [.. _masterDb.EntityMNumericalParameterMap
            .Where(r => r.NumericalParameterMapId == mapId)
            .OrderBy(r => r.ParameterKey)];
        int level = 1;
        foreach (EntityMNumericalParameterMap row in rows)
        {
            if (currentExp >= row.ParameterValue)
                level = row.ParameterKey;
            else
                break;
        }
        return level;
    }

    /// <summary>
    /// A quest is rental when any of its scenes uses a battle group that has a
    /// rental deck entry. Rental quests skip character/costume EXP.
    /// </summary>
    private bool IsRentalQuest(int questId)
    {
        IEnumerable<int> sceneIds = _masterDb.EntityMQuestScene
            .Where(s => s.QuestId == questId)
            .Select(s => s.QuestSceneId);

        foreach (int sceneId in sceneIds)
        {
            EntityMQuestSceneBattle? battle = _masterDb.EntityMQuestSceneBattle
                .FirstOrDefault(b => b.QuestSceneId == sceneId);
            if (battle != null && _masterDb.EntityMBattleRentalDeck.Any(r => r.BattleGroupId == battle.BattleGroupId))
                return true;
        }

        return false;
    }

    /// <summary>Clears all quest missions for a given quest, creating any missing records.</summary>
    private void ClearQuestMissions(DarkUserMemoryDatabase userDb, int questId, long userId, long nowMs)
    {
        EntityMQuest? quest = _masterDb.EntityMQuest.FirstOrDefault(q => q.QuestId == questId);
        if (quest == null || quest.QuestMissionGroupId == 0) return;

        foreach (EntityMQuestMissionGroup missionGroupRow in _masterDb.EntityMQuestMissionGroup
            .Where(g => g.QuestMissionGroupId == quest.QuestMissionGroupId))
        {
            EntityIUserQuestMission userMission = userDb.EntityIUserQuestMission.GetOrCreate(
                m => m.QuestId == questId && m.QuestMissionId == missionGroupRow.QuestMissionId,
                () => new EntityIUserQuestMission { UserId = userId, QuestId = questId, QuestMissionId = missionGroupRow.QuestMissionId });
            userMission.IsClear = true;
            userMission.ProgressValue = 1;
            userMission.LatestClearDatetime = nowMs;
        }
    }

    /// <summary>Grants gold (consumable item ID 1) from quest completion.</summary>
    private static void ApplyGoldReward(DarkUserMemoryDatabase userDb, EntityMQuest quest, long userId, long nowMs)
    {
        if (quest.Gold <= 0) return;
        EntityIUserConsumableItem? gold = userDb.EntityIUserConsumableItem.FirstOrDefault(c => c.ConsumableItemId == 1);
        if (gold == null)
        {
            gold = new EntityIUserConsumableItem { UserId = userId, ConsumableItemId = 1, Count = 0, FirstAcquisitionDatetime = nowMs };
            userDb.EntityIUserConsumableItem.Add(gold);
        }
        gold.Count += quest.Gold;
    }

    /// <summary>Grants first-clear reward possessions to the user.</summary>
    private static void ApplyFirstClearPossessions(DarkUserMemoryDatabase userDb, List<EntityMQuestFirstClearRewardGroup> rewardRows, long userId, DarkMasterMemoryDatabase masterDb)
    {
        foreach (EntityMQuestFirstClearRewardGroup row in rewardRows)
            PossessionHelper.Apply(userDb, userId, row.PossessionType, row.PossessionId, row.Count, masterDb);
    }

    /// <summary>Grants scene-based possessions from master data for the given quest scene.</summary>
    private void ApplySceneGrants(DarkUserMemoryDatabase userDb, int questSceneId, long userId)
    {
        List<EntityMUserQuestSceneGrantPossession> grants = [.. _masterDb.EntityMUserQuestSceneGrantPossession
            .Where(g => g.QuestSceneId == questSceneId && !g.IsDebug)];

        foreach (EntityMUserQuestSceneGrantPossession grant in grants)
            PossessionHelper.Apply(userDb, userId, grant.PossessionType, grant.PossessionId, grant.Count, _masterDb);
    }

    /// <summary>Grants all drop reward possessions from a reward list.</summary>
    private static void ApplyDropRewardPossessions(DarkUserMemoryDatabase userDb, List<QuestReward> rewards, long userId, DarkMasterMemoryDatabase masterDb)
    {
        foreach (QuestReward reward in rewards)
            PossessionHelper.Apply(userDb, userId, (PossessionType)reward.PossessionType, reward.PossessionId, reward.Count, masterDb);
    }

    /// <summary>Unlocks weapon stories triggered by QUEST_CLEAR. ACQUISITION unlocks are handled by WeaponHelper.GrantWeapon.</summary>
    private void ApplyWeaponStoryUnlocks(DarkUserMemoryDatabase userDb, int questId, long userId)
    {
        IEnumerable<EntityMWeaponStoryReleaseConditionGroup> questClearCondRows = _masterDb.EntityMWeaponStoryReleaseConditionGroup
            .Where(c => c.WeaponStoryReleaseConditionType == WeaponStoryReleaseConditionType.QUEST_CLEAR
                     && c.ConditionValue == questId);
        foreach (EntityMWeaponStoryReleaseConditionGroup condRow in questClearCondRows)
        {
            EntityMWeapon? masterWeapon = _masterDb.EntityMWeapon
                .FirstOrDefault(w => w.WeaponStoryReleaseConditionGroupId == condRow.WeaponStoryReleaseConditionGroupId);
            if (masterWeapon != null)
                WeaponHelper.GrantWeaponStory(userDb, masterWeapon.WeaponId, condRow.StoryIndex, userId);
        }
    }

    /// <summary>Initializes an extra quest: creates quest/mission records and returns battle drop rewards.</summary>
    public override Task<StartExtraQuestResponse> StartExtraQuest(StartExtraQuestRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        int questId = request.QuestId;

        EntityMQuest? quest = _masterDb.EntityMQuest.FirstOrDefault(q => q.QuestId == questId);

        // Ensure user quest and mission rows exist
        EntityIUserQuest userQuest = userDb.EntityIUserQuest.GetOrCreate(
            q => q.QuestId == questId,
            () => new EntityIUserQuest { UserId = userId, QuestId = questId });

        if (quest != null && quest.QuestMissionGroupId != 0)
        {
            List<int> missionIds = [.. _masterDb.EntityMQuestMissionGroup
                .Where(g => g.QuestMissionGroupId == quest.QuestMissionGroupId)
                .Select(g => g.QuestMissionId)];

            foreach (int missionId in missionIds)
            {
                if (!userDb.EntityIUserQuestMission.Any(m => m.QuestId == questId && m.QuestMissionId == missionId))
                    userDb.EntityIUserQuestMission.Add(new EntityIUserQuestMission { UserId = userId, QuestId = questId, QuestMissionId = missionId });
            }
        }

        EntitySQuestSession session = userDb.EntitySQuestSession.GetOrCreate(
            s => s.QuestId == questId,
            () => new EntitySQuestSession { UserId = userId, QuestId = questId });
        session.UserDeckNumber = request.UserDeckNumber;

        userQuest.QuestStateType = (int)QuestStateType.ACTIVE;
        userQuest.LatestStartDatetime = nowMs;

        EntityIUserExtraQuestProgressStatus progressStatus = userDb.EntityIUserExtraQuestProgressStatus.GetOrCreate(userId);
        progressStatus.CurrentQuestId = questId;

        // Set initial scene IDs from the first scene of this quest
        EntityMQuestScene? firstScene = _masterDb.EntityMQuestScene
            .Where(s => s.QuestId == questId)
            .OrderBy(s => s.SortOrder)
            .FirstOrDefault();
        if (firstScene != null)
        {
            progressStatus.CurrentQuestSceneId = firstScene.QuestSceneId;
            progressStatus.HeadQuestSceneId = firstScene.QuestSceneId;
        }

        StartExtraQuestResponse response = new();
        response.BattleDropReward.AddRange(BuildBattleDropRewards(questId));
        return Task.FromResult(response);
    }

    /// <summary>Restarts an extra quest: resets mission progress and returns battle drop rewards.</summary>
    public override Task<RestartExtraQuestResponse> RestartExtraQuest(RestartExtraQuestRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        int questId = request.QuestId;

        // Reset quest state to ACTIVE
        EntityIUserQuest userQuest = userDb.EntityIUserQuest.GetOrCreate(
            q => q.QuestId == questId,
            () => new EntityIUserQuest { UserId = userId, QuestId = questId });

        userQuest.QuestStateType = (int)QuestStateType.ACTIVE;
        userQuest.IsBattleOnly = false;
        userQuest.LatestStartDatetime = nowMs;

        EntitySQuestSession session = userDb.EntitySQuestSession.GetOrCreate(
            s => s.QuestId == questId,
            () => new EntitySQuestSession { UserId = userId, QuestId = questId });

        // Reset all mission progress for this quest so they can be re-evaluated during battle
        foreach (EntityIUserQuestMission mission in userDb.EntityIUserQuestMission.Where(m => m.QuestId == questId))
        {
            mission.IsClear = false;
            mission.ProgressValue = 0;
            mission.LatestClearDatetime = 0;
        }

        EntityIUserExtraQuestProgressStatus progressStatus = userDb.EntityIUserExtraQuestProgressStatus.GetOrCreate(userId);
        progressStatus.CurrentQuestId = questId;

        RestartExtraQuestResponse response = new();
        response.BattleDropReward.AddRange(BuildBattleDropRewards(questId));
        response.DeckNumber = session.UserDeckNumber;
        return Task.FromResult(response);
    }

    /// <summary>Completes an extra quest: evaluates rewards, applies possessions, updates quest state, and returns results.</summary>
    public override Task<FinishExtraQuestResponse> FinishExtraQuest(FinishExtraQuestRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        if (!_store.TryGet(userId, out DarkUserMemoryDatabase userDb)) return Task.FromResult(new FinishExtraQuestResponse());

        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        int questId = request.QuestId;
        bool isRetired = request.IsRetired;

        EntityMQuest? quest = _masterDb.EntityMQuest.FirstOrDefault(q => q.QuestId == questId);
        if (quest == null) return Task.FromResult(new FinishExtraQuestResponse());

        EntityIUserQuest userQuest = userDb.EntityIUserQuest.GetOrCreate(
            q => q.QuestId == questId,
            () => new EntityIUserQuest { UserId = userId, QuestId = questId });

        List<QuestReward> firstClearRewards = [];
        List<QuestReward> missionClearRewards = [];
        List<QuestReward> bigWinRewards = [];
        bool isBigWin = false;

        bool isFirstClear = userQuest.ClearCount == 0;

        List<EntityMQuestFirstClearRewardGroup> firstClearRewardRows = [];
        if (isFirstClear && quest.QuestFirstClearRewardGroupId != 0)
        {
            int rewardGroupId = quest.QuestFirstClearRewardGroupId;
            EntityMQuestFirstClearRewardSwitch? switchRow = _masterDb.EntityMQuestFirstClearRewardSwitch
                .FirstOrDefault(s => s.QuestId == questId);
            if (switchRow != null)
            {
                EntityIUserQuest? prereqQuest = userDb.EntityIUserQuest
                    .FirstOrDefault(q => q.QuestId == switchRow.SwitchConditionClearQuestId);
                if (prereqQuest != null && prereqQuest.QuestStateType == (int)QuestStateType.CLEARED)
                    rewardGroupId = switchRow.QuestFirstClearRewardGroupId;
            }

            firstClearRewardRows = [.. _masterDb.EntityMQuestFirstClearRewardGroup
                .Where(r => r.QuestFirstClearRewardGroupId == rewardGroupId)];
            firstClearRewards.AddRange(firstClearRewardRows.Select(r => new QuestReward
            {
                PossessionType = (int)r.PossessionType,
                PossessionId = r.PossessionId,
                Count = r.Count
            }));
        }

        List<EntityMQuestMissionGroup> missionGroupRows = quest.QuestMissionGroupId != 0
            ? [.. _masterDb.EntityMQuestMissionGroup.Where(m => m.QuestMissionGroupId == quest.QuestMissionGroupId)]
            : [];

        int regularMissionCount = 0;
        int pendingClearCount = 0;
        int bigWinMissionId = 0;

        foreach (EntityMQuestMissionGroup missionGroupRow in missionGroupRows)
        {
            EntityMQuestMission? mission = _masterDb.EntityMQuestMission
                .FirstOrDefault(m => m.QuestMissionId == missionGroupRow.QuestMissionId);
            if (mission == null) continue;

            if (mission.QuestMissionConditionType == QuestMissionConditionType.COMPLETE)
            {
                bigWinMissionId = mission.QuestMissionId;
                continue;
            }

            regularMissionCount++;

            EntityIUserQuestMission? userMission = userDb.EntityIUserQuestMission
                .FirstOrDefault(m => m.QuestId == questId && m.QuestMissionId == mission.QuestMissionId);
            if (userMission == null || !userMission.IsClear)
            {
                pendingClearCount++;
                EntityMQuestMissionReward? missionReward = _masterDb.EntityMQuestMissionReward
                    .FirstOrDefault(r => r.QuestMissionRewardId == mission.QuestMissionRewardId);
                if (missionReward != null)
                {
                    missionClearRewards.Add(new QuestReward
                    {
                        PossessionType = (int)missionReward.PossessionType,
                        PossessionId = missionReward.PossessionId,
                        Count = missionReward.Count
                    });
                }
            }
        }

        bool allRegularWillClear = regularMissionCount > 0 && (regularMissionCount - pendingClearCount + pendingClearCount) == regularMissionCount;
        if (allRegularWillClear && bigWinMissionId != 0)
        {
            EntityIUserQuestMission? bigWinUserMission = userDb.EntityIUserQuestMission
                .FirstOrDefault(m => m.QuestId == questId && m.QuestMissionId == bigWinMissionId);
            if (bigWinUserMission == null || !bigWinUserMission.IsClear)
            {
                isBigWin = true;
                EntityMQuestMission? bigWinMission = _masterDb.EntityMQuestMission
                    .FirstOrDefault(m => m.QuestMissionId == bigWinMissionId);
                if (bigWinMission != null)
                {
                    EntityMQuestMissionReward? bigWinReward = _masterDb.EntityMQuestMissionReward
                        .FirstOrDefault(r => r.QuestMissionRewardId == bigWinMission.QuestMissionRewardId);
                    if (bigWinReward != null)
                    {
                        bigWinRewards.Add(new QuestReward
                        {
                            PossessionType = (int)bigWinReward.PossessionType,
                            PossessionId = bigWinReward.PossessionId,
                            Count = bigWinReward.Count
                        });
                    }
                }
            }
        }

        List<QuestReward> dropRewardsExtra = BuildDropRewards(quest);

        // Apply rewards and update quest state only on victory (not retired)
        if (!isRetired)
        {
            if (isFirstClear)
            {
                ApplyExpRewards(userDb, quest, userId, questId);
                ApplyGoldReward(userDb, quest, userId, nowMs);
                ApplyFirstClearPossessions(userDb, firstClearRewardRows, userId, _masterDb);
                ApplyWeaponStoryUnlocks(userDb, questId, userId);
            }

            ApplyDropRewardPossessions(userDb, dropRewardsExtra, userId, _masterDb);

            userQuest.QuestStateType = (int)QuestStateType.CLEARED;
            userQuest.ClearCount++;
            userQuest.DailyClearCount++;
            userQuest.LastClearDatetime = nowMs;
        }

        // Reset extra quest progress (always)
        EntityIUserExtraQuestProgressStatus? extraProgress = userDb.EntityIUserExtraQuestProgressStatus
            .FirstOrDefault(s => s.UserId == userId);
        if (extraProgress != null)
        {
            extraProgress.CurrentQuestId = 0;
            extraProgress.CurrentQuestSceneId = 0;
            extraProgress.HeadQuestSceneId = 0;
        }

        // Mark all missions as cleared (always)
        ClearQuestMissions(userDb, questId, userId, nowMs);

        FinishExtraQuestResponse response = new() { IsBigWin = isBigWin };
        response.DropReward.AddRange(dropRewardsExtra);
        response.FirstClearReward.AddRange(firstClearRewards);
        response.MissionClearReward.AddRange(missionClearRewards);
        response.MissionClearCompleteReward.AddRange(bigWinRewards);
        if (isBigWin)
        {
            response.BigWinClearedQuestMissionIdList.Add(bigWinMissionId);
        }

        userDb.EntitySQuestSession.RemoveAll(s => s.QuestId == questId);

        return Task.FromResult(response);
    }

    /// <summary>Initializes an event quest: creates quest/mission records and returns battle drop rewards.</summary>
    public override Task<StartEventQuestResponse> StartEventQuest(StartEventQuestRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        int questId = request.QuestId;
        int chapterId = request.EventQuestChapterId;

        EntityMQuest? quest = _masterDb.EntityMQuest.FirstOrDefault(q => q.QuestId == questId);

        EntityIUserQuest userQuest = userDb.EntityIUserQuest.GetOrCreate(
            q => q.QuestId == questId,
            () => new EntityIUserQuest { UserId = userId, QuestId = questId });

        if (quest != null && quest.QuestMissionGroupId != 0)
        {
            List<int> missionIds = [.. _masterDb.EntityMQuestMissionGroup
                .Where(g => g.QuestMissionGroupId == quest.QuestMissionGroupId)
                .Select(g => g.QuestMissionId)];

            foreach (int missionId in missionIds)
            {
                if (!userDb.EntityIUserQuestMission.Any(m => m.QuestId == questId && m.QuestMissionId == missionId))
                    userDb.EntityIUserQuestMission.Add(new EntityIUserQuestMission { UserId = userId, QuestId = questId, QuestMissionId = missionId });
            }
        }

        userQuest.IsBattleOnly = request.IsBattleOnly;

        EntitySQuestSession eventSession = userDb.EntitySQuestSession.GetOrCreate(
            s => s.QuestId == questId,
            () => new EntitySQuestSession { UserId = userId, QuestId = questId });
        eventSession.UserDeckNumber = request.UserDeckNumber;

        userQuest.QuestStateType = (int)QuestStateType.ACTIVE;
        userQuest.LatestStartDatetime = nowMs;

        EntityIUserEventQuestProgressStatus progressStatus = userDb.EntityIUserEventQuestProgressStatus.GetOrCreate(userId);
        progressStatus.CurrentEventQuestChapterId = chapterId;
        progressStatus.CurrentQuestId = questId;

        // Set initial scene IDs from the first scene of this quest
        EntityMQuestScene? firstScene = _masterDb.EntityMQuestScene
            .Where(s => s.QuestId == questId)
            .OrderBy(s => s.SortOrder)
            .FirstOrDefault();
        if (firstScene != null)
        {
            progressStatus.CurrentQuestSceneId = firstScene.QuestSceneId;
            progressStatus.HeadQuestSceneId = firstScene.QuestSceneId;
        }

        StartEventQuestResponse response = new();
        response.BattleDropReward.AddRange(BuildBattleDropRewards(questId));
        return Task.FromResult(response);
    }

    /// <summary>Restarts an event quest: resets mission progress.</summary>
    public override Task<RestartEventQuestResponse> RestartEventQuest(RestartEventQuestRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        int questId = request.QuestId;

        EntityIUserQuest userQuest = userDb.EntityIUserQuest.GetOrCreate(
            q => q.QuestId == questId,
            () => new EntityIUserQuest { UserId = userId, QuestId = questId });

        userQuest.QuestStateType = (int)QuestStateType.ACTIVE;
        userQuest.IsBattleOnly = false;
        userQuest.LatestStartDatetime = nowMs;

        // Reset all mission progress for this quest so they can be re-evaluated during battle
        foreach (EntityIUserQuestMission mission in userDb.EntityIUserQuestMission.Where(m => m.QuestId == questId))
        {
            mission.IsClear = false;
            mission.ProgressValue = 0;
            mission.LatestClearDatetime = 0;
        }

        EntityIUserEventQuestProgressStatus progressStatus = userDb.EntityIUserEventQuestProgressStatus.GetOrCreate(userId);
        progressStatus.CurrentEventQuestChapterId = request.EventQuestChapterId;
        progressStatus.CurrentQuestId = questId;

        return Task.FromResult(new RestartEventQuestResponse());
    }

    /// <summary>Completes an event quest: evaluates rewards, applies possessions, updates quest state, and returns results.</summary>
    public override Task<FinishEventQuestResponse> FinishEventQuest(FinishEventQuestRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        if (!_store.TryGet(userId, out DarkUserMemoryDatabase userDb)) return Task.FromResult(new FinishEventQuestResponse());

        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        int questId = request.QuestId;
        bool isRetired = request.IsRetired;

        EntityMQuest? quest = _masterDb.EntityMQuest.FirstOrDefault(q => q.QuestId == questId);
        if (quest == null) return Task.FromResult(new FinishEventQuestResponse());

        EntityIUserQuest userQuest = userDb.EntityIUserQuest.GetOrCreate(
            q => q.QuestId == questId,
            () => new EntityIUserQuest { UserId = userId, QuestId = questId });

        List<QuestReward> firstClearRewards = [];
        List<QuestReward> missionClearRewards = [];
        List<QuestReward> bigWinRewards = [];
        bool isBigWin = false;

        bool isFirstClear = userQuest.ClearCount == 0;

        List<EntityMQuestFirstClearRewardGroup> firstClearRewardRows = [];
        if (isFirstClear && quest.QuestFirstClearRewardGroupId != 0)
        {
            int rewardGroupId = quest.QuestFirstClearRewardGroupId;
            EntityMQuestFirstClearRewardSwitch? switchRow = _masterDb.EntityMQuestFirstClearRewardSwitch
                .FirstOrDefault(s => s.QuestId == questId);
            if (switchRow != null)
            {
                EntityIUserQuest? prereqQuest = userDb.EntityIUserQuest
                    .FirstOrDefault(q => q.QuestId == switchRow.SwitchConditionClearQuestId);
                if (prereqQuest != null && prereqQuest.QuestStateType == (int)QuestStateType.CLEARED)
                    rewardGroupId = switchRow.QuestFirstClearRewardGroupId;
            }

            firstClearRewardRows = [.. _masterDb.EntityMQuestFirstClearRewardGroup
                .Where(r => r.QuestFirstClearRewardGroupId == rewardGroupId)];
            firstClearRewards.AddRange(firstClearRewardRows.Select(r => new QuestReward
            {
                PossessionType = (int)r.PossessionType,
                PossessionId = r.PossessionId,
                Count = r.Count
            }));
        }

        List<EntityMQuestMissionGroup> missionGroupRows = quest.QuestMissionGroupId != 0
            ? [.. _masterDb.EntityMQuestMissionGroup.Where(m => m.QuestMissionGroupId == quest.QuestMissionGroupId)]
            : [];

        int regularMissionCount = 0;
        int pendingClearCount = 0;
        int bigWinMissionId = 0;

        foreach (EntityMQuestMissionGroup missionGroupRow in missionGroupRows)
        {
            EntityMQuestMission? mission = _masterDb.EntityMQuestMission
                .FirstOrDefault(m => m.QuestMissionId == missionGroupRow.QuestMissionId);
            if (mission == null) continue;

            if (mission.QuestMissionConditionType == QuestMissionConditionType.COMPLETE)
            {
                bigWinMissionId = mission.QuestMissionId;
                continue;
            }

            regularMissionCount++;

            EntityIUserQuestMission? userMission = userDb.EntityIUserQuestMission
                .FirstOrDefault(m => m.QuestId == questId && m.QuestMissionId == mission.QuestMissionId);
            if (userMission == null || !userMission.IsClear)
            {
                pendingClearCount++;
                EntityMQuestMissionReward? missionReward = _masterDb.EntityMQuestMissionReward
                    .FirstOrDefault(r => r.QuestMissionRewardId == mission.QuestMissionRewardId);
                if (missionReward != null)
                {
                    missionClearRewards.Add(new QuestReward
                    {
                        PossessionType = (int)missionReward.PossessionType,
                        PossessionId = missionReward.PossessionId,
                        Count = missionReward.Count
                    });
                }
            }
        }

        bool allRegularWillClear = regularMissionCount > 0 && (regularMissionCount - pendingClearCount + pendingClearCount) == regularMissionCount;
        if (allRegularWillClear && bigWinMissionId != 0)
        {
            EntityIUserQuestMission? bigWinUserMission = userDb.EntityIUserQuestMission
                .FirstOrDefault(m => m.QuestId == questId && m.QuestMissionId == bigWinMissionId);
            if (bigWinUserMission == null || !bigWinUserMission.IsClear)
            {
                isBigWin = true;
                EntityMQuestMission? bigWinMission = _masterDb.EntityMQuestMission
                    .FirstOrDefault(m => m.QuestMissionId == bigWinMissionId);
                if (bigWinMission != null)
                {
                    EntityMQuestMissionReward? bigWinReward = _masterDb.EntityMQuestMissionReward
                        .FirstOrDefault(r => r.QuestMissionRewardId == bigWinMission.QuestMissionRewardId);
                    if (bigWinReward != null)
                    {
                        bigWinRewards.Add(new QuestReward
                        {
                            PossessionType = (int)bigWinReward.PossessionType,
                            PossessionId = bigWinReward.PossessionId,
                            Count = bigWinReward.Count
                        });
                    }
                }
            }
        }

        List<QuestReward> dropRewardsEvent = BuildDropRewards(quest);

        // Apply rewards and update quest state only on victory (not retired)
        if (!isRetired)
        {
            if (isFirstClear)
            {
                ApplyExpRewards(userDb, quest, userId, questId);
                ApplyGoldReward(userDb, quest, userId, nowMs);
                ApplyFirstClearPossessions(userDb, firstClearRewardRows, userId, _masterDb);
                ApplyWeaponStoryUnlocks(userDb, questId, userId);
            }

            ApplyDropRewardPossessions(userDb, dropRewardsEvent, userId, _masterDb);

            userQuest.QuestStateType = (int)QuestStateType.CLEARED;
            userQuest.ClearCount++;
            userQuest.DailyClearCount++;
            userQuest.LastClearDatetime = nowMs;
        }

        // Reset event quest progress (always)
        EntityIUserEventQuestProgressStatus? progressStatus = userDb.EntityIUserEventQuestProgressStatus
            .FirstOrDefault(s => s.UserId == userId);
        if (progressStatus != null)
        {
            progressStatus.CurrentQuestId = 0;
            progressStatus.CurrentQuestSceneId = 0;
            progressStatus.HeadQuestSceneId = 0;
        }

        // Mark all missions as cleared (always)
        ClearQuestMissions(userDb, questId, userId, nowMs);

        FinishEventQuestResponse response = new() { IsBigWin = isBigWin };
        response.DropReward.AddRange(dropRewardsEvent);
        response.FirstClearReward.AddRange(firstClearRewards);
        response.MissionClearReward.AddRange(missionClearRewards);
        response.MissionClearCompleteReward.AddRange(bigWinRewards);
        if (isBigWin)
        {
            response.BigWinClearedQuestMissionIdList.Add(bigWinMissionId);
        }

        userDb.EntitySQuestSession.RemoveAll(s => s.QuestId == questId);

        return Task.FromResult(response);
    }

    /// <summary>Stub for auto-orbit completion; returns empty response.</summary>
    public override Task<FinishAutoOrbitResponse> FinishAutoOrbit(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new FinishAutoOrbitResponse());
    }

    /// <summary>Sets the active main quest route and updates season route tracking.</summary>
    public override Task<SetRouteResponse> SetRoute(SetRouteRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        int routeId = request.MainQuestRouteId;

        EntityMMainQuestRoute? route = _masterDb.EntityMMainQuestRoute.FirstOrDefault(r => r.MainQuestRouteId == routeId);

        EntityIUserMainQuestMainFlowStatus mainFlowStatus = userDb.EntityIUserMainQuestMainFlowStatus.GetOrCreate(userId);
        mainFlowStatus.CurrentMainQuestRouteId = routeId;

        if (route != null)
        {
            EntityIUserMainQuestSeasonRoute seasonRoute = userDb.EntityIUserMainQuestSeasonRoute.GetOrCreate(userId);
            seasonRoute.MainQuestRouteId = routeId;
            seasonRoute.MainQuestSeasonId = route.MainQuestSeasonId;
        }

        EntityIUserPortalCageStatus portalCageStatus = userDb.EntityIUserPortalCageStatus.GetOrCreate(userId);
        portalCageStatus.IsCurrentProgress = false;

        return Task.FromResult(new SetRouteResponse());
    }

    /// <summary>Stub for scene choice selection; returns empty response.</summary>
    public override Task<SetQuestSceneChoiceResponse> SetQuestSceneChoice(SetQuestSceneChoiceRequest request, ServerCallContext context)
    {
        return Task.FromResult(new SetQuestSceneChoiceResponse());
    }

    /// <summary>Stub for tower accumulation reward; returns empty response.</summary>
    public override Task<ReceiveTowerAccumulationRewardResponse> ReceiveTowerAccumulationReward(ReceiveTowerAccumulationRewardRequest request, ServerCallContext context)
    {
        return Task.FromResult(new ReceiveTowerAccumulationRewardResponse());
    }

    /// <summary>Skips a quest using consumable items: applies EXP/gold, marks cleared, and returns drop rewards.</summary>
    public override Task<SkipQuestResponse> SkipQuest(SkipQuestRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
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

        EntityMQuest? quest = _masterDb.EntityMQuest.FirstOrDefault(q => q.QuestId == questId);
        EntityIUserQuest userQuest = userDb.EntityIUserQuest.GetOrCreate(
            q => q.QuestId == questId,
            () => new EntityIUserQuest { UserId = userId, QuestId = questId });

        // Simulate clearing the quest SkipCount times, awarding EXP/gold on each iteration
        List<QuestReward> dropRewards = [];
        if (quest != null)
        {
            for (int i = 0; i < request.SkipCount; i++)
            {
                List<QuestReward> iterDrops = BuildDropRewards(quest);
                ApplyDropRewardPossessions(userDb, iterDrops, userId, _masterDb);
                dropRewards.AddRange(iterDrops);

                ApplyGoldReward(userDb, quest, userId, nowMs);
                ApplyExpRewards(userDb, quest, userId, questId);
            }

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

        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        EntityIUserEventQuestGuerrillaFreeOpen freeOpen = userDb.EntityIUserEventQuestGuerrillaFreeOpen.GetOrCreate(userId);
        freeOpen.StartDatetime = nowMs;
        freeOpen.OpenMinutes = 60;
        freeOpen.DailyOpenedCount++;

        return Task.FromResult(new StartGuerrillaFreeOpenResponse());
    }

    /// <summary>Resets progress for a limit-content side story quest.</summary>
    public override Task<ResetLimitContentQuestProgressResponse> ResetLimitContentQuestProgress(ResetLimitContentQuestProgressRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        int questId = request.QuestId;

        EntityIUserSideStoryQuest? sideStoryQuest = userDb.EntityIUserSideStoryQuest
            .FirstOrDefault(s => s.SideStoryQuestId == questId);
        if (sideStoryQuest != null)
        {
            sideStoryQuest.HeadSideStoryQuestSceneId = 0;
            sideStoryQuest.SideStoryQuestStateType = 0; // Unknown
        }

        userDb.EntityIUserQuestLimitContentStatus.RemoveAll(s => s.QuestId == questId);

        EntityIUserSideStoryQuestSceneProgressStatus? activeProgress = userDb.EntityIUserSideStoryQuestSceneProgressStatus
            .FirstOrDefault(s => s.UserId == userId);
        if (activeProgress != null && activeProgress.CurrentSideStoryQuestId == questId)
        {
            activeProgress.CurrentSideStoryQuestId = 0;
            activeProgress.CurrentSideStoryQuestSceneId = 0;
        }

        return Task.FromResult(new ResetLimitContentQuestProgressResponse());
    }

    /// <summary>Stub for daily quest group reward; returns empty response.</summary>
    public override Task<ReceiveDailyQuestGroupCompleteRewardResponse> ReceiveDailyQuestGroupCompleteReward(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new ReceiveDailyQuestGroupCompleteRewardResponse());
    }

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
    /// Builds the BattleDropReward list for a given quest by following the master data chain:
    /// QuestScene -> QuestSceneBattle -> BattleGroup -> Battle -> BattleNpcDeck -> BattleNpcDeckCharacterDropCategory.
    /// Returns an empty list if master data is missing. BattleDropEffectId is always 1.
    /// </summary>
    private List<BattleDropReward> BuildBattleDropRewards(int questId)
    {
        HashSet<int> sceneIds = [.. _masterDb.EntityMQuestScene
            .Where(s => s.QuestId == questId)
            .Select(s => s.QuestSceneId)];

        if (sceneIds.Count == 0)
        {
            return [];
        }

        Dictionary<int, int> battleGroupByScene = [];
        foreach (EntityMQuestSceneBattle sb in _masterDb.EntityMQuestSceneBattle)
        {
            if (sceneIds.Contains(sb.QuestSceneId))
            {
                battleGroupByScene[sb.QuestSceneId] = sb.BattleGroupId;
            }
        }

        HashSet<(int SceneId, int CategoryId)> seen = [];
        List<BattleDropReward> drops = [];

        foreach (int sceneId in sceneIds)
        {
            if (!battleGroupByScene.TryGetValue(sceneId, out int groupId))
            {
                continue;
            }

            foreach (EntityMBattleGroup bg in _masterDb.EntityMBattleGroup.Where(bg => bg.BattleGroupId == groupId))
            {
                EntityMBattle? battle = _masterDb.EntityMBattle.FirstOrDefault(b => b.BattleId == bg.BattleId);
                if (battle == null)
                {
                    continue;
                }

                EntityMBattleNpcDeck? deck = _masterDb.EntityMBattleNpcDeck.FirstOrDefault(d =>
                    d.BattleNpcId == battle.BattleNpcId &&
                    d.DeckType == battle.DeckType &&
                    d.BattleNpcDeckNumber == battle.BattleNpcDeckNumber);
                if (deck == null)
                {
                    continue;
                }

                foreach (string uuid in (string[])[deck.BattleNpcDeckCharacterUuid01, deck.BattleNpcDeckCharacterUuid02, deck.BattleNpcDeckCharacterUuid03])
                {
                    if (string.IsNullOrEmpty(uuid))
                    {
                        continue;
                    }

                    EntityMBattleNpcDeckCharacterDropCategory? dropCat = _masterDb.EntityMBattleNpcDeckCharacterDropCategory
                        .FirstOrDefault(dc => dc.BattleNpcId == battle.BattleNpcId && dc.BattleNpcDeckCharacterUuid == uuid);
                    if (dropCat == null)
                    {
                        continue;
                    }

                    if (seen.Add((sceneId, dropCat.BattleDropCategoryId)))
                    {
                        drops.Add(new BattleDropReward
                        {
                            QuestSceneId = sceneId,
                            BattleDropCategoryId = dropCat.BattleDropCategoryId,
                            BattleDropEffectId = 1
                        });
                    }
                }
            }
        }

        return drops;
    }

    /// <summary>
    /// Builds the DropReward list for a given quest from EntityMQuestPickupRewardGroup + EntityMBattleDropReward.
    /// Returns an empty list if the quest has no QuestPickupRewardGroupId or master data is missing.
    /// </summary>
    private List<QuestReward> BuildDropRewards(EntityMQuest quest)
    {
        List<QuestReward> rewards = [];
        if (quest.QuestPickupRewardGroupId == 0)
        {
            return rewards;
        }

        foreach (EntityMQuestPickupRewardGroup pg in _masterDb.EntityMQuestPickupRewardGroup
            .Where(pg => pg.QuestPickupRewardGroupId == quest.QuestPickupRewardGroupId))
        {
            EntityMBattleDropReward? bdr = _masterDb.EntityMBattleDropReward
                .FirstOrDefault(r => r.BattleDropRewardId == pg.BattleDropRewardId);
            if (bdr != null)
            {
                rewards.Add(new QuestReward
                {
                    PossessionType = (int)bdr.PossessionType,
                    PossessionId = bdr.PossessionId,
                    Count = bdr.Count
                });
            }
        }

        return rewards;
    }
}
