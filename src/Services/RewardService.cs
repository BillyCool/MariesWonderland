using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Helpers;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.BigHunt;
using MariesWonderland.Proto.Labyrinth;
using MariesWonderland.Proto.Pvp;
using MariesWonderland.Proto.Reward;

namespace MariesWonderland.Services;

public class RewardService(UserDataStore store, DarkMasterMemoryDatabase masterDb)
    : MariesWonderland.Proto.Reward.RewardService.RewardServiceBase
{
    private readonly UserDataStore _store = store;
    private readonly DarkMasterMemoryDatabase _masterDb = masterDb;

    /// <summary>
    /// Collects the weekly score rewards for all bosses and grants any unclaimed ones to the user.
    /// </summary>
    public override Task<ReceiveBigHuntRewardResponse> ReceiveBigHuntReward(Empty request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);
        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        long weeklyVersion = GetWeeklyVersion(nowMs);

        EntityIUserBigHuntWeeklyStatus? ws = userDb.EntityIUserBigHuntWeeklyStatus
            .FirstOrDefault(s => s.BigHuntWeeklyVersion == weeklyVersion);

        bool isReceived = ws?.IsReceivedWeeklyReward ?? false;

        List<WeeklyScoreResult> weeklyScoreResults = [];
        List<BigHuntReward> weeklyRewards = [];

        foreach (EntityMBigHuntBoss boss in _masterDb.EntityMBigHuntBoss)
        {
            EntityIUserBigHuntWeeklyMaxScore? wms = userDb.EntityIUserBigHuntWeeklyMaxScore
                .FirstOrDefault(s => s.UserId == userId
                    && s.BigHuntWeeklyVersion == weeklyVersion
                    && s.AttributeType == boss.AttributeType);

            long maxScore = wms?.MaxScore ?? 0;
            int gradeIcon = ResolveGradeIconId(boss.BigHuntBossId, maxScore);

            weeklyScoreResults.Add(new WeeklyScoreResult
            {
                AttributeType = (int)boss.AttributeType,
                BeforeMaxScore = maxScore,
                CurrentMaxScore = maxScore,
                BeforeAssetGradeIconId = gradeIcon,
                CurrentAssetGradeIconId = gradeIcon,
                AfterMaxScore = maxScore,
                AfterAssetGradeIconId = gradeIcon,
            });
        }

        if (!isReceived)
        {
            foreach (EntityMBigHuntBoss boss in _masterDb.EntityMBigHuntBoss)
            {
                int rewardGroupId = ResolveActiveWeeklyRewardGroupId((int)boss.AttributeType, nowMs);
                if (rewardGroupId == 0) { continue; }

                EntityIUserBigHuntWeeklyMaxScore? wms = userDb.EntityIUserBigHuntWeeklyMaxScore
                    .FirstOrDefault(s => s.UserId == userId
                        && s.BigHuntWeeklyVersion == weeklyVersion
                        && s.AttributeType == boss.AttributeType);

                long maxScore = wms?.MaxScore ?? 0;

                List<EntityMBigHuntRewardGroup> items = CollectNewRewards(rewardGroupId, 0, maxScore);
                foreach (EntityMBigHuntRewardGroup item in items)
                {
                    PossessionHelper.Apply(userDb, item.PossessionType, item.PossessionId, item.Count, _masterDb);
                    weeklyRewards.Add(new BigHuntReward
                    {
                        PossessionType = (int)item.PossessionType,
                        PossessionId = item.PossessionId,
                        Count = item.Count,
                    });
                }
            }

            if (ws == null)
            {
                ws = new EntityIUserBigHuntWeeklyStatus
                {
                    UserId = userId,
                    BigHuntWeeklyVersion = weeklyVersion,
                };
                userDb.EntityIUserBigHuntWeeklyStatus.Add(ws);
            }
            ws.IsReceivedWeeklyReward = true;
            isReceived = true;
        }

        ReceiveBigHuntRewardResponse response = new()
        {
            IsReceivedWeeklyScoreReward = isReceived,
        };
        response.WeeklyScoreResult.AddRange(weeklyScoreResults);
        response.WeeklyScoreReward.AddRange(weeklyRewards);
        return Task.FromResult(response);
    }

    /// <summary>
    /// Grants any claimable PvP weekly rewards and any ended-season rewards supported by the current models.
    /// </summary>
    public override Task<ReceivePvpRewardResponse> ReceivePvpReward(Empty request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        if (!_store.TryGet(userId, out DarkUserMemoryDatabase? userDb))
        {
            return Task.FromResult(new ReceivePvpRewardResponse());
        }

        EntityIUserPvpStatus? existingStatus = userDb.EntityIUserPvpStatus.FirstOrDefault(s => s.UserId == userId);
        long latestReceivedWeeklyVersion = existingStatus?.LatestRewardReceivePvpWeeklyVersion ?? 0;
        int latestReceivedSeasonId = existingStatus?.LatestRewardReceivePvpSeasonId ?? 0;
        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        ReceivePvpRewardResponse response = new();

        long newestClaimedWeeklyVersion = latestReceivedWeeklyVersion;
        foreach (EntityIUserPvpWeeklyResult weeklyResult in userDb.EntityIUserPvpWeeklyResult
            .Where(r => r.UserId == userId && r.PvpWeeklyVersion > latestReceivedWeeklyVersion)
            .OrderBy(r => r.PvpWeeklyVersion))
        {
            EntityMPvpSeason? season = _masterDb.EntityMPvpSeason.FirstOrDefault(s => s.PvpSeasonId == weeklyResult.PvpSeasonId);
            if (season is null)
            {
                break;
            }

            int gradeRewardGroupId = ResolvePvpGradeWeeklyRewardGroupId(season, weeklyResult.FinalPoint);
            int weeklyRankRewardGroupId = ResolvePvpWeeklyRankRewardGroupId(season, weeklyResult.FinalRank);

            ApplyPvpRewards(userDb, CollectPvpRewards(
                gradeRewardGroupId,
                _masterDb.EntityMPvpGradeWeeklyRewardGroup,
                g => g.PvpGradeWeeklyRewardGroupId,
                g => g.PvpRewardId,
                g => g.SortOrder));
            ApplyPvpRewards(userDb, CollectPvpRewards(
                weeklyRankRewardGroupId,
                _masterDb.EntityMPvpWeeklyRankRewardGroup,
                g => g.PvpWeeklyRankRewardGroupId,
                g => g.PvpRewardId,
                g => g.SortOrder));

            response.WeeklyGradeResult = new WeeklyGradeResult
            {
                TargetSeasonId = weeklyResult.PvpSeasonId,
                PvpPoint = weeklyResult.FinalPoint,
                PvpGradeWeeklyRewardGroupId = gradeRewardGroupId
            };
            response.WeeklyRankResult = new WeeklyRankResult
            {
                TargetSeasonId = weeklyResult.PvpSeasonId,
                Rank = weeklyResult.FinalRank,
                PvpWeeklyRankRewardGroupId = weeklyRankRewardGroupId
            };

            newestClaimedWeeklyVersion = weeklyResult.PvpWeeklyVersion;
        }

        int newestClaimedSeasonId = latestReceivedSeasonId;
        foreach (IGrouping<int, EntityIUserPvpWeeklyResult> seasonResults in userDb.EntityIUserPvpWeeklyResult
            .Where(r => r.UserId == userId && r.PvpSeasonId > latestReceivedSeasonId)
            .GroupBy(r => r.PvpSeasonId)
            .OrderBy(g => g.Key))
        {
            EntityIUserPvpWeeklyResult latestSeasonResult = seasonResults
                .OrderBy(r => r.PvpWeeklyVersion)
                .Last();
            EntityMPvpSeason? season = _masterDb.EntityMPvpSeason.FirstOrDefault(s => s.PvpSeasonId == latestSeasonResult.PvpSeasonId);
            if (season is null)
            {
                break;
            }

            if (season.SeasonEndDatetime > nowMs)
            {
                break;
            }

            int seasonRankRewardGroupId = ResolvePvpSeasonRankRewardGroupId(season, latestSeasonResult.FinalRank);
            ApplyPvpRewards(userDb, CollectPvpRewards(
                seasonRankRewardGroupId,
                _masterDb.EntityMPvpSeasonRankRewardGroup,
                g => g.PvpSeasonRankRewardGroupId,
                g => g.PvpRewardId,
                g => g.SortOrder));

            response.SeasonRankResult = new SeasonRankResult
            {
                TargetSeasonId = latestSeasonResult.PvpSeasonId,
                Rank = latestSeasonResult.FinalRank,
                PvpSeasonRankRewardGroupId = seasonRankRewardGroupId
            };

            newestClaimedSeasonId = latestSeasonResult.PvpSeasonId;
        }

        if (newestClaimedWeeklyVersion == latestReceivedWeeklyVersion
            && newestClaimedSeasonId == latestReceivedSeasonId)
        {
            return Task.FromResult(response);
        }

        EntityIUserPvpStatus status = existingStatus ?? userDb.EntityIUserPvpStatus.GetOrCreate(userId);
        status.LatestRewardReceivePvpWeeklyVersion = newestClaimedWeeklyVersion;
        status.LatestRewardReceivePvpSeasonId = newestClaimedSeasonId;
        status.LatestVersion = nowMs;

        return Task.FromResult(response);
    }

    /// <summary>
    /// The current labyrinth models do not record enough per-season progression to safely resolve claimable season rewards.
    /// </summary>
    public override Task<ReceiveLabyrinthSeasonRewardResponse> ReceiveLabyrinthSeasonReward(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new ReceiveLabyrinthSeasonRewardResponse());
    }

    /// <summary>
    /// Returns an empty mission pass remaining reward response (not yet implemented).
    /// </summary>
    public override Task<ReceiveMissionPassRemainingRewardResponse> ReceiveMissionPassRemainingReward(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new ReceiveMissionPassRemainingRewardResponse());
    }

    /// <summary>
    /// Returns the Monday 00:00 UTC timestamp in milliseconds for the week containing the given timestamp.
    /// </summary>
    private static long GetWeeklyVersion(long millis)
    {
        DateTimeOffset dt = DateTimeOffset.FromUnixTimeMilliseconds(millis).ToUniversalTime();
        int weekday = (int)dt.DayOfWeek;
        if (weekday == 0) { weekday = 7; }
        DateTimeOffset monday = new(dt.Year, dt.Month, dt.Day, 0, 0, 0, TimeSpan.Zero);
        monday = monday.AddDays(-(weekday - 1));
        return monday.ToUnixTimeMilliseconds();
    }

    private int ResolveGradeIconId(int bossId, long score)
    {
        EntityMBigHuntBoss? boss = _masterDb.EntityMBigHuntBoss.FirstOrDefault(b => b.BigHuntBossId == bossId);
        if (boss == null) { return 0; }

        List<EntityMBigHuntBossGradeGroup> thresholds = [.. _masterDb.EntityMBigHuntBossGradeGroup
            .Where(g => g.BigHuntBossGradeGroupId == boss.BigHuntBossGradeGroupId)
            .OrderBy(g => g.NecessaryScore)];

        int iconId = 0;
        foreach (EntityMBigHuntBossGradeGroup t in thresholds)
        {
            if (score >= t.NecessaryScore) { iconId = t.AssetGradeIconId; }
            else { break; }
        }
        return iconId;
    }

    /// <summary>
    /// Finds the active weekly reward group for a given attribute type at the specified time.
    /// Uses ScheduleId=1 (the default schedule).
    /// </summary>
    private int ResolveActiveWeeklyRewardGroupId(int attributeType, long nowMs)
    {
        List<EntityMBigHuntWeeklyAttributeScoreRewardGroupSchedule> entries = [.. _masterDb
            .EntityMBigHuntWeeklyAttributeScoreRewardGroupSchedule
            .Where(e => e.AttributeType == (AttributeType)attributeType
                && e.BigHuntWeeklyAttributeScoreRewardGroupScheduleId == 1)
            .OrderByDescending(e => e.StartDatetime)];

        foreach (EntityMBigHuntWeeklyAttributeScoreRewardGroupSchedule e in entries)
        {
            if (nowMs >= e.StartDatetime) { return e.BigHuntScoreRewardGroupId; }
        }
        return entries.Count > 0 ? entries[^1].BigHuntScoreRewardGroupId : 0;
    }

    /// <summary>
    /// Collects reward items for thresholds between oldMax (exclusive) and newMax (inclusive).
    /// </summary>
    private List<EntityMBigHuntRewardGroup> CollectNewRewards(int scoreRewardGroupId, long oldMax, long newMax)
    {
        List<EntityMBigHuntScoreRewardGroup> thresholds = [.. _masterDb.EntityMBigHuntScoreRewardGroup
            .Where(t => t.BigHuntScoreRewardGroupId == scoreRewardGroupId)
            .OrderBy(t => t.NecessaryScore)];

        List<EntityMBigHuntRewardGroup> items = [];
        foreach (EntityMBigHuntScoreRewardGroup t in thresholds)
        {
            if (t.NecessaryScore > oldMax && t.NecessaryScore <= newMax)
            {
                items.AddRange(_masterDb.EntityMBigHuntRewardGroup
                    .Where(r => r.BigHuntRewardGroupId == t.BigHuntRewardGroupId));
            }
        }
        return items;
    }

    private int ResolvePvpGradeWeeklyRewardGroupId(EntityMPvpSeason season, int finalPoint)
    {
        List<EntityMPvpSeasonGrade> seasonGrades = [.. _masterDb.EntityMPvpSeasonGrade
            .Where(g => g.PvpSeasonId == season.PvpSeasonId)
            .OrderBy(g => g.NecessaryPvpPoint)];
        if (seasonGrades.Count > 0)
        {
            int rewardGroupId = 0;
            foreach (EntityMPvpSeasonGrade grade in seasonGrades)
            {
                if (finalPoint >= grade.NecessaryPvpPoint)
                {
                    rewardGroupId = grade.PvpGradeWeeklyRewardGroupId;
                }
                else
                {
                    break;
                }
            }
            return rewardGroupId;
        }

        List<EntityMPvpGradeGroup> gradeGroups = [.. _masterDb.EntityMPvpGradeGroup
            .Where(g => g.PvpGradeGroupId == season.PvpGradeGroupId)
            .OrderBy(g => g.NecessaryPvpPoint)];
        int fallbackRewardGroupId = 0;
        foreach (EntityMPvpGradeGroup grade in gradeGroups)
        {
            if (finalPoint >= grade.NecessaryPvpPoint)
            {
                fallbackRewardGroupId = grade.PvpGradeWeeklyRewardGroupId;
            }
            else
            {
                break;
            }
        }
        return fallbackRewardGroupId;
    }

    private int ResolvePvpWeeklyRankRewardGroupId(EntityMPvpSeason season, int finalRank)
        => ResolvePvpRankRewardGroupId(
            season.PvpWeeklyRankRewardRankGroupId,
            finalRank,
            _masterDb.EntityMPvpWeeklyRankRewardRankGroup,
            g => g.PvpWeeklyRankRewardRankGroupId,
            g => g.RankLowerLimit,
            g => g.PvpWeeklyRankRewardGroupId);

    private int ResolvePvpSeasonRankRewardGroupId(EntityMPvpSeason season, int finalRank)
        => ResolvePvpRankRewardGroupId(
            season.PvpSeasonRankRewardRankGroupId,
            finalRank,
            _masterDb.EntityMPvpSeasonRankRewardRankGroup,
            g => g.PvpSeasonRankRewardRankGroupId,
            g => g.RankLowerLimit,
            g => g.PvpSeasonRankRewardGroupId);

    private static int ResolvePvpRankRewardGroupId<T>(
        int rankGroupId,
        int finalRank,
        IEnumerable<T> groups,
        Func<T, int> rankGroupIdSelector,
        Func<T, int> lowerLimitSelector,
        Func<T, int> rewardGroupIdSelector)
    {
        if (rankGroupId == 0 || finalRank <= 0)
        {
            return 0;
        }

        int rewardGroupId = 0;
        foreach (T group in groups
            .Where(g => rankGroupIdSelector(g) == rankGroupId)
            .OrderBy(g => lowerLimitSelector(g)))
        {
            if (finalRank >= lowerLimitSelector(group))
            {
                rewardGroupId = rewardGroupIdSelector(group);
            }
            else
            {
                break;
            }
        }

        return rewardGroupId;
    }

    private List<EntityMPvpReward> CollectPvpRewards<T>(
        int rewardGroupId,
        IEnumerable<T> groups,
        Func<T, int> rewardGroupIdSelector,
        Func<T, int> rewardIdSelector,
        Func<T, int> sortOrderSelector)
    {
        if (rewardGroupId == 0)
        {
            return [];
        }

        List<int> rewardIds = [.. groups
            .Where(g => rewardGroupIdSelector(g) == rewardGroupId)
            .OrderBy(sortOrderSelector)
            .Select(rewardIdSelector)];
        return [.. rewardIds.Join(
            _masterDb.EntityMPvpReward,
            rewardId => rewardId,
            reward => reward.PvpRewardId,
            (_, reward) => reward)];
    }

    private void ApplyPvpRewards(DarkUserMemoryDatabase userDb, List<EntityMPvpReward> rewards)
    {
        if (rewards.Count == 0)
        {
            return;
        }

        if (userDb.EntityIUserGem.Count == 0)
        {
            userDb.EntityIUserGem.Add(new EntityIUserGem { UserId = userDb.UserId });
        }

        foreach (EntityMPvpReward reward in rewards)
        {
            PossessionHelper.Apply(userDb, reward.PossessionType, reward.PossessionId, reward.Count, _masterDb);
        }
    }

}
