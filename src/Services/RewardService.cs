using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Helpers;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.BigHunt;
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
                    PossessionHelper.Apply(userDb, userId, item.PossessionType, item.PossessionId, item.Count, _masterDb);
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
    /// Returns an empty PvP reward response (not yet implemented).
    /// </summary>
    public override Task<ReceivePvpRewardResponse> ReceivePvpReward(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new ReceivePvpRewardResponse());
    }

    /// <summary>
    /// Returns an empty labyrinth season reward response (not yet implemented).
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

}
