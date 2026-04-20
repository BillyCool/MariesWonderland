using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Helpers;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.BigHunt;

namespace MariesWonderland.Services;

public class BigHuntService(UserDataStore store, DarkMasterMemoryDatabase masterDb)
    : MariesWonderland.Proto.BigHunt.BighuntService.BighuntServiceBase
{
    private readonly UserDataStore _store = store;
    private readonly DarkMasterMemoryDatabase _masterDb = masterDb;

    /// <summary>
    /// Begins a BigHunt quest run: deducts stamina for the selected quest, records the player's deck choice,
    /// and initialises per-boss-quest status tracking.
    /// </summary>
    public override Task<StartBigHuntQuestResponse> StartBigHuntQuest(StartBigHuntQuestRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);
        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        EntityMBigHuntQuest? bhQuest = _masterDb.EntityMBigHuntQuest
            .FirstOrDefault(q => q.BigHuntQuestId == request.BigHuntQuestId);

        if (bhQuest is not null)
        {
            HandleBigHuntQuestStart(userDb, bhQuest.QuestId, request.UserDeckNumber, nowMs);
        }

        // Set progress status
        EntityIUserBigHuntProgressStatus progress = GetOrCreateProgress(userDb);
        progress.CurrentBigHuntBossQuestId = request.BigHuntBossQuestId;
        progress.CurrentBigHuntQuestId = request.BigHuntQuestId;
        progress.CurrentQuestSceneId = 0;
        progress.IsDryRun = request.IsDryRun;

        // Store deck number in server-side session
        EntitySBigHuntSession session = GetOrCreateSession(userDb);
        session.DeckNumber = request.UserDeckNumber;

        // Update per-boss-quest status
        EntityIUserBigHuntStatus status = GetOrCreateStatus(userDb, request.BigHuntBossQuestId);
        status.DailyChallengeCount++;
        status.LatestChallengeDatetime = nowMs;

        return Task.FromResult(new StartBigHuntQuestResponse());
    }

    /// <summary>
    /// Advances the player's current quest scene checkpoint during an active hunt run.
    /// </summary>
    public override Task<UpdateBigHuntQuestSceneProgressResponse> UpdateBigHuntQuestSceneProgress(UpdateBigHuntQuestSceneProgressRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        EntityIUserBigHuntProgressStatus progress = GetOrCreateProgress(userDb);
        progress.CurrentQuestSceneId = request.QuestSceneId;

        return Task.FromResult(new UpdateBigHuntQuestSceneProgressResponse());
    }

    /// <summary>
    /// Concludes a hunt run: computes the final score from damage and permil bonuses (difficulty, alive, combo),
    /// updates high scores at boss/schedule/weekly levels, issues newly unlocked threshold rewards, and clears
    /// the in-progress session state. Returns an empty score info for retired or dry-run runs.
    /// </summary>
    public override Task<FinishBigHuntQuestResponse> FinishBigHuntQuest(FinishBigHuntQuestRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);
        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        EntityMBigHuntQuest? bhQuest = _masterDb.EntityMBigHuntQuest
            .FirstOrDefault(q => q.BigHuntQuestId == request.BigHuntQuestId);
        EntityMBigHuntBossQuest? bossQuest = _masterDb.EntityMBigHuntBossQuest
            .FirstOrDefault(q => q.BigHuntBossQuestId == request.BigHuntBossQuestId);
        EntityMBigHuntBoss? boss = bossQuest is not null
            ? _masterDb.EntityMBigHuntBoss.FirstOrDefault(b => b.BigHuntBossId == bossQuest.BigHuntBossId)
            : null;

        if (bhQuest is not null)
        {
            HandleBigHuntQuestFinish(userDb, bhQuest.QuestId, request.IsRetired, nowMs);
        }

        EntityIUserBigHuntProgressStatus progress = GetOrCreateProgress(userDb);
        EntitySBigHuntSession session = GetOrCreateSession(userDb);

        // Retired or dry run — clear progress and return empty score info.
        if (request.IsRetired || progress.IsDryRun)
        {
            ClearProgress(progress);
            return Task.FromResult(new FinishBigHuntQuestResponse
            {
                ScoreInfo = new BigHuntScoreInfo(),
                BattleReport = new BigHuntBattleReport()
            });
        }

        // --- Scoring engine ---
        long totalDamage = session.TotalDamage;
        long baseScore = totalDamage;

        int difficultyBonusPermil = 0;
        if (bhQuest is not null)
        {
            EntityMBigHuntQuestScoreCoefficient? coeff = _masterDb.EntityMBigHuntQuestScoreCoefficient
                .FirstOrDefault(c => c.BigHuntQuestScoreCoefficientId == bhQuest.BigHuntQuestScoreCoefficientId);
            if (coeff is not null)
            {
                difficultyBonusPermil = coeff.ScoreDifficultBonusPermil;
            }
        }

        int aliveBonusPermil = 500;

        int maxComboBonusPermil = session.MaxComboCount switch
        {
            >= 100 => 300,
            >= 50 => 200,
            >= 20 => 100,
            _ => 0
        };

        long userScore = baseScore * (1000 + difficultyBonusPermil + aliveBonusPermil + maxComboBonusPermil) / 1000;

        // --- High-score tracking (per-boss) ---
        bool isHighScore = false;
        long oldMaxScore = 0;
        if (bossQuest is not null)
        {
            EntityIUserBigHuntMaxScore? maxScoreEntry = userDb.EntityIUserBigHuntMaxScore
                .FirstOrDefault(m => m.BigHuntBossId == bossQuest.BigHuntBossId);
            oldMaxScore = maxScoreEntry?.MaxScore ?? 0;

            if (userScore > oldMaxScore)
            {
                isHighScore = true;
                if (maxScoreEntry is null)
                {
                    userDb.EntityIUserBigHuntMaxScore.Add(new EntityIUserBigHuntMaxScore
                    {
                        UserId = userId,
                        BigHuntBossId = bossQuest.BigHuntBossId,
                        MaxScore = userScore,
                        MaxScoreUpdateDatetime = nowMs
                    });
                }
                else
                {
                    maxScoreEntry.MaxScore = userScore;
                    maxScoreEntry.MaxScoreUpdateDatetime = nowMs;
                }
            }
        }

        // --- High-score tracking (per-schedule) ---
        int activeScheduleId = ResolveActiveScheduleId(nowMs);
        if (bossQuest is not null && activeScheduleId > 0)
        {
            EntityIUserBigHuntScheduleMaxScore? schedMax = userDb.EntityIUserBigHuntScheduleMaxScore
                .FirstOrDefault(m => m.UserId == userId
                    && m.BigHuntScheduleId == activeScheduleId
                    && m.BigHuntBossId == bossQuest.BigHuntBossId);

            if (schedMax is null)
            {
                if (userScore > 0)
                {
                    userDb.EntityIUserBigHuntScheduleMaxScore.Add(new EntityIUserBigHuntScheduleMaxScore
                    {
                        UserId = userId,
                        BigHuntScheduleId = activeScheduleId,
                        BigHuntBossId = bossQuest.BigHuntBossId,
                        MaxScore = userScore,
                        MaxScoreUpdateDatetime = nowMs
                    });
                }
            }
            else if (userScore > schedMax.MaxScore)
            {
                schedMax.MaxScore = userScore;
                schedMax.MaxScoreUpdateDatetime = nowMs;
            }
        }

        // --- High-score tracking (per-weekly-attribute) ---
        long weeklyVersion = WeeklyVersion(nowMs);
        if (boss is not null)
        {
            EntityIUserBigHuntWeeklyMaxScore? weeklyMax = userDb.EntityIUserBigHuntWeeklyMaxScore
                .FirstOrDefault(m => m.UserId == userId
                    && m.BigHuntWeeklyVersion == weeklyVersion
                    && m.AttributeType == boss.AttributeType);

            if (weeklyMax is null)
            {
                if (userScore > 0)
                {
                    userDb.EntityIUserBigHuntWeeklyMaxScore.Add(new EntityIUserBigHuntWeeklyMaxScore
                    {
                        UserId = userId,
                        BigHuntWeeklyVersion = weeklyVersion,
                        AttributeType = boss.AttributeType,
                        MaxScore = userScore
                    });
                }
            }
            else if (userScore > weeklyMax.MaxScore)
            {
                weeklyMax.MaxScore = userScore;
            }
        }

        // Grade icon
        int assetGradeIconId = bossQuest is not null
            ? ResolveGradeIconId(bossQuest.BigHuntBossId, userScore)
            : 0;

        BigHuntScoreInfo scoreInfo = new()
        {
            UserScore = userScore,
            IsHighScore = isHighScore,
            TotalDamage = totalDamage,
            BaseScore = baseScore,
            DifficultyBonusPermil = difficultyBonusPermil,
            AliveBonusPermil = aliveBonusPermil,
            MaxComboBonusPermil = maxComboBonusPermil,
            AssetGradeIconId = assetGradeIconId
        };

        // --- Reward collection on high score ---
        List<BigHuntReward> scoreRewards = [];
        if (isHighScore && bossQuest is not null)
        {
            int rewardGroupId = ResolveActiveScoreRewardGroupId(bossQuest.BigHuntScoreRewardGroupScheduleId, nowMs);
            if (rewardGroupId > 0)
            {
                List<(PossessionType Type, int Id, int Count)> newItems = CollectNewRewards(rewardGroupId, oldMaxScore, userScore);
                foreach ((PossessionType type, int id, int count) in newItems)
                {
                    GrantPossessionViaPossessionHelper(userDb, type, id, count);
                    scoreRewards.Add(new BigHuntReward
                    {
                        PossessionType = (int)type,
                        PossessionId = id,
                        Count = count
                    });
                }
            }
        }

        // Clear progress and battle state
        ClearProgress(progress);
        session.BattleBinary = [];
        session.TotalDamage = 0;
        session.MaxComboCount = 0;
        session.BossKnockDownCount = 0;
        session.DeckType = 0;
        session.UserTripleDeckNumber = 0;

        FinishBigHuntQuestResponse response = new()
        {
            ScoreInfo = scoreInfo,
            BattleReport = new BigHuntBattleReport()
        };
        response.ScoreReward.AddRange(scoreRewards);

        return Task.FromResult(response);
    }

    /// <summary>
    /// Resumes a BigHunt quest after a disconnect: restores the saved battle binary and deck choice,
    /// and increments the daily challenge count for the boss quest.
    /// </summary>
    public override Task<RestartBigHuntQuestResponse> RestartBigHuntQuest(RestartBigHuntQuestRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);
        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        EntityMBigHuntQuest? bhQuest = _masterDb.EntityMBigHuntQuest
            .FirstOrDefault(q => q.BigHuntQuestId == request.BigHuntQuestId);

        EntitySBigHuntSession session = GetOrCreateSession(userDb);

        if (bhQuest is not null)
        {
            HandleBigHuntQuestStart(userDb, bhQuest.QuestId, session.DeckNumber, nowMs);
        }

        // Reset scene progress
        EntityIUserBigHuntProgressStatus progress = GetOrCreateProgress(userDb);
        progress.CurrentQuestSceneId = 0;

        // Increment daily challenge count
        EntityIUserBigHuntStatus status = GetOrCreateStatus(userDb, request.BigHuntBossQuestId);
        status.DailyChallengeCount++;
        status.LatestChallengeDatetime = nowMs;

        RestartBigHuntQuestResponse response= new()
        {
            BattleBinary = Google.Protobuf.ByteString.CopyFrom(session.BattleBinary),
            DeckNumber = session.DeckNumber
        };

        return Task.FromResult(response);
    }

    /// <summary>
    /// Applies a bulk skip of one or more hunt attempts, incrementing the daily challenge counter
    /// without entering combat.
    /// </summary>
    public override Task<SkipBigHuntQuestResponse> SkipBigHuntQuest(SkipBigHuntQuestRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);
        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        EntityIUserBigHuntStatus status = GetOrCreateStatus(userDb, request.BigHuntBossQuestId);
        status.DailyChallengeCount += request.SkipCount;
        status.LatestChallengeDatetime = nowMs;

        return Task.FromResult(new SkipBigHuntQuestResponse());
    }

    /// <summary>
    /// Persists mid-battle state to the server session: the raw battle binary snapshot plus
    /// accumulated damage, combo, and boss knockdown statistics for restart recovery and scoring.
    /// </summary>
    /// <summary>
    /// Persists the battle binary and detail (damage, combo, knock-downs) from the client into the session.
    /// </summary>
    public override Task<SaveBigHuntBattleInfoResponse> SaveBigHuntBattleInfo(SaveBigHuntBattleInfoRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        EntitySBigHuntSession session = GetOrCreateSession(userDb);
        session.BattleBinary= request.BattleBinary.ToByteArray();

        if (request.BigHuntBattleDetail is not null)
        {
            // Sum damage across all costumes in the party
            long totalDamage = 0;
            foreach (Proto.Battle.CostumeBattleInfo ci in request.BigHuntBattleDetail.CostumeBattleInfo)
            {
                if (ci is not null)
                {
                    totalDamage += ci.TotalDamage;
                }
            }

            // Persist battle statistics used for scoring and restart
            session.DeckType = request.BigHuntBattleDetail.DeckType;
            session.UserTripleDeckNumber = request.BigHuntBattleDetail.UserTripleDeckNumber;
            session.BossKnockDownCount = request.BigHuntBattleDetail.BossKnockDownCount;
            session.MaxComboCount = request.BigHuntBattleDetail.MaxComboCount;
            session.TotalDamage = totalDamage;
        }

        return Task.FromResult(new SaveBigHuntBattleInfoResponse());
    }

    /// <summary>
    /// Returns the player's BigHunt overview: weekly score results and grade icons per boss attribute,
    /// the current week's uncollected rewards, and last week's earned rewards.
    /// </summary>
    /// <summary>
    /// Returns the top-level summary view: weekly score results, weekly rewards, and last week's rewards for all bosses.
    /// </summary>
    public override Task<GetBigHuntTopDataResponse> GetBigHuntTopData(Empty request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);
        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        long weeklyVersion = WeeklyVersion(nowMs);

        // Build weekly score results for each boss
        List<WeeklyScoreResult> weeklyScoreResults = [];
        foreach (EntityMBigHuntBoss boss in _masterDb.EntityMBigHuntBoss)
        {
            EntityIUserBigHuntWeeklyMaxScore? ws = userDb.EntityIUserBigHuntWeeklyMaxScore
                .FirstOrDefault(m => m.UserId == userId
                    && m.BigHuntWeeklyVersion == weeklyVersion
                    && m.AttributeType == boss.AttributeType);

            long maxScore = ws?.MaxScore ?? 0;
            int gradeIconId = ResolveGradeIconId(boss.BigHuntBossId, maxScore);

            weeklyScoreResults.Add(new WeeklyScoreResult
            {
                AttributeType = (int)boss.AttributeType,
                BeforeMaxScore = maxScore,
                CurrentMaxScore = maxScore,
                BeforeAssetGradeIconId = gradeIconId,
                CurrentAssetGradeIconId = gradeIconId,
                AfterMaxScore = maxScore,
                AfterAssetGradeIconId = gradeIconId
            });
        }

        // Check if weekly reward was already received
        EntityIUserBigHuntWeeklyStatus? weeklyStatus = userDb.EntityIUserBigHuntWeeklyStatus
            .FirstOrDefault(s => s.BigHuntWeeklyVersion == weeklyVersion);

        // Resolve current week rewards
        List<BigHuntReward> weeklyRewards = ResolveWeeklyRewards(userDb, weeklyVersion, nowMs);

        // Resolve last week rewards
        long lastWeekVersion = weeklyVersion - (7L * 24 * 60 * 60 * 1000);
        List<BigHuntReward> lastWeekRewards = ResolveWeeklyRewards(userDb, lastWeekVersion, nowMs);

        GetBigHuntTopDataResponse response = new()
        {
            IsReceivedWeeklyScoreReward = weeklyStatus?.IsReceivedWeeklyReward ?? false
        };
        response.WeeklyScoreResult.AddRange(weeklyScoreResults);
        response.WeeklyScoreReward.AddRange(weeklyRewards);
        response.LastWeekWeeklyScoreReward.AddRange(lastWeekRewards);

        return Task.FromResult(response);
    }

    // ────────── Quest state helpers ──────────

    /// <summary>
    /// Initializes quest and mission state for the underlying quest, and transitions its state to active.
    /// </summary>
    private void HandleBigHuntQuestStart(DarkUserMemoryDatabase userDb, int questId, int deckNumber, long nowMs)
    {
        EntityMQuest? masterQuest = _masterDb.EntityMQuest.FirstOrDefault(q => q.QuestId == questId);

        EntityIUserQuest userQuest = userDb.EntityIUserQuest
            .FirstOrDefault(q => q.QuestId == questId)
            ?? AddEntity(userDb.EntityIUserQuest, new EntityIUserQuest { UserId = userDb.UserId, QuestId = questId });

        // Initialize quest missions
        if (masterQuest is not null && masterQuest.QuestMissionGroupId != 0)
        {
            foreach (EntityMQuestMissionGroup missionGroupRow in _masterDb.EntityMQuestMissionGroup
                .Where(g => g.QuestMissionGroupId == masterQuest.QuestMissionGroupId))
            {
                if (!userDb.EntityIUserQuestMission.Any(m => m.QuestId == questId && m.QuestMissionId == missionGroupRow.QuestMissionId))
                    userDb.EntityIUserQuestMission.Add(new EntityIUserQuestMission { UserId = userDb.UserId, QuestId = questId, QuestMissionId = missionGroupRow.QuestMissionId });
            }
        }

        userQuest.QuestStateType = (int)QuestStateType.ACTIVE;
        userQuest.LatestStartDatetime = nowMs;
    }

    /// <summary>
    /// Marks the quest cleared, applies first-clear and drop rewards on success,
    /// and clears quest missions.
    /// </summary>
    private void HandleBigHuntQuestFinish(DarkUserMemoryDatabase userDb, int questId, bool isRetired, long nowMs)
    {
        EntityMQuest? masterQuest = _masterDb.EntityMQuest.FirstOrDefault(q => q.QuestId == questId);
        EntityIUserQuest? userQuest = userDb.EntityIUserQuest
            .FirstOrDefault(q => q.QuestId == questId);

        if (userQuest is not null && !isRetired)
        {
            // First-clear rewards
            if (userQuest.ClearCount == 0 && masterQuest is not null && masterQuest.QuestFirstClearRewardGroupId != 0)
            {
                int rewardGroupId = masterQuest.QuestFirstClearRewardGroupId;
                EntityMQuestFirstClearRewardSwitch? switchRow = _masterDb.EntityMQuestFirstClearRewardSwitch
                    .FirstOrDefault(s => s.QuestId == questId);
                if (switchRow != null)
                {
                    EntityIUserQuest? prereq = userDb.EntityIUserQuest
                        .FirstOrDefault(q => q.QuestId == switchRow.SwitchConditionClearQuestId);
                    if (prereq != null && prereq.QuestStateType == (int)QuestStateType.CLEARED)
                        rewardGroupId = switchRow.QuestFirstClearRewardGroupId;
                }

                foreach (EntityMQuestFirstClearRewardGroup reward in _masterDb.EntityMQuestFirstClearRewardGroup
                    .Where(r => r.QuestFirstClearRewardGroupId == rewardGroupId))
                {
                    PossessionHelper.Apply(userDb, reward.PossessionType, reward.PossessionId, reward.Count, _masterDb);
                }
            }

            // Drop rewards
            if (masterQuest is not null && masterQuest.QuestPickupRewardGroupId != 0)
            {
                foreach (EntityMQuestPickupRewardGroup pickup in _masterDb.EntityMQuestPickupRewardGroup
                    .Where(p => p.QuestPickupRewardGroupId == masterQuest.QuestPickupRewardGroupId))
                {
                    EntityMBattleDropReward? drop = _masterDb.EntityMBattleDropReward
                        .FirstOrDefault(d => d.BattleDropRewardId == pickup.BattleDropRewardId);
                    if (drop != null)
                        PossessionHelper.Apply(userDb, drop.PossessionType, drop.PossessionId, drop.Count, _masterDb);
                }
            }

            userQuest.QuestStateType = (int)QuestStateType.CLEARED;
            userQuest.ClearCount++;
            userQuest.DailyClearCount++;
            userQuest.LastClearDatetime = nowMs;
        }

        // Clear quest missions
        if (masterQuest is not null && masterQuest.QuestMissionGroupId != 0)
        {
            foreach (EntityMQuestMissionGroup missionGroupRow in _masterDb.EntityMQuestMissionGroup
                .Where(g => g.QuestMissionGroupId == masterQuest.QuestMissionGroupId))
            {
                EntityIUserQuestMission? userMission = userDb.EntityIUserQuestMission
                    .FirstOrDefault(m => m.QuestId == questId && m.QuestMissionId == missionGroupRow.QuestMissionId);
                if (userMission is null)
                {
                    userMission = new EntityIUserQuestMission { UserId = userDb.UserId, QuestId = questId, QuestMissionId = missionGroupRow.QuestMissionId };
                    userDb.EntityIUserQuestMission.Add(userMission);
                }
                userMission.IsClear = true;
                userMission.ProgressValue = 1;
                userMission.LatestClearDatetime = nowMs;
            }
        }
    }

    // ────────── Catalog resolution helpers ──────────

    /// <summary>
    /// Returns the ID of the schedule whose challenge window contains <paramref name="nowMs"/>,
    /// falling back to the schedule with the most recently ended window.
    /// </summary>
    private int ResolveActiveScheduleId(long nowMs)
    {
        EntityMBigHuntSchedule? active = _masterDb.EntityMBigHuntSchedule
            .FirstOrDefault(s => nowMs >= s.ChallengeStartDatetime && nowMs <= s.ChallengeEndDatetime);

        if (active is not null)
        {
            return active.BigHuntScheduleId;
        }

        // Fall back to schedule with latest end datetime.
        return _masterDb.EntityMBigHuntSchedule
            .OrderByDescending(s => s.ChallengeEndDatetime)
            .Select(s => s.BigHuntScheduleId)
            .FirstOrDefault();
    }

    /// <summary>
    /// Returns the highest grade icon ID whose score threshold the player's score meets for the given boss,
    /// or 0 if no threshold is reached.
    /// </summary>
    private int ResolveGradeIconId(int bossId, long score)
    {
        EntityMBigHuntBoss? boss = _masterDb.EntityMBigHuntBoss
            .FirstOrDefault(b => b.BigHuntBossId == bossId);
        if (boss is null)
        {
            return 0;
        }

        // Thresholds sorted ascending by NecessaryScore; last one where score >= threshold wins.
        int iconId = 0;
        foreach (EntityMBigHuntBossGradeGroup g in _masterDb.EntityMBigHuntBossGradeGroup
            .Where(g => g.BigHuntBossGradeGroupId == boss.BigHuntBossGradeGroupId)
            .OrderBy(g => g.NecessaryScore))
        {
            if (score >= g.NecessaryScore)
            {
                iconId = g.AssetGradeIconId;
            }
            else
            {
                break;
            }
        }

        return iconId;
    }

    /// <summary>
    /// Returns the score reward group ID in effect for a given boss-quest score reward group schedule at <paramref name="nowMs"/>.
    /// </summary>
    private int ResolveActiveScoreRewardGroupId(int scheduleId, long nowMs)
    {
        // Entries sorted descending by start datetime; first one where nowMs >= start wins.
        List<EntityMBigHuntScoreRewardGroupSchedule> entries = [.. _masterDb.EntityMBigHuntScoreRewardGroupSchedule
            .Where(e => e.BigHuntScoreRewardGroupScheduleId == scheduleId)
            .OrderByDescending(e => e.StartDatetime)];

        foreach (EntityMBigHuntScoreRewardGroupSchedule entry in entries)
        {
            if (nowMs >= entry.StartDatetime)
            {
                return entry.BigHuntScoreRewardGroupId;
            }
        }

        return entries.Count > 0 ? entries[^1].BigHuntScoreRewardGroupId : 0;
    }

    /// <summary>
    /// Returns the weekly score reward group ID in effect for a given schedule and boss attribute type at <paramref name="nowMs"/>.
    /// </summary>
    private int ResolveActiveWeeklyRewardGroupId(int scheduleId, AttributeType attributeType, long nowMs)
    {
        List<EntityMBigHuntWeeklyAttributeScoreRewardGroupSchedule> entries =
            [.. _masterDb.EntityMBigHuntWeeklyAttributeScoreRewardGroupSchedule
                .Where(e => e.BigHuntWeeklyAttributeScoreRewardGroupScheduleId == scheduleId
                    && e.AttributeType == attributeType)
                .OrderByDescending(e => e.StartDatetime)];

        foreach (EntityMBigHuntWeeklyAttributeScoreRewardGroupSchedule entry in entries)
        {
            if (nowMs >= entry.StartDatetime)
            {
                return entry.BigHuntScoreRewardGroupId;
            }
        }

        return entries.Count > 0 ? entries[^1].BigHuntScoreRewardGroupId : 0;
    }

    /// <summary>
    /// Returns all reward items whose score thresholds fall within the range (<paramref name="oldMax"/>, <paramref name="newMax"/>],
    /// i.e. thresholds newly crossed by the player's improved score.
    /// </summary>
    private List<(PossessionType Type, int Id, int Count)> CollectNewRewards(int scoreRewardGroupId, long oldMax, long newMax)
    {
        List<(PossessionType, int, int)> items = [];

        foreach (EntityMBigHuntScoreRewardGroup threshold in _masterDb.EntityMBigHuntScoreRewardGroup
            .Where(t => t.BigHuntScoreRewardGroupId == scoreRewardGroupId)
            .OrderBy(t => t.NecessaryScore))
        {
            if (threshold.NecessaryScore > oldMax && threshold.NecessaryScore <= newMax)
            {
                foreach (EntityMBigHuntRewardGroup reward in _masterDb.EntityMBigHuntRewardGroup
                    .Where(r => r.BigHuntRewardGroupId == threshold.BigHuntRewardGroupId))
                {
                    items.Add((reward.PossessionType, reward.PossessionId, reward.Count));
                }
            }
        }

        return items;
    }

    /// <summary>
    /// Builds the list of weekly reward items earned across all boss attributes for a given week version,
    /// based on the player's weekly max scores.
    /// </summary>
    private List<BigHuntReward> ResolveWeeklyRewards(DarkUserMemoryDatabase userDb, long weeklyVersion, long nowMs)
    {
        List<BigHuntReward> rewards = [];

        foreach (EntityMBigHuntBoss boss in _masterDb.EntityMBigHuntBoss)
        {
            int rewardGroupId = ResolveActiveWeeklyRewardGroupId(1, boss.AttributeType, nowMs);
            if (rewardGroupId == 0)
            {
                continue;
            }

            EntityIUserBigHuntWeeklyMaxScore? ws = userDb.EntityIUserBigHuntWeeklyMaxScore
                .FirstOrDefault(m => m.UserId == userDb.UserId
                    && m.BigHuntWeeklyVersion == weeklyVersion
                    && m.AttributeType == boss.AttributeType);

            long maxScore = ws?.MaxScore ?? 0;

            foreach ((PossessionType type, int id, int count) in CollectNewRewards(rewardGroupId, 0, maxScore))
            {
                rewards.Add(new BigHuntReward
                {
                    PossessionType = (int)type,
                    PossessionId = id,
                    Count = count
                });
            }
        }

        return rewards;
    }

    // ────────── State accessors ──────────

    /// <summary>
    /// Gets or initialises the player's BigHunt in-progress quest status record.
    /// </summary>
    private static EntityIUserBigHuntProgressStatus GetOrCreateProgress(DarkUserMemoryDatabase userDb)
    {
        return userDb.EntityIUserBigHuntProgressStatus
            .FirstOrDefault(p => p.UserId == userDb.UserId)
            ?? AddEntity(userDb.EntityIUserBigHuntProgressStatus, new EntityIUserBigHuntProgressStatus { UserId = userDb.UserId });
    }

    /// <summary>
    /// Gets or initialises the player's per-boss-quest challenge status record.
    /// </summary>
    private static EntityIUserBigHuntStatus GetOrCreateStatus(DarkUserMemoryDatabase userDb, int bossQuestId)
    {
        return userDb.EntityIUserBigHuntStatus
            .FirstOrDefault(s => s.BigHuntBossQuestId == bossQuestId)
            ?? AddEntity(userDb.EntityIUserBigHuntStatus, new EntityIUserBigHuntStatus
            {
                UserId = userDb.UserId,
                BigHuntBossQuestId = bossQuestId
            });
    }

    /// <summary>
    /// Gets or initialises the player's server-side battle session record.
    /// </summary>
    private static EntitySBigHuntSession GetOrCreateSession(DarkUserMemoryDatabase userDb)
    {
        return userDb.EntitySBigHuntSession
            .FirstOrDefault(s => s.UserId == userDb.UserId)
            ?? AddEntity(userDb.EntitySBigHuntSession, new EntitySBigHuntSession { UserId = userDb.UserId });
    }

    /// <summary>
    /// Resets all fields on a BigHunt progress status record to their default (no active hunt) values.
    /// </summary>
    private static void ClearProgress(EntityIUserBigHuntProgressStatus progress)
    {
        progress.CurrentBigHuntBossQuestId = 0;
        progress.CurrentBigHuntQuestId = 0;
        progress.CurrentQuestSceneId = 0;
        progress.IsDryRun = false;
    }

    // ────────── Possession grant ──────────

    /// <summary>
    /// Routes possession grants through PossessionHelper.Apply for consistent handling.
    /// </summary>
    private void GrantPossessionViaPossessionHelper(DarkUserMemoryDatabase userDb, PossessionType possessionType, int possessionId, int count)
    {
        PossessionHelper.Apply(userDb, possessionType, possessionId, count, _masterDb);
    }

    // ────────── Time helpers ──────────

    /// <summary>
    /// Returns Monday 00:00 UTC of the week containing the given timestamp, as millis.
    /// </summary>
    private static long WeeklyVersion(long millis)
    {
        DateTimeOffset dto = DateTimeOffset.FromUnixTimeMilliseconds(millis);
        DateTime utc = dto.UtcDateTime;
        int weekday = (int)utc.DayOfWeek;
        if (weekday == 0) { weekday = 7; } // Sunday = 7
        DateTime monday = utc.Date.AddDays(-(weekday - 1));
        return new DateTimeOffset(monday, TimeSpan.Zero).ToUnixTimeMilliseconds();
    }

    /// <summary>
    /// Appends an entity to the given list and returns it, enabling inline initialisation.
    /// </summary>
    private static T AddEntity<T>(List<T> list, T entity)
    {
        list.Add(entity);
        return entity;
    }
}
