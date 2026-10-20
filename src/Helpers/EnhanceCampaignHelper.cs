using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Helpers;

/// <summary>
/// Enhance campaigns: a value of 400 multiplies the target quantity by 1.4 (costume/weapon EXP, memoir success rate).
/// </summary>
public sealed class EnhanceCampaignHelper
{
    private readonly IReadOnlyList<EnhanceCampaignRow> _campaigns;

    public EnhanceCampaignHelper(DarkMasterMemoryDatabase masterDb)
    {
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

    /// <summary>Costume enhance EXP after active EXP campaigns.</summary>
    public int ApplyCostumeExp(int exp, EntityMCostume costume, long nowMs)
        => ApplyPermil(exp, SumPermil(nowMs, EnhanceCampaignEffectType.ADDITIONAL_PER_MILE, t => t.EnhanceCampaignTargetType switch
        {
            EnhanceCampaignTargetType.COSTUME_ALL => true,
            EnhanceCampaignTargetType.COSTUME_CHARACTER_ID => t.EnhanceCampaignTargetValue == costume.CharacterId,
            EnhanceCampaignTargetType.COSTUME_SKILLFUL_WEAPON_TYPE_ID => t.EnhanceCampaignTargetValue == (int)costume.SkillfulWeaponType,
            EnhanceCampaignTargetType.COSTUME_ID => t.EnhanceCampaignTargetValue == costume.CostumeId,
            _ => false
        }));

    /// <summary>Weapon enhance EXP after active EXP campaigns.</summary>
    public int ApplyWeaponExp(int exp, EntityMWeapon weapon, long nowMs)
        => ApplyPermil(exp, SumPermil(nowMs, EnhanceCampaignEffectType.ADDITIONAL_PER_MILE, t => t.EnhanceCampaignTargetType switch
        {
            EnhanceCampaignTargetType.WEAPON_ALL => true,
            EnhanceCampaignTargetType.WEAPON_TYPE_ID => t.EnhanceCampaignTargetValue == (int)weapon.WeaponType,
            EnhanceCampaignTargetType.WEAPON_ATTRIBUTE_TYPE_ID => t.EnhanceCampaignTargetValue == (int)weapon.AttributeType,
            EnhanceCampaignTargetType.WEAPON_ID => t.EnhanceCampaignTargetValue == weapon.WeaponId,
            _ => false
        }));

    /// <summary>Memoir enhance success rate (permil) after active campaigns of either effect type, capped at 1000.</summary>
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
        return Math.Min(1000, ApplyPermil(ratePermil, bonus));
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

    private static int ApplyPermil(int value, int bonusPermil)
        => (int)((long)value * (1000 + bonusPermil) / 1000);

    private sealed record EnhanceCampaignRow(EntityMEnhanceCampaign Campaign, IReadOnlyList<EntityMEnhanceCampaignTargetGroup> Targets);
}
