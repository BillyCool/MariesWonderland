using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.BigHunt;

namespace MariesWonderland.Helpers;

/// <summary>
/// Big Hunt score rewards. A score earns the rewards of the single highest tier it reaches (each tier is a full
/// bundle, not an increment), for battle, skip and weekly rewards alike.
/// </summary>
public static class BigHuntRewardHelper
{
    /// <summary>The boss quest's score reward group in effect at <paramref name="nowMs"/>.</summary>
    public static int ResolveScoreRewardGroupId(DarkMasterMemoryDatabase masterDb, int scoreRewardGroupScheduleId, long nowMs)
        => ResolveByStart(
            masterDb.EntityMBigHuntScoreRewardGroupSchedule
                .Where(e => e.BigHuntScoreRewardGroupScheduleId == scoreRewardGroupScheduleId)
                .Select(e => (e.StartDatetime, e.BigHuntScoreRewardGroupId)),
            nowMs);

    /// <summary>
    /// The schedule (season) whose challenge window contains <paramref name="nowMs"/>, else the one that ended last.
    /// </summary>
    public static int ResolveActiveScheduleId(DarkMasterMemoryDatabase masterDb, long nowMs)
    {
        EntityMBigHuntSchedule? active = masterDb.EntityMBigHuntSchedule
            .FirstOrDefault(s => nowMs >= s.ChallengeStartDatetime && nowMs <= s.ChallengeEndDatetime);
        if (active is not null)
        {
            return active.BigHuntScheduleId;
        }

        return masterDb.EntityMBigHuntSchedule
            .OrderByDescending(s => s.ChallengeEndDatetime)
            .Select(s => s.BigHuntScheduleId)
            .FirstOrDefault();
    }

    /// <summary>
    /// A user's best score against a boss in the current season, or 0 without one. The weekly max of an attribute
    /// carries it into every week of the season (the real save's weekly rows only fall at a new season).
    /// </summary>
    public static long SeasonBest(DarkMasterMemoryDatabase masterDb, DarkUserMemoryDatabase userDb, int bossId, long nowMs)
    {
        int scheduleId = ResolveActiveScheduleId(masterDb, nowMs);
        return userDb.EntityIUserBigHuntScheduleMaxScore
            .FirstOrDefault(m => m.BigHuntScheduleId == scheduleId && m.BigHuntBossId == bossId)?.MaxScore ?? 0;
    }

    /// <summary>The week whose weekly reward is claimed now: the game week that just ended.</summary>
    public static long ClaimWeekVersion(long nowMs) => GameDayHelper.StartOfWeek(nowMs) - (long)TimeSpan.FromDays(7).TotalMilliseconds;

    /// <summary>
    /// An attribute's weekly max for a week: its row, or 0 when the week has none (a week the user never signed in
    /// during pays nothing).
    /// </summary>
    public static long WeekScore(DarkMasterMemoryDatabase masterDb, DarkUserMemoryDatabase userDb, long weeklyVersion, EntityMBigHuntBoss boss, long nowMs)
        => userDb.EntityIUserBigHuntWeeklyMaxScore
            .FirstOrDefault(m => m.BigHuntWeeklyVersion == weeklyVersion && m.AttributeType == boss.AttributeType)?.MaxScore ?? 0;

    /// <summary>True when the week has nothing left to claim: no score in it, or its reward already received.</summary>
    public static bool IsWeekSettled(DarkMasterMemoryDatabase masterDb, DarkUserMemoryDatabase userDb, long weeklyVersion, long nowMs)
        => !masterDb.EntityMBigHuntBoss.Any(b => WeekScore(masterDb, userDb, weeklyVersion, b, nowMs) > 0)
            || userDb.EntityIUserBigHuntWeeklyStatus.Any(s => s.BigHuntWeeklyVersion == weeklyVersion && s.IsReceivedWeeklyReward);

