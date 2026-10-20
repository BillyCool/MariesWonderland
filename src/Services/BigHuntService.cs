using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Helpers;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Battle;
using MariesWonderland.Proto.BigHunt;
using MariesWonderland.Proto.Quest;
using MariesWonderland.Quests;

namespace MariesWonderland.Services;

public class BigHuntService(UserDataStore store, DarkMasterMemoryDatabase masterDb, QuestEngine questEngine, QuestCampaignHelper questCampaignHelper, TimeProvider timeProvider)
    : MariesWonderland.Proto.BigHunt.BighuntService.BighuntServiceBase
{
    private readonly UserDataStore _store = store;
    private readonly DarkMasterMemoryDatabase _masterDb = masterDb;
    private readonly QuestEngine _questEngine = questEngine;
    private readonly QuestCampaignHelper _questCampaignHelper = questCampaignHelper;
    private readonly TimeProvider _timeProvider = timeProvider;

    /// <summary>
    /// Begins a BigHunt quest run: deducts stamina for the selected quest, records the player's deck choice,
    /// and initialises per-boss-quest status tracking.
    /// </summary>
    public override Task<StartBigHuntQuestResponse> StartBigHuntQuest(StartBigHuntQuestRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);
        long nowMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();

        EntityMBigHuntQuest? bhQuest = _masterDb.EntityMBigHuntQuest
            .FirstOrDefault(q => q.BigHuntQuestId == request.BigHuntQuestId);

        if (bhQuest is not null)
        {
            _questEngine.ConsumeStamina(userDb, _questEngine.FindQuest(bhQuest.QuestId), _questCampaignHelper.GetBigHuntQuestTarget(bhQuest.QuestId), 1, nowMs);
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
        CountChallenges(GetOrCreateStatus(userDb, request.BigHuntBossQuestId), 1, nowMs);

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
        long nowMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();

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
            ClearBattleSession(session);
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
        long weeklyVersion = GameDayHelper.StartOfWeek(nowMs);
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

        // --- Score reward: every run grants the rewards of the highest tier its score reached ---
        List<BigHuntReward> scoreRewards = bossQuest is not null
            ? GrantHighestTierRewards(userDb, bossQuest, userScore, 1, nowMs)
            : [];

        // Clear progress and battle state
        ClearProgress(progress);
        BigHuntBattleReport battleReport = BuildBattleReport(session);
        ClearBattleSession(session);

        FinishBigHuntQuestResponse response = new()
        {
            ScoreInfo = scoreInfo,
            BattleReport = battleReport
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
        long nowMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();

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
        CountChallenges(GetOrCreateStatus(userDb, request.BigHuntBossQuestId), 1, nowMs);

        RestartBigHuntQuestResponse response= new()
        {
            BattleBinary = Google.Protobuf.ByteString.CopyFrom(session.BattleBinary),
            DeckNumber = session.DeckNumber
        };

        return Task.FromResult(response);
    }

    /// <summary>
    /// Skips one or more hunt attempts without combat: uses up daily attempts and, per skip, grants the rewards of
    /// the highest tier reached by the best score against this boss in the current schedule. Nothing is granted
    /// before the boss has been fought in this schedule.
    /// </summary>
    public override Task<SkipBigHuntQuestResponse> SkipBigHuntQuest(SkipBigHuntQuestRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);
        long nowMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();

        SkipBigHuntQuestResponse response = new();
        if (request.SkipCount <= 0)
        {
            return Task.FromResult(response);
        }

        CountChallenges(GetOrCreateStatus(userDb, request.BigHuntBossQuestId), request.SkipCount, nowMs);

        EntityMBigHuntBossQuest? bossQuest = _masterDb.EntityMBigHuntBossQuest
            .FirstOrDefault(q => q.BigHuntBossQuestId == request.BigHuntBossQuestId);
        if (bossQuest is null)
        {
            return Task.FromResult(response);
        }

        int activeScheduleId = ResolveActiveScheduleId(nowMs);
        long bestScore = userDb.EntityIUserBigHuntScheduleMaxScore
            .FirstOrDefault(m => m.BigHuntScheduleId == activeScheduleId && m.BigHuntBossId == bossQuest.BigHuntBossId)
            ?.MaxScore ?? 0;
        if (bestScore > 0)
        {
            response.ScoreReward.AddRange(GrantHighestTierRewards(userDb, bossQuest, bestScore, request.SkipCount, nowMs));
        }

        return Task.FromResult(response);
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

            int waveIndex = NextWaveIndex(session);
            foreach (CostumeBattleInfo ci in request.BigHuntBattleDetail.CostumeBattleInfo)
            {
                if (ci is null)
                {
                    continue;
                }

                session.BattleReportCostume.Add(new EntitySBigHuntBattleReportCostume
                {
                    WaveIndex = waveIndex,
                    CostumeId = ResolveBigHuntCostumeId(userDb, ci.UserDeckNumber, ci.DeckCharacterNumber, session.DeckNumber),
                    TotalDamage = ci.TotalDamage,
                    HitCount = ci.HitCount,
                    RandomDisplayValueType = ci.BattleReportRandomDisplay?.RandomDisplayValueType ?? 0,
                    RandomDisplayValue = ci.BattleReportRandomDisplay?.RandomDisplayValue ?? 0
                });
            }
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
        long nowMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();
        long weeklyVersion = GameDayHelper.StartOfWeek(nowMs);

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
        EntityIUserQuest userQuest = _questEngine.EnsureQuestRows(userDb, questId);
        QuestEngine.MarkActive(userQuest, nowMs);
    }

    /// <summary>
    /// Marks the quest cleared, applies first-clear and drop rewards on success,
    /// and clears quest missions.
    /// </summary>
    private void HandleBigHuntQuestFinish(DarkUserMemoryDatabase userDb, int questId, bool isRetired, long nowMs)
    {
        EntityMQuest? masterQuest = _questEngine.FindQuest(questId);
        EntityIUserQuest? userQuest = userDb.EntityIUserQuest.FirstOrDefault(q => q.QuestId == questId);

        if (userQuest is not null && !isRetired)
        {
            if (masterQuest is not null)
            {
                if (userQuest.ClearCount == 0)
                {
                    foreach (EntityMQuestFirstClearRewardGroup reward in _questEngine.ResolveFirstClearRewardRows(userDb, masterQuest))
                    {
                        PossessionHelper.Apply(userDb, reward.PossessionType, reward.PossessionId, reward.Count, _masterDb, nowMs);
                    }
                }

                foreach (QuestReward dropReward in _questEngine.BuildDropRewards(masterQuest, _questCampaignHelper.GetBigHuntQuestTarget(questId), nowMs))
                {
                    PossessionHelper.Apply(userDb, (PossessionType)dropReward.PossessionType, dropReward.PossessionId, dropReward.Count, _masterDb, nowMs);
                }
            }

            QuestEngine.MarkCleared(userQuest, 1, nowMs);
        }

        _questEngine.ClearQuestMissions(userDb, questId, nowMs);
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
    /// Grants the rewards of the highest score tier <paramref name="score"/> reaches, <paramref name="times"/> times,
    /// and returns them as granted (one entry per item per time).
    /// </summary>
    private List<BigHuntReward> GrantHighestTierRewards(DarkUserMemoryDatabase userDb, EntityMBigHuntBossQuest bossQuest, long score, int times, long nowMs)
    {
        List<BigHuntReward> granted = [];
        int rewardGroupId = ResolveActiveScoreRewardGroupId(bossQuest.BigHuntScoreRewardGroupScheduleId, nowMs);
        EntityMBigHuntScoreRewardGroup? tier = _masterDb.EntityMBigHuntScoreRewardGroup
            .Where(t => t.BigHuntScoreRewardGroupId == rewardGroupId && t.NecessaryScore <= score)
            .OrderByDescending(t => t.NecessaryScore)
            .FirstOrDefault();
        if (tier is null)
        {
            return granted;
        }

        List<EntityMBigHuntRewardGroup> items = [.. _masterDb.EntityMBigHuntRewardGroup
            .Where(r => r.BigHuntRewardGroupId == tier.BigHuntRewardGroupId)
            .OrderBy(r => r.SortOrder)];
        for (int i = 0; i < times; i++)
        {
            foreach (EntityMBigHuntRewardGroup item in items)
            {
                GrantPossessionViaPossessionHelper(userDb, item.PossessionType, item.PossessionId, item.Count, nowMs);
                granted.Add(new BigHuntReward
                {
                    PossessionType = (int)item.PossessionType,
                    PossessionId = item.PossessionId,
                    Count = item.Count
                });
            }
        }

        return granted;
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
    /// Adds <paramref name="count"/> attempts to today's challenge count, starting from zero on a new game day.
    /// </summary>
    private static void CountChallenges(EntityIUserBigHuntStatus status, int count, long nowMs)
    {
        if (GameDayHelper.IsBeforeToday(status.LatestChallengeDatetime, nowMs))
        {
            status.DailyChallengeCount = 0;
        }

        status.DailyChallengeCount += count;
        status.LatestChallengeDatetime = nowMs;
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

    private static int NextWaveIndex(EntitySBigHuntSession session)
        => session.BattleReportCostume.Count == 0 ? 0 : session.BattleReportCostume.Max(c => c.WaveIndex) + 1;

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

    private static void ClearBattleSession(EntitySBigHuntSession session)
    {
        session.BattleBinary = [];
        session.TotalDamage = 0;
        session.MaxComboCount = 0;
        session.BossKnockDownCount = 0;
        session.DeckType = 0;
        session.UserTripleDeckNumber = 0;
        session.BattleReportCostume.Clear();
    }

    private static BigHuntBattleReport BuildBattleReport(EntitySBigHuntSession session)
    {
        BigHuntBattleReport report = new();

        foreach (IGrouping<int, EntitySBigHuntBattleReportCostume> wave in session.BattleReportCostume
            .OrderBy(c => c.WaveIndex)
            .GroupBy(c => c.WaveIndex))
        {
            BigHuntBattleReportWave reportWave = new();
            foreach (EntitySBigHuntBattleReportCostume costume in wave)
            {
                reportWave.BattleReportCostume.Add(new BigHuntBattleReportCostume
                {
                    CostumeId = costume.CostumeId,
                    TotalDamage = costume.TotalDamage,
                    HitCount = costume.HitCount,
                    BattleReportRandomDisplay = new BattleReportRandomDisplay
                    {
                        RandomDisplayValueType = costume.RandomDisplayValueType,
                        RandomDisplayValue = costume.RandomDisplayValue
                    }
                });
            }

            report.BattleReportWave.Add(reportWave);
        }

        return report;
    }

    private static int ResolveBigHuntCostumeId(DarkUserMemoryDatabase userDb, int userDeckNumber, int deckCharacterNumber, int fallbackDeckNumber)
    {
        int deckNumber = userDeckNumber == 0 ? fallbackDeckNumber : userDeckNumber;
        foreach (DeckType deckType in new[] { DeckType.BIG_HUNT, DeckType.QUEST })
        {
            EntityIUserDeck? deck = userDb.EntityIUserDeck
                .FirstOrDefault(d => d.DeckType == deckType && d.UserDeckNumber == deckNumber);
            if (deck is null)
            {
                continue;
            }

            string? deckCharacterUuid = deckCharacterNumber switch
            {
                1 => deck.UserDeckCharacterUuid01,
                2 => deck.UserDeckCharacterUuid02,
                3 => deck.UserDeckCharacterUuid03,
                _ => null
            };
            if (string.IsNullOrEmpty(deckCharacterUuid))
            {
                continue;
            }

            EntityIUserDeckCharacter? deckCharacter = userDb.EntityIUserDeckCharacter
                .FirstOrDefault(dc => dc.UserDeckCharacterUuid == deckCharacterUuid);
            if (deckCharacter is null || string.IsNullOrEmpty(deckCharacter.UserCostumeUuid))
            {
                continue;
            }

            EntityIUserCostume? costume = userDb.EntityIUserCostume
                .FirstOrDefault(c => c.UserCostumeUuid == deckCharacter.UserCostumeUuid);
            if (costume is not null)
            {
                return costume.CostumeId;
            }
        }

        return 0;
    }

    // ────────── Possession grant ──────────

    /// <summary>
    /// Routes possession grants through PossessionHelper.Apply for consistent handling.
    /// </summary>
    private void GrantPossessionViaPossessionHelper(DarkUserMemoryDatabase userDb, PossessionType possessionType, int possessionId, int count, long nowMs)
    {
        PossessionHelper.Apply(userDb, possessionType, possessionId, count, _masterDb, nowMs);
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
