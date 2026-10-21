using MariesWonderland.Data;
using MariesWonderland.Helpers;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Missions;

/// <summary>
/// Mission pass (a monthly pass) rewards. A pass's missions reward points; each level needs its
/// <c>NecessaryPoint</c>. Every reached level has a free reward, and premium rewards for players who own the pass's
/// premium item (bought in the shop), including levels reached before buying it. Claimed levels are tracked per
/// track on <c>IUserMissionPassPoint</c>.
/// </summary>
public static class MissionPassHelper
{
    /// <summary>
    /// Grants every reached level of the pass not claimed yet: free rewards always, premium rewards when the premium
    /// item is owned. Returns the rewards granted (empty when there was nothing to claim).
    /// </summary>
    public static List<MissionRewardGrant> Claim(DarkUserMemoryDatabase userDb, DarkMasterMemoryDatabase masterDb, int missionPassId, long nowMs)
    {
        EntityMMissionPass? pass = masterDb.EntityMMissionPass.Find(missionPassId);
        EntityIUserMissionPassPoint? progress = userDb.EntityIUserMissionPassPoint.FirstOrDefault(p => p.MissionPassId == missionPassId);
        if (pass == null || progress == null)
        {
            return [];
        }

        int reached = ReachedLevel(masterDb, pass, progress.Point);
        bool hasPremium = HasPremium(userDb, pass);
        List<EntityMMissionPassRewardGroup> rewards = [.. masterDb.EntityMMissionPassRewardGroup
            .Where(r => r.MissionPassRewardGroupId == pass.MissionPassRewardGroupId && r.Level <= reached
                && (r.IsPremium
                    ? hasPremium && r.Level > progress.PremiumRewardReceivedLevel
                    : r.Level > progress.NoPremiumRewardReceivedLevel))
            .OrderBy(r => r.Level).ThenBy(r => r.IsPremium).ThenBy(r => r.SortOrder)];

        List<MissionRewardGrant> granted = [];
        foreach (EntityMMissionPassRewardGroup reward in rewards)
        {
            PossessionHelper.Apply(userDb, reward.PossessionType, reward.PossessionId, reward.Count, masterDb, nowMs);
            granted.Add(new MissionRewardGrant(reward.PossessionType, reward.PossessionId, reward.Count));
        }

        progress.NoPremiumRewardReceivedLevel = Math.Max(progress.NoPremiumRewardReceivedLevel, reached);
        if (hasPremium)
        {
            progress.PremiumRewardReceivedLevel = Math.Max(progress.PremiumRewardReceivedLevel, reached);
        }

        return granted;
    }

    /// <summary>
    /// Claims what's left on the most recently ended pass that still has unclaimed levels. Returns that pass's id,
    /// or 0 when no ended pass has anything left.
    /// </summary>
    public static int ClaimRemaining(DarkUserMemoryDatabase userDb, DarkMasterMemoryDatabase masterDb, long nowMs)
    {
        EntityMMissionPass? pass = masterDb.EntityMMissionPass
            .Where(p => p.EndDatetime < nowMs && HasUnclaimed(userDb, masterDb, p))
            .OrderByDescending(p => p.EndDatetime)
            .FirstOrDefault();
        if (pass == null)
        {
            return 0;
        }

        Claim(userDb, masterDb, pass.MissionPassId, nowMs);
        return pass.MissionPassId;
    }

    /// <summary>The highest level whose points the pass has reached (0 below the first level).</summary>
    public static int ReachedLevel(DarkMasterMemoryDatabase masterDb, EntityMMissionPass pass, int points)
        => masterDb.EntityMMissionPassLevelGroup
            .Where(l => l.MissionPassLevelGroupId == pass.MissionPassLevelGroupId && l.NecessaryPoint <= points)
            .Select(l => l.Level).DefaultIfEmpty(0).Max();

    private static bool HasPremium(DarkUserMemoryDatabase userDb, EntityMMissionPass pass)
        => pass.PremiumItemId != 0 && userDb.EntityIUserPremiumItem.Any(p => p.PremiumItemId == pass.PremiumItemId);

    private static bool HasUnclaimed(DarkUserMemoryDatabase userDb, DarkMasterMemoryDatabase masterDb, EntityMMissionPass pass)
    {
        EntityIUserMissionPassPoint? progress = userDb.EntityIUserMissionPassPoint.FirstOrDefault(p => p.MissionPassId == pass.MissionPassId);
        if (progress == null)
        {
            return false;
        }

        int reached = ReachedLevel(masterDb, pass, progress.Point);
        return reached > progress.NoPremiumRewardReceivedLevel || (HasPremium(userDb, pass) && reached > progress.PremiumRewardReceivedLevel);
    }
}