    /// <summary>
    /// Brings the current game week up to date: its weekly status, and each attribute's weekly max starting from
    /// the best score of the season so far, as in the real save (a row for every week).
    /// </summary>
    public static void EnsureWeek(DarkMasterMemoryDatabase masterDb, DarkUserMemoryDatabase userDb, long userId, long nowMs)
    {
        long weeklyVersion = GameDayHelper.StartOfWeek(nowMs);
        if (!userDb.EntityIUserBigHuntWeeklyStatus.Any(s => s.BigHuntWeeklyVersion == weeklyVersion))
        {
            userDb.EntityIUserBigHuntWeeklyStatus.Add(new EntityIUserBigHuntWeeklyStatus { UserId = userId, BigHuntWeeklyVersion = weeklyVersion });
        }

        foreach (EntityMBigHuntBoss boss in masterDb.EntityMBigHuntBoss)
        {
            long best = SeasonBest(masterDb, userDb, boss.BigHuntBossId, nowMs);
            if (best > 0 && !userDb.EntityIUserBigHuntWeeklyMaxScore.Any(m => m.BigHuntWeeklyVersion == weeklyVersion && m.AttributeType == boss.AttributeType))
            {
                userDb.EntityIUserBigHuntWeeklyMaxScore.Add(new EntityIUserBigHuntWeeklyMaxScore
                {
                    UserId = userId,
                    BigHuntWeeklyVersion = weeklyVersion,
                    AttributeType = boss.AttributeType,
                    MaxScore = best
                });
            }
        }
    }

    /// <summary>The attribute's weekly reward group in effect at <paramref name="nowMs"/>. Each attribute has its own schedule.</summary>
    public static int ResolveWeeklyRewardGroupId(DarkMasterMemoryDatabase masterDb, AttributeType attributeType, long nowMs)
        => ResolveByStart(
            masterDb.EntityMBigHuntWeeklyAttributeScoreRewardGroupSchedule
                .Where(e => e.AttributeType == attributeType)
                .Select(e => (e.StartDatetime, e.BigHuntScoreRewardGroupId)),
            nowMs);

    /// <summary>The reward items of the highest tier of <paramref name="scoreRewardGroupId"/> that <paramref name="score"/> reaches; empty below the first tier.</summary>
    public static List<EntityMBigHuntRewardGroup> HighestTierItems(DarkMasterMemoryDatabase masterDb, int scoreRewardGroupId, long score)
    {
        EntityMBigHuntScoreRewardGroup? tier = masterDb.EntityMBigHuntScoreRewardGroup
            .Where(t => t.BigHuntScoreRewardGroupId == scoreRewardGroupId && t.NecessaryScore <= score)
            .OrderByDescending(t => t.NecessaryScore)
            .FirstOrDefault();
        if (tier is null)
        {
            return [];
        }

        return [.. masterDb.EntityMBigHuntRewardGroup
            .Where(r => r.BigHuntRewardGroupId == tier.BigHuntRewardGroupId)
            .OrderBy(r => r.SortOrder)];
    }

    /// <summary>Merges rewards of the same possession into one entry with the summed count, in first-seen order; the client lists each entry as its own tile.</summary>
    public static List<BigHuntReward> Combine(IEnumerable<BigHuntReward> rewards)
    {
        List<BigHuntReward> combined = [];
        foreach (BigHuntReward reward in rewards)
        {
            BigHuntReward? existing = combined.FirstOrDefault(r => r.PossessionType == reward.PossessionType && r.PossessionId == reward.PossessionId);
            if (existing is null)
            {
                combined.Add(reward.Clone());
            }
            else
            {
                existing.Count += reward.Count;
            }
        }

        return combined;
    }

    /// <summary>The group whose start is the latest one not after <paramref name="nowMs"/>; the earliest group before any start.</summary>
    private static int ResolveByStart(IEnumerable<(long StartDatetime, int GroupId)> entries, long nowMs)
    {
        List<(long StartDatetime, int GroupId)> ordered = [.. entries.OrderByDescending(e => e.StartDatetime)];
        foreach ((long start, int groupId) in ordered)
        {
            if (nowMs >= start)
            {
                return groupId;
            }
        }

        return ordered.Count > 0 ? ordered[^1].GroupId : 0;
    }
}
