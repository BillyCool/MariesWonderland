using MariesWonderland.Data;
using MariesWonderland.Helpers;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Tests.Helpers;

/// <summary>
/// Enhance campaigns: the best active matching campaign wins (additional adds to the base, probability replaces it),
/// target matching, windows, and beginner/comeback campaigns tied to the user's own dates.
/// </summary>
public class EnhanceCampaignTests
{
    private const long Start = 1_000_000;
    private const long End = 2_000_000;
    private const long During = 1_500_000;
    private const long DayMs = 24L * 60 * 60 * 1000;

    private static readonly EntityMCostume Costume = new() { CostumeId = 21002, CharacterId = 3, SkillfulWeaponType = WeaponType.SWORD };
    private static readonly EntityMWeapon Weapon = new() { WeaponId = 210171, WeaponType = WeaponType.SPEAR, AttributeType = AttributeType.FIRE };
    private static readonly EntityMParts Parts = new() { PartsId = 5, PartsGroupId = 7 };

    private static DarkUserMemoryDatabase NewUser() => new() { UserId = 1 };

    [Fact]
    public void RollGreatSuccess_WithoutCampaigns_WinsBelowTheBaseChance()
    {
        DarkUserMemoryDatabase user = NewUser();
        Assert.True(Build(new FixedRandom(199)).RollCostumeGreatSuccess(Costume, user, During));
        Assert.True(Build(new FixedRandom(199)).RollWeaponGreatSuccess(Weapon, user, During));
        Assert.False(Build(new FixedRandom(200)).RollCostumeGreatSuccess(Costume, user, During));
        Assert.False(Build(new FixedRandom(200)).RollWeaponGreatSuccess(Weapon, user, During));
    }

    [Fact]
    public void RollGreatSuccess_BestMatchingCampaignInItsWindowWins()
    {
        Row[] campaigns =
        [
            new(1, EnhanceCampaignEffectType.ADDITIONAL_PER_MILE, 200, EnhanceCampaignTargetType.COSTUME_ALL, 0),
            new(2, EnhanceCampaignEffectType.ADDITIONAL_PER_MILE, 400, EnhanceCampaignTargetType.COSTUME_ID, Costume.CostumeId),
            new(3, EnhanceCampaignEffectType.ADDITIONAL_PER_MILE, 700, EnhanceCampaignTargetType.COSTUME_ID, 99999),
            new(4, EnhanceCampaignEffectType.PROBABILITY_PER_MILE, 100, EnhanceCampaignTargetType.COSTUME_CHARACTER_ID, Costume.CharacterId),
            new(5, EnhanceCampaignEffectType.ADDITIONAL_PER_MILE, 400, EnhanceCampaignTargetType.WEAPON_ALL, 0)
        ];
        DarkUserMemoryDatabase user = NewUser();

        // Best of 200+200, 200+400 and 100 (campaign 3 is for another costume): 600, not their sum.
        Assert.True(Build(new FixedRandom(599), campaigns).RollCostumeGreatSuccess(Costume, user, During));
        Assert.False(Build(new FixedRandom(600), campaigns).RollCostumeGreatSuccess(Costume, user, During));
        // Outside the window: the base 200.
        Assert.False(Build(new FixedRandom(200), campaigns).RollCostumeGreatSuccess(Costume, user, End + 1));
        // The weapon only gets its own 200+400.
        Assert.True(Build(new FixedRandom(599), campaigns).RollWeaponGreatSuccess(Weapon, user, During));
        Assert.False(Build(new FixedRandom(600), campaigns).RollWeaponGreatSuccess(Weapon, user, During));
    }

    [Fact]
    public void RollGreatSuccess_WeaponCampaignsMatchTypeAttributeAndId_AndCapAtCertain()
    {
        Row[] campaigns =
        [
            new(1, EnhanceCampaignEffectType.ADDITIONAL_PER_MILE, 100, EnhanceCampaignTargetType.WEAPON_TYPE_ID, (int)WeaponType.SPEAR),
            new(2, EnhanceCampaignEffectType.ADDITIONAL_PER_MILE, 200, EnhanceCampaignTargetType.WEAPON_ATTRIBUTE_TYPE_ID, (int)AttributeType.FIRE),
            new(3, EnhanceCampaignEffectType.ADDITIONAL_PER_MILE, 400, EnhanceCampaignTargetType.WEAPON_ID, Weapon.WeaponId)
        ];
        DarkUserMemoryDatabase user = NewUser();

        Assert.True(Build(new FixedRandom(599), campaigns).RollWeaponGreatSuccess(Weapon, user, During));
        Assert.False(Build(new FixedRandom(600), campaigns).RollWeaponGreatSuccess(Weapon, user, During));

        Row[] certain = [new(1, EnhanceCampaignEffectType.ADDITIONAL_PER_MILE, 900, EnhanceCampaignTargetType.WEAPON_ALL, 0)];
        Assert.True(Build(new FixedRandom(999), certain).RollWeaponGreatSuccess(Weapon, user, During));
    }

    [Fact]
    public void GreatSuccessRefund_IsHalfRoundedDown()
    {
        Assert.Equal(5, EnhanceCampaignHelper.GreatSuccessRefund(10));
        Assert.Equal(2, EnhanceCampaignHelper.GreatSuccessRefund(5));
        Assert.Equal(0, EnhanceCampaignHelper.GreatSuccessRefund(1));
    }

