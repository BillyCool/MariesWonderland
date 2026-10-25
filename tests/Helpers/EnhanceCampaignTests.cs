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
    public void RollGreatSuccess_WithoutCampaigns_WinsBelowTheBaseChance()
    {
        Assert.True(Build(new FixedRandom(199)).RollCostumeGreatSuccess(Costume, During));
        Assert.True(Build(new FixedRandom(199)).RollWeaponGreatSuccess(Weapon, During));
        Assert.False(Build(new FixedRandom(200)).RollCostumeGreatSuccess(Costume, During));
        Assert.False(Build(new FixedRandom(200)).RollWeaponGreatSuccess(Weapon, During));
    }

    [Fact]
    public void RollGreatSuccess_CampaignsOfBothEffectTypesAddToTheChance_OnlyForMatchingTargetsInTheirWindow()
    {
        (int, EnhanceCampaignEffectType, int, EnhanceCampaignTargetType, int)[] campaigns =
        [
            (1, EnhanceCampaignEffectType.ADDITIONAL_PER_MILE, 200, EnhanceCampaignTargetType.COSTUME_ALL, 0),
            (2, EnhanceCampaignEffectType.ADDITIONAL_PER_MILE, 400, EnhanceCampaignTargetType.COSTUME_ID, Costume.CostumeId),
            (3, EnhanceCampaignEffectType.ADDITIONAL_PER_MILE, 400, EnhanceCampaignTargetType.COSTUME_ID, 99999),
            (4, EnhanceCampaignEffectType.PROBABILITY_PER_MILE, 100, EnhanceCampaignTargetType.COSTUME_CHARACTER_ID, Costume.CharacterId),
            (5, EnhanceCampaignEffectType.ADDITIONAL_PER_MILE, 400, EnhanceCampaignTargetType.WEAPON_ALL, 0)
        ];

        // 200 base + 200 + 400 + 100 = 900.
        Assert.True(Build(new FixedRandom(899), campaigns).RollCostumeGreatSuccess(Costume, During));
        Assert.False(Build(new FixedRandom(900), campaigns).RollCostumeGreatSuccess(Costume, During));
        Assert.False(Build(new FixedRandom(200), campaigns).RollCostumeGreatSuccess(Costume, End + 1));
        // The weapon only gets its own 400.
        Assert.True(Build(new FixedRandom(599), campaigns).RollWeaponGreatSuccess(Weapon, During));
        Assert.False(Build(new FixedRandom(600), campaigns).RollWeaponGreatSuccess(Weapon, During));
    }

    [Fact]
    public void RollGreatSuccess_WeaponCampaignsMatchTypeAttributeAndId_AndCapAtCertain()
    {
        (int, EnhanceCampaignEffectType, int, EnhanceCampaignTargetType, int)[] campaigns =
        [
            (1, EnhanceCampaignEffectType.ADDITIONAL_PER_MILE, 100, EnhanceCampaignTargetType.WEAPON_TYPE_ID, (int)WeaponType.SPEAR),
            (2, EnhanceCampaignEffectType.ADDITIONAL_PER_MILE, 200, EnhanceCampaignTargetType.WEAPON_ATTRIBUTE_TYPE_ID, (int)AttributeType.FIRE),
            (3, EnhanceCampaignEffectType.ADDITIONAL_PER_MILE, 400, EnhanceCampaignTargetType.WEAPON_ID, Weapon.WeaponId)
        ];

        Assert.True(Build(new FixedRandom(899), campaigns).RollWeaponGreatSuccess(Weapon, During));
        Assert.False(Build(new FixedRandom(900), campaigns).RollWeaponGreatSuccess(Weapon, During));

        (int, EnhanceCampaignEffectType, int, EnhanceCampaignTargetType, int)[] certain =
            [(1, EnhanceCampaignEffectType.ADDITIONAL_PER_MILE, 800, EnhanceCampaignTargetType.WEAPON_ALL, 0)];
        Assert.True(Build(new FixedRandom(999), certain).RollWeaponGreatSuccess(Weapon, During));
    }

    [Fact]
    public void GreatSuccessRefund_IsHalfRoundedDown()
    {
        Assert.Equal(5, EnhanceCampaignHelper.GreatSuccessRefund(10));
        Assert.Equal(2, EnhanceCampaignHelper.GreatSuccessRefund(5));
        Assert.Equal(0, EnhanceCampaignHelper.GreatSuccessRefund(1));
    }

    [Fact]
    public void ApplyPartsSuccessRate_RaisesTheRateAndCapsAtCertain()
    {
        EnhanceCampaignHelper campaigns = Build(
            (1, EnhanceCampaignEffectType.ADDITIONAL_PER_MILE, 500, EnhanceCampaignTargetType.PARTS_ALL, 0),
            (2, EnhanceCampaignEffectType.ADDITIONAL_PER_MILE, 300, EnhanceCampaignTargetType.PARTS_SERIES_ID, Parts.PartsGroupId));

        Assert.Equal(900, campaigns.ApplyPartsSuccessRate(100, Parts, During));
        Assert.Equal(1000, campaigns.ApplyPartsSuccessRate(300, Parts, During));
        Assert.Equal(500, campaigns.ApplyPartsSuccessRate(500, Parts, End + 1));
    }

    private sealed class FixedRandom(int value) : Random
    {
        public override int Next(int maxValue) => value;
    }

    private static EnhanceCampaignHelper Build(params (int Id, EnhanceCampaignEffectType Effect, int Value, EnhanceCampaignTargetType TargetType, int TargetValue)[] rows)
        => Build(null, rows);

    private static EnhanceCampaignHelper Build(Random? random, params (int Id, EnhanceCampaignEffectType Effect, int Value, EnhanceCampaignTargetType TargetType, int TargetValue)[] rows)
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

        return new EnhanceCampaignHelper(masterDb, random);
    }
}
