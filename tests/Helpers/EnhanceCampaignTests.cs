using MariesWonderland.Data;
using MariesWonderland.Helpers;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Tests.Helpers;

/// <summary>Enhance campaigns: value/1000 bonus multipliers, target matching, windows and additive overlap.</summary>
public class EnhanceCampaignTests
{
    private const long Start = 1_000_000;
    private const long End = 2_000_000;
    private const long During = 1_500_000;

    private static readonly EntityMCostume Costume = new() { CostumeId = 21002, CharacterId = 3, SkillfulWeaponType = WeaponType.SWORD };
    private static readonly EntityMWeapon Weapon = new() { WeaponId = 210171, WeaponType = WeaponType.SPEAR, AttributeType = AttributeType.FIRE };
    private static readonly EntityMParts Parts = new() { PartsId = 5, PartsGroupId = 7 };

    [Fact]
    public void ApplyCostumeExp_OutsideTheWindow_IsUnchanged()
    {
        EnhanceCampaignHelper campaigns = Build((1, EnhanceCampaignEffectType.ADDITIONAL_PER_MILE, 400, EnhanceCampaignTargetType.COSTUME_ALL, 0));

        Assert.Equal(1000, campaigns.ApplyCostumeExp(1000, Costume, Start - 1));
        Assert.Equal(1000, campaigns.ApplyCostumeExp(1000, Costume, End + 1));
        Assert.Equal(1400, campaigns.ApplyCostumeExp(1000, Costume, During));
    }

    [Fact]
    public void ApplyCostumeExp_OverlappingCampaignsAdd()
    {
        EnhanceCampaignHelper campaigns = Build(
            (1, EnhanceCampaignEffectType.ADDITIONAL_PER_MILE, 200, EnhanceCampaignTargetType.COSTUME_ALL, 0),
            (2, EnhanceCampaignEffectType.ADDITIONAL_PER_MILE, 400, EnhanceCampaignTargetType.COSTUME_ID, Costume.CostumeId),
            (3, EnhanceCampaignEffectType.ADDITIONAL_PER_MILE, 400, EnhanceCampaignTargetType.COSTUME_ID, 99999));

        Assert.Equal(1600, campaigns.ApplyCostumeExp(1000, Costume, During));
    }

    [Fact]
    public void ApplyCostumeExp_MatchesCharacterAndSkillfulWeaponType()
    {
        EnhanceCampaignHelper campaigns = Build(
            (1, EnhanceCampaignEffectType.ADDITIONAL_PER_MILE, 100, EnhanceCampaignTargetType.COSTUME_CHARACTER_ID, Costume.CharacterId),
            (2, EnhanceCampaignEffectType.ADDITIONAL_PER_MILE, 200, EnhanceCampaignTargetType.COSTUME_SKILLFUL_WEAPON_TYPE_ID, (int)WeaponType.SWORD),
            (3, EnhanceCampaignEffectType.ADDITIONAL_PER_MILE, 400, EnhanceCampaignTargetType.WEAPON_ALL, 0));

        Assert.Equal(1300, campaigns.ApplyCostumeExp(1000, Costume, During));
    }

    [Fact]
    public void ApplyWeaponExp_MatchesTypeAttributeAndId_AndIgnoresProbabilityCampaigns()
    {
        EnhanceCampaignHelper campaigns = Build(
            (1, EnhanceCampaignEffectType.ADDITIONAL_PER_MILE, 100, EnhanceCampaignTargetType.WEAPON_TYPE_ID, (int)WeaponType.SPEAR),
            (2, EnhanceCampaignEffectType.ADDITIONAL_PER_MILE, 200, EnhanceCampaignTargetType.WEAPON_ATTRIBUTE_TYPE_ID, (int)AttributeType.FIRE),
            (3, EnhanceCampaignEffectType.ADDITIONAL_PER_MILE, 400, EnhanceCampaignTargetType.WEAPON_ID, Weapon.WeaponId),
            (4, EnhanceCampaignEffectType.PROBABILITY_PER_MILE, 400, EnhanceCampaignTargetType.WEAPON_ALL, 0));

        Assert.Equal(1700, campaigns.ApplyWeaponExp(1000, Weapon, During));
    }

    [Fact]
    public void ApplyPartsSuccessRate_RaisesTheRateAndCapsAtCertain()
    {
        EnhanceCampaignHelper campaigns = Build(
            (1, EnhanceCampaignEffectType.ADDITIONAL_PER_MILE, 500, EnhanceCampaignTargetType.PARTS_ALL, 0),
            (2, EnhanceCampaignEffectType.ADDITIONAL_PER_MILE, 300, EnhanceCampaignTargetType.PARTS_SERIES_ID, Parts.PartsGroupId));

        Assert.Equal(900, campaigns.ApplyPartsSuccessRate(500, Parts, During));
        Assert.Equal(1000, campaigns.ApplyPartsSuccessRate(600, Parts, During));
        Assert.Equal(500, campaigns.ApplyPartsSuccessRate(500, Parts, End + 1));
    }

    private static EnhanceCampaignHelper Build(params (int Id, EnhanceCampaignEffectType Effect, int Value, EnhanceCampaignTargetType TargetType, int TargetValue)[] rows)
    {
        DarkMasterMemoryDatabase masterDb = new();
        foreach ((int id, EnhanceCampaignEffectType effect, int value, EnhanceCampaignTargetType targetType, int targetValue) in rows)
        {
            masterDb.EntityMEnhanceCampaign.Add(new EntityMEnhanceCampaign
            {
                EnhanceCampaignId = id,
                EnhanceCampaignTargetGroupId = id,
                EnhanceCampaignEffectType = effect,
                EnhanceCampaignEffectValue = value,
                StartDatetime = Start,
                EndDatetime = End,
                TargetUserStatusType = TargetUserStatusType.ALL
            });
            masterDb.EntityMEnhanceCampaignTargetGroup.Add(new EntityMEnhanceCampaignTargetGroup
            {
                EnhanceCampaignTargetGroupId = id,
                EnhanceCampaignTargetType = targetType,
                EnhanceCampaignTargetValue = targetValue
            });
        }

        return new EnhanceCampaignHelper(masterDb);
    }
}
