using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Helpers;

/// <summary>
/// Enhance campaigns: a value of 400 adds 40 percentage points to the memoir success rate and to the great success
/// chance of costume and weapon enhances.
/// </summary>
public sealed class EnhanceCampaignHelper
{
    /// <summary>Chance, before campaigns, that a costume or weapon enhance is a great success (permil).</summary>
    public const int GreatSuccessChancePermil = 200;

    private readonly IReadOnlyList<EnhanceCampaignRow> _campaigns;
    private readonly Random _random;

    public EnhanceCampaignHelper(DarkMasterMemoryDatabase masterDb, Random? random = null)
    {
        _random = random ?? Random.Shared;

        Dictionary<int, List<EntityMEnhanceCampaignTargetGroup>> targetsByGroupId = masterDb.EntityMEnhanceCampaignTargetGroup
            .GroupBy(t => t.EnhanceCampaignTargetGroupId)
            .ToDictionary(g => g.Key, g => g.ToList());

        List<EnhanceCampaignRow> campaigns = [];
        foreach (EntityMEnhanceCampaign campaign in masterDb.EntityMEnhanceCampaign)
        {
            if (targetsByGroupId.TryGetValue(campaign.EnhanceCampaignTargetGroupId, out List<EntityMEnhanceCampaignTargetGroup>? targets))
            {
                campaigns.Add(new EnhanceCampaignRow(campaign, targets));
            }
        }

        _campaigns = campaigns;
    }

    /// <summary>Rolls whether a costume enhance is a great success; every active campaign for it adds to the chance.</summary>
    public bool RollCostumeGreatSuccess(EntityMCostume costume, long nowMs)
        => RollGreatSuccess(nowMs, t => MatchesCostume(t, costume));

    /// <summary>Rolls whether a weapon enhance is a great success; every active campaign for it adds to the chance.</summary>
    public bool RollWeaponGreatSuccess(EntityMWeapon weapon, long nowMs)
        => RollGreatSuccess(nowMs, t => MatchesWeapon(t, weapon));

    /// <summary>What a great success gives back of a cost: half, rounded down.</summary>
    public static int GreatSuccessRefund(int cost) => cost / 2;

    private bool RollGreatSuccess(long nowMs, Func<EntityMEnhanceCampaignTargetGroup, bool> matches)
    {
        int bonus = SumPermil(nowMs, EnhanceCampaignEffectType.ADDITIONAL_PER_MILE, matches)
            + SumPermil(nowMs, EnhanceCampaignEffectType.PROBABILITY_PER_MILE, matches);
        return _random.Next(1000) < Math.Min(1000, GreatSuccessChancePermil + bonus);
    }

    private static bool MatchesCostume(EntityMEnhanceCampaignTargetGroup t, EntityMCostume costume) => t.EnhanceCampaignTargetType switch
    {
        EnhanceCampaignTargetType.COSTUME_ALL => true,
        EnhanceCampaignTargetType.COSTUME_CHARACTER_ID => t.EnhanceCampaignTargetValue == costume.CharacterId,
        EnhanceCampaignTargetType.COSTUME_SKILLFUL_WEAPON_TYPE_ID => t.EnhanceCampaignTargetValue == (int)costume.SkillfulWeaponType,
        EnhanceCampaignTargetType.COSTUME_ID => t.EnhanceCampaignTargetValue == costume.CostumeId,
        _ => false
    };

    private static bool MatchesWeapon(EntityMEnhanceCampaignTargetGroup t, EntityMWeapon weapon) => t.EnhanceCampaignTargetType switch
    {
        EnhanceCampaignTargetType.WEAPON_ALL => true,
        EnhanceCampaignTargetType.WEAPON_TYPE_ID => t.EnhanceCampaignTargetValue == (int)weapon.WeaponType,
        EnhanceCampaignTargetType.WEAPON_ATTRIBUTE_TYPE_ID => t.EnhanceCampaignTargetValue == (int)weapon.AttributeType,
        EnhanceCampaignTargetType.WEAPON_ID => t.EnhanceCampaignTargetValue == weapon.WeaponId,
        _ => false
    };

    /// <summary>Memoir enhance success rate (permil) with active campaigns of either effect type added, capped at 1000.</summary>
    public int ApplyPartsSuccessRate(int ratePermil, EntityMParts parts, long nowMs)
    {
        bool Matches(EntityMEnhanceCampaignTargetGroup t) => t.EnhanceCampaignTargetType switch
        {
            EnhanceCampaignTargetType.PARTS_ALL => true,
            EnhanceCampaignTargetType.PARTS_SERIES_ID => t.EnhanceCampaignTargetValue == parts.PartsGroupId,
            EnhanceCampaignTargetType.PARTS_ID => t.EnhanceCampaignTargetValue == parts.PartsId,
            _ => false
        };

        int bonus = SumPermil(nowMs, EnhanceCampaignEffectType.ADDITIONAL_PER_MILE, Matches)
            + SumPermil(nowMs, EnhanceCampaignEffectType.PROBABILITY_PER_MILE, Matches);
        return Math.Min(1000, ratePermil + bonus);
    }

    private int SumPermil(long nowMs, EnhanceCampaignEffectType effectType, Func<EntityMEnhanceCampaignTargetGroup, bool> matches)
    {
        int sum = 0;
        foreach (EnhanceCampaignRow row in _campaigns)
        {
            EntityMEnhanceCampaign c = row.Campaign;
            if (c.EnhanceCampaignEffectType == effectType &&
                nowMs >= c.StartDatetime && nowMs <= c.EndDatetime &&
                c.TargetUserStatusType == TargetUserStatusType.ALL &&
                row.Targets.Any(matches))
            {
                sum += c.EnhanceCampaignEffectValue;
            }
        }

        return sum;
    }

    private sealed record EnhanceCampaignRow(EntityMEnhanceCampaign Campaign, IReadOnlyList<EntityMEnhanceCampaignTargetGroup> Targets);
}