    [Fact]
    public void ApplyPartsSuccessRate_UsesTheBestCampaign_AndCapsAtCertain()
    {
        EnhanceCampaignHelper campaigns = Build(null,
            new Row(1, EnhanceCampaignEffectType.ADDITIONAL_PER_MILE, 500, EnhanceCampaignTargetType.PARTS_ALL, 0),
            new Row(2, EnhanceCampaignEffectType.ADDITIONAL_PER_MILE, 300, EnhanceCampaignTargetType.PARTS_SERIES_ID, Parts.PartsGroupId));
        DarkUserMemoryDatabase user = NewUser();

        Assert.Equal(600, campaigns.ApplyPartsSuccessRate(100, Parts, user, During));
        Assert.Equal(1000, campaigns.ApplyPartsSuccessRate(600, Parts, user, During));
        Assert.Equal(500, campaigns.ApplyPartsSuccessRate(500, Parts, user, End + 1));
    }

    [Fact]
    public void ApplyPartsSuccessRate_ProbabilityCampaignSetsTheRate()
    {
        EnhanceCampaignHelper campaigns = Build(null,
            new Row(1, EnhanceCampaignEffectType.PROBABILITY_PER_MILE, 300, EnhanceCampaignTargetType.PARTS_ID, Parts.PartsId));

        Assert.Equal(300, campaigns.ApplyPartsSuccessRate(100, Parts, NewUser(), During));
        Assert.Equal(300, campaigns.ApplyPartsSuccessRate(800, Parts, NewUser(), During));
    }

    [Fact]
    public void BeginnerCampaign_AppliesOnlyDuringTheUsersOwnTerm()
    {
        EnhanceCampaignHelper campaigns = Build(null,
            new Row(1, EnhanceCampaignEffectType.ADDITIONAL_PER_MILE, 800, EnhanceCampaignTargetType.PARTS_ALL, 0, TargetUserStatusType.BEGINNER));
        long registered = During;
        DarkUserMemoryDatabase beginner = NewUser();
        beginner.EntityIUserBeginnerCampaign.Add(new EntityIUserBeginnerCampaign { UserId = 1, BeginnerCampaignId = 1, CampaignRegisterDatetime = registered });

        Assert.Equal(100, campaigns.ApplyPartsSuccessRate(100, Parts, NewUser(), registered));
        Assert.Equal(900, campaigns.ApplyPartsSuccessRate(100, Parts, beginner, registered));
        Assert.Equal(900, campaigns.ApplyPartsSuccessRate(100, Parts, beginner, registered + 7 * DayMs));
        Assert.Equal(100, campaigns.ApplyPartsSuccessRate(100, Parts, beginner, registered + 7 * DayMs + 1));
    }

    [Fact]
    public void ComebackCampaign_NeedsTheComebackDateInsideTheCampaign()
    {
        EnhanceCampaignHelper campaigns = Build(null,
            new Row(1, EnhanceCampaignEffectType.ADDITIONAL_PER_MILE, 400, EnhanceCampaignTargetType.PARTS_ALL, 0, TargetUserStatusType.COMEBACK));
        DarkUserMemoryDatabase inside = NewUser();
        inside.EntityIUserComebackCampaign.Add(new EntityIUserComebackCampaign { UserId = 1, ComebackCampaignId = 1, ComebackDatetime = During });
        DarkUserMemoryDatabase before = NewUser();
        before.EntityIUserComebackCampaign.Add(new EntityIUserComebackCampaign { UserId = 1, ComebackCampaignId = 1, ComebackDatetime = Start - 1 });

        Assert.Equal(500, campaigns.ApplyPartsSuccessRate(100, Parts, inside, During + DayMs));
        // Came back before the campaign started: not for them, even while their own term runs.
        Assert.Equal(100, campaigns.ApplyPartsSuccessRate(100, Parts, before, Start + 1));
        // Past their 14-day term.
        Assert.Equal(100, campaigns.ApplyPartsSuccessRate(100, Parts, inside, During + 14 * DayMs + 1));
    }

    private sealed class FixedRandom(int value) : Random
    {
        public override int Next(int maxValue) => value;
    }

    private sealed record Row(
        int Id,
        EnhanceCampaignEffectType Effect,
        int Value,
        EnhanceCampaignTargetType TargetType,
        int TargetValue,
        TargetUserStatusType UserStatus = TargetUserStatusType.ALL);

    private static EnhanceCampaignHelper Build(Random? random, params Row[] rows)
    {
        DarkMasterMemoryDatabase masterDb = new();
        masterDb.EntityMBeginnerCampaign.Add(new EntityMBeginnerCampaign { BeginnerCampaignId = 1, GrantCampaignTermDayCount = 7 });
        masterDb.EntityMComebackCampaign.Add(new EntityMComebackCampaign { ComebackCampaignId = 1, GrantCampaignTermDayCount = 14 });
        foreach (Row row in rows)
        {
            masterDb.EntityMEnhanceCampaign.Add(new EntityMEnhanceCampaign
            {
                EnhanceCampaignId = row.Id,
                EnhanceCampaignTargetGroupId = row.Id,
                EnhanceCampaignEffectType = row.Effect,
                EnhanceCampaignEffectValue = row.Value,
                StartDatetime = Start,
                EndDatetime = End,
                TargetUserStatusType = row.UserStatus
            });
            masterDb.EntityMEnhanceCampaignTargetGroup.Add(new EntityMEnhanceCampaignTargetGroup
            {
                EnhanceCampaignTargetGroupId = row.Id,
                EnhanceCampaignTargetType = row.TargetType,
                EnhanceCampaignTargetValue = row.TargetValue
            });
        }

        return new EnhanceCampaignHelper(masterDb, random);
    }
}
