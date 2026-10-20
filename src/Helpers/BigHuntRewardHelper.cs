using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

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
