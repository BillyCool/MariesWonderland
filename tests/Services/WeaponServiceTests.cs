using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Weapon;
using MariesWonderland.Tests.Infrastructure;
using WeaponServiceImpl = MariesWonderland.Services.WeaponService;

namespace MariesWonderland.Tests.Services;

public class WeaponServiceTests
{
    private const long UserId = 1L;
    private const int GoldItemId = 900;
    private const int MedalItemId = 901;
    private const int TargetWeaponId = 1001;
    private const int FodderWeaponId = 1002;
    private const int EvolvedWeaponId = 1003;
    private const int SameWeaponMaterialId = 2001;
    private const int OffWeaponMaterialId = 2002;
    private const int SkillMaterialId = 2003;
    private const int AbilityMaterialId = 2004;
    private const int LimitBreakMaterialId = 2005;
    private const int EvolutionMaterialAId = 2006;
    private const int EvolutionMaterialBId = 2007;
    private const int AwakenMaterialAId = 2008;
    private const int AwakenMaterialBId = 2009;
    private const int SkillId = 3001;
    private const int AbilityId = 4001;
    private const int MainEnhanceId = 5001;
    private const int FodderEnhanceId = 5002;
    private const int ExpMapId = 6001;
    private const int WeaponStoryGroupId = 7001;
    private const int EvolvedWeaponStoryGroupId = 7002;
    private const int WeaponSkillGroupId = 8001;
    private const int WeaponAbilityGroupId = 8002;
    private const int WeaponSkillMaterialGroupId = 8101;
    private const int WeaponAbilityMaterialGroupId = 8102;
    private const int WeaponEvolutionGroupId = 8201;
    private const int WeaponEvolutionMaterialGroupId = 8202;
    private const int WeaponAwakenMaterialGroupId = 8203;
    private const int SellPriceFunctionId = 9001;
    private const int MaxLevelFunctionId = 9002;
    private const int EnhanceByMaterialCostFunctionId = 9003;
    private const int EnhanceByWeaponCostFunctionId = 9004;
    private const int MaxSkillLevelFunctionId = 9005;
    private const int SkillCostFunctionId = 9006;
    private const int MaxAbilityLevelFunctionId = 9007;
    private const int AbilityCostFunctionId = 9008;
    private const int LimitBreakByMaterialCostFunctionId = 9009;
    private const int LimitBreakByWeaponCostFunctionId = 9010;
    private const int EvolutionCostFunctionId = 9011;

    [Fact]
    public async Task Sell_RemovesSoldWeaponRecordsAndAddsGoldAndExchangeItems()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        ConfigureMainWeapon(masterDb, sellPriceFunctionId: SellPriceFunctionId);
        AddLinearFunction(masterDb, SellPriceFunctionId, slope: 0, intercept: 11);
        masterDb.EntityMWeaponConsumeExchangeConsumableItemGroup.Add(new EntityMWeaponConsumeExchangeConsumableItemGroup
        {
            WeaponId = TargetWeaponId,
            ConsumableItemId = MedalItemId,
            Count = 2
        });

        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserConsumableItem.AddRange(
        [
            new EntityIUserConsumableItem
            {
                UserId = UserId,
                ConsumableItemId = GoldItemId,
                Count = 10,
                FirstAcquisitionDatetime = 100
            },
            new EntityIUserConsumableItem
            {
                UserId = UserId,
                ConsumableItemId = MedalItemId,
                Count = 5,
                FirstAcquisitionDatetime = 200
            }
        ]);
        userDb.EntityIUserWeapon.AddRange(
        [
            new EntityIUserWeapon
            {
                UserId = UserId,
                UserWeaponUuid = "sold-weapon",
                WeaponId = TargetWeaponId,
                Level = 4
            },
            new EntityIUserWeapon
            {
                UserId = UserId,
                UserWeaponUuid = "kept-weapon",
                WeaponId = TargetWeaponId,
                Level = 1
            }
        ]);
        userDb.EntityIUserWeaponSkill.AddRange(
        [
            new EntityIUserWeaponSkill
            {
                UserId = UserId,
                UserWeaponUuid = "sold-weapon",
                SlotNumber = 1,
                Level = 2
            },
            new EntityIUserWeaponSkill
            {
                UserId = UserId,
                UserWeaponUuid = "kept-weapon",
                SlotNumber = 1,
                Level = 1
            }
        ]);
        userDb.EntityIUserWeaponAbility.AddRange(
        [
            new EntityIUserWeaponAbility
            {
                UserId = UserId,
                UserWeaponUuid = "sold-weapon",
                SlotNumber = 1,
                Level = 2
            },
            new EntityIUserWeaponAbility
            {
                UserId = UserId,
                UserWeaponUuid = "kept-weapon",
                SlotNumber = 1,
                Level = 1
            }
        ]);
        userDb.EntityIUserWeaponAwaken.AddRange(
        [
            new EntityIUserWeaponAwaken
            {
                UserId = UserId,
                UserWeaponUuid = "sold-weapon"
            },
            new EntityIUserWeaponAwaken
            {
                UserId = UserId,
                UserWeaponUuid = "kept-weapon"
            }
        ]);

        WeaponServiceImpl service = CreateService(masterDb, userDb);
        SellRequest request = new();
        request.UserWeaponUuid.Add("sold-weapon");
        request.UserWeaponUuid.Add("missing-weapon");

        SellResponse response = await service.Sell(request, FakeServerCallContext.For(UserId));

        Assert.NotNull(response);
        Assert.Single(userDb.EntityIUserWeapon);
        Assert.Equal("kept-weapon", userDb.EntityIUserWeapon.Single().UserWeaponUuid);
        Assert.Single(userDb.EntityIUserWeaponSkill);
        Assert.Equal("kept-weapon", userDb.EntityIUserWeaponSkill.Single().UserWeaponUuid);
        Assert.Single(userDb.EntityIUserWeaponAbility);
        Assert.Equal("kept-weapon", userDb.EntityIUserWeaponAbility.Single().UserWeaponUuid);
        Assert.Single(userDb.EntityIUserWeaponAwaken);
        Assert.Equal("kept-weapon", userDb.EntityIUserWeaponAwaken.Single().UserWeaponUuid);
        Assert.Equal(21, Assert.Single(userDb.EntityIUserConsumableItem, item => item.ConsumableItemId == GoldItemId).Count);
        Assert.Equal(7, Assert.Single(userDb.EntityIUserConsumableItem, item => item.ConsumableItemId == MedalItemId).Count);
    }

    [Fact]
    public async Task ProtectAndUnprotect_OnlyUpdatesMatchingWeapons()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserWeapon.AddRange(
        [
            new EntityIUserWeapon
            {
                UserId = UserId,
                UserWeaponUuid = "target-weapon",
                WeaponId = TargetWeaponId,
                IsProtected = false
            },
            new EntityIUserWeapon
            {
                UserId = UserId,
                UserWeaponUuid = "other-weapon",
                WeaponId = TargetWeaponId,
                IsProtected = true
            }
        ]);

        WeaponServiceImpl service = CreateService(masterDb, userDb);
        ProtectRequest protectRequest = new();
        protectRequest.UserWeaponUuid.Add("target-weapon");
        protectRequest.UserWeaponUuid.Add("missing-weapon");

        await service.Protect(protectRequest, FakeServerCallContext.For(UserId));

        Assert.True(Assert.Single(userDb.EntityIUserWeapon, weapon => weapon.UserWeaponUuid == "target-weapon").IsProtected);
        Assert.True(Assert.Single(userDb.EntityIUserWeapon, weapon => weapon.UserWeaponUuid == "other-weapon").IsProtected);

        UnprotectRequest unprotectRequest = new();
        unprotectRequest.UserWeaponUuid.Add("target-weapon");
        unprotectRequest.UserWeaponUuid.Add("missing-weapon");

        await service.Unprotect(unprotectRequest, FakeServerCallContext.For(UserId));

        Assert.False(Assert.Single(userDb.EntityIUserWeapon, weapon => weapon.UserWeaponUuid == "target-weapon").IsProtected);
        Assert.True(Assert.Single(userDb.EntityIUserWeapon, weapon => weapon.UserWeaponUuid == "other-weapon").IsProtected);
    }

    [Fact]
    public async Task EnhanceByMaterial_AppliesSameWeaponBonusCapsExpAndUnlocksStories()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        ConfigureMainWeapon(
            masterDb,
            weaponType: WeaponType.SWORD,
            maxLevelFunctionId: MaxLevelFunctionId,
            requiredExpMapId: ExpMapId,
            enhancementCostByMaterialFunctionId: EnhanceByMaterialCostFunctionId,
            weaponStoryReleaseConditionGroupId: WeaponStoryGroupId);
        AddLinearFunction(masterDb, MaxLevelFunctionId, slope: 0, intercept: 3);
        AddLinearFunction(masterDb, EnhanceByMaterialCostFunctionId, slope: 10, intercept: 0);
        AddLevelThreshold(masterDb, ExpMapId, level: 1, requiredExp: 0);
        AddLevelThreshold(masterDb, ExpMapId, level: 2, requiredExp: 100);
        AddLevelThreshold(masterDb, ExpMapId, level: 3, requiredExp: 300);
        AddStoryCondition(masterDb, WeaponStoryGroupId, storyIndex: 2, WeaponStoryReleaseConditionType.REACH_SPECIFIED_LEVEL, conditionValue: 2);
        AddStoryCondition(masterDb, WeaponStoryGroupId, storyIndex: 3, WeaponStoryReleaseConditionType.REACH_INITIAL_MAX_LEVEL, conditionValue: 0);
        AddMaterial(masterDb, SameWeaponMaterialId, effectValue: 100, weaponType: WeaponType.SWORD);
        AddMaterial(masterDb, OffWeaponMaterialId, effectValue: 50, weaponType: WeaponType.GUN);

        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserWeapon.Add(new EntityIUserWeapon
        {
            UserId = UserId,
            UserWeaponUuid = "target-weapon",
            WeaponId = TargetWeaponId,
            Level = 1,
            Exp = 0
        });
        userDb.EntityIUserConsumableItem.Add(new EntityIUserConsumableItem
        {
            UserId = UserId,
            ConsumableItemId = GoldItemId,
            Count = 100
        });
        userDb.EntityIUserMaterial.AddRange(
        [
            new EntityIUserMaterial
            {
                UserId = UserId,
                MaterialId = SameWeaponMaterialId,
                Count = 2
            },
            new EntityIUserMaterial
            {
                UserId = UserId,
                MaterialId = OffWeaponMaterialId,
                Count = 1
            }
        ]);

        WeaponServiceImpl service = CreateService(masterDb, userDb);
        EnhanceByMaterialRequest request = new() { UserWeaponUuid = "target-weapon" };
        request.Materials[SameWeaponMaterialId] = 2;
        request.Materials[OffWeaponMaterialId] = 1;
        request.Materials[9999] = 5;

        EnhanceByMaterialResponse response = await service.EnhanceByMaterial(request, FakeServerCallContext.For(UserId));

        EntityIUserWeapon weapon = Assert.Single(userDb.EntityIUserWeapon);
        Assert.NotNull(response);
        Assert.False(response.IsGreatSuccess);
        Assert.Equal(3, weapon.Level);
        Assert.Equal(300, weapon.Exp);
        Assert.Equal(70, Assert.Single(userDb.EntityIUserConsumableItem).Count);
        Assert.Equal(0, Assert.Single(userDb.EntityIUserMaterial, material => material.MaterialId == SameWeaponMaterialId).Count);
        Assert.Equal(0, Assert.Single(userDb.EntityIUserMaterial, material => material.MaterialId == OffWeaponMaterialId).Count);
        Assert.Equal(3, Assert.Single(userDb.EntityIUserWeaponStory).ReleasedMaxStoryIndex);
    }

    [Fact]
    public async Task EnhanceByWeapon_ConsumesFodderWeaponsAndAttachedRecords()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        ConfigureMainWeapon(
            masterDb,
            weaponType: WeaponType.SWORD,
            maxLevelFunctionId: MaxLevelFunctionId,
            requiredExpMapId: ExpMapId,
            enhancementCostByWeaponFunctionId: EnhanceByWeaponCostFunctionId,
            weaponStoryReleaseConditionGroupId: WeaponStoryGroupId);
        ConfigureFodderWeapon(masterDb, weaponType: WeaponType.GUN, baseEnhancementObtainedExp: 50);
        masterDb.EntityMWeaponSpecificEnhance.Single(enhance => enhance.WeaponSpecificEnhanceId == MainEnhanceId).BaseEnhancementObtainedExp = 120;
        AddLinearFunction(masterDb, MaxLevelFunctionId, slope: 0, intercept: 3);
        AddLinearFunction(masterDb, EnhanceByWeaponCostFunctionId, slope: 15, intercept: 0);
        AddLevelThreshold(masterDb, ExpMapId, level: 1, requiredExp: 0);
        AddLevelThreshold(masterDb, ExpMapId, level: 2, requiredExp: 100);
        AddLevelThreshold(masterDb, ExpMapId, level: 3, requiredExp: 200);
        AddStoryCondition(masterDb, WeaponStoryGroupId, storyIndex: 2, WeaponStoryReleaseConditionType.REACH_SPECIFIED_LEVEL, conditionValue: 3);
        masterDb.EntityMWeaponConsumeExchangeConsumableItemGroup.AddRange(
        [
            new EntityMWeaponConsumeExchangeConsumableItemGroup
            {
                WeaponId = TargetWeaponId,
                ConsumableItemId = MedalItemId,
                Count = 1
            },
            new EntityMWeaponConsumeExchangeConsumableItemGroup
            {
                WeaponId = FodderWeaponId,
                ConsumableItemId = MedalItemId,
                Count = 2
            }
        ]);

        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserWeapon.AddRange(
        [
            new EntityIUserWeapon
            {
                UserId = UserId,
                UserWeaponUuid = "target-weapon",
                WeaponId = TargetWeaponId,
                Level = 1,
                Exp = 0
            },
            new EntityIUserWeapon
            {
                UserId = UserId,
                UserWeaponUuid = "same-fodder",
                WeaponId = TargetWeaponId,
                Level = 1
            },
            new EntityIUserWeapon
            {
                UserId = UserId,
                UserWeaponUuid = "gun-fodder",
                WeaponId = FodderWeaponId,
                Level = 1
            }
        ]);
        userDb.EntityIUserWeaponSkill.AddRange(
        [
            new EntityIUserWeaponSkill
            {
                UserId = UserId,
                UserWeaponUuid = "same-fodder",
                SlotNumber = 1,
                Level = 1
            },
            new EntityIUserWeaponSkill
            {
                UserId = UserId,
                UserWeaponUuid = "gun-fodder",
                SlotNumber = 1,
                Level = 1
            }
        ]);
        userDb.EntityIUserWeaponAbility.AddRange(
        [
            new EntityIUserWeaponAbility
            {
                UserId = UserId,
                UserWeaponUuid = "same-fodder",
                SlotNumber = 1,
                Level = 1
            },
            new EntityIUserWeaponAbility
            {
                UserId = UserId,
                UserWeaponUuid = "gun-fodder",
                SlotNumber = 1,
                Level = 1
            }
        ]);
        userDb.EntityIUserWeaponAwaken.AddRange(
        [
            new EntityIUserWeaponAwaken
            {
                UserId = UserId,
                UserWeaponUuid = "same-fodder"
            },
            new EntityIUserWeaponAwaken
            {
                UserId = UserId,
                UserWeaponUuid = "gun-fodder"
            }
        ]);
        userDb.EntityIUserConsumableItem.AddRange(
        [
            new EntityIUserConsumableItem
            {
                UserId = UserId,
                ConsumableItemId = GoldItemId,
                Count = 100
            },
            new EntityIUserConsumableItem
            {
                UserId = UserId,
                ConsumableItemId = MedalItemId,
                Count = 4
            }
        ]);

        WeaponServiceImpl service = CreateService(masterDb, userDb);
        EnhanceByWeaponRequest request = new() { UserWeaponUuid = "target-weapon" };
        request.MaterialUserWeaponUuids.Add("same-fodder");
        request.MaterialUserWeaponUuids.Add("missing-fodder");
        request.MaterialUserWeaponUuids.Add("gun-fodder");

        EnhanceByWeaponResponse response = await service.EnhanceByWeapon(request, FakeServerCallContext.For(UserId));

        EntityIUserWeapon targetWeapon = Assert.Single(userDb.EntityIUserWeapon);
        Assert.NotNull(response);
        Assert.False(response.IsGreatSuccess);
        Assert.Equal("target-weapon", targetWeapon.UserWeaponUuid);
        Assert.Equal(3, targetWeapon.Level);
        Assert.Equal(200, targetWeapon.Exp);
        Assert.Empty(userDb.EntityIUserWeaponSkill);
        Assert.Empty(userDb.EntityIUserWeaponAbility);
        Assert.Empty(userDb.EntityIUserWeaponAwaken);
        Assert.Equal(70, Assert.Single(userDb.EntityIUserConsumableItem, item => item.ConsumableItemId == GoldItemId).Count);
        Assert.Equal(7, Assert.Single(userDb.EntityIUserConsumableItem, item => item.ConsumableItemId == MedalItemId).Count);
        Assert.Equal(2, Assert.Single(userDb.EntityIUserWeaponStory).ReleasedMaxStoryIndex);
    }

    [Fact]
    public async Task EnhanceSkill_ClampsToConfiguredMaxLevelAndConsumesOneStepOfCost()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        ConfigureMainWeapon(
            masterDb,
            weaponSkillGroupId: WeaponSkillGroupId,
            maxSkillLevelFunctionId: MaxSkillLevelFunctionId,
            skillCostFunctionId: SkillCostFunctionId);
        AddLinearFunction(masterDb, MaxSkillLevelFunctionId, slope: 0, intercept: 3);
        AddLinearFunction(masterDb, SkillCostFunctionId, slope: 5, intercept: 0);
        masterDb.EntityMWeaponSkillGroup.Add(new EntityMWeaponSkillGroup
        {
            WeaponSkillGroupId = WeaponSkillGroupId,
            SlotNumber = 1,
            SkillId = SkillId,
            WeaponSkillEnhancementMaterialId = WeaponSkillMaterialGroupId
        });
        masterDb.EntityMWeaponSkillEnhancementMaterial.Add(new EntityMWeaponSkillEnhancementMaterial
        {
            WeaponSkillEnhancementMaterialId = WeaponSkillMaterialGroupId,
            SkillLevel = 2,
            MaterialId = SkillMaterialId,
            Count = 4
        });

        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserWeapon.Add(new EntityIUserWeapon
        {
            UserId = UserId,
            UserWeaponUuid = "target-weapon",
            WeaponId = TargetWeaponId,
            LimitBreakCount = 1
        });
        userDb.EntityIUserWeaponSkill.Add(new EntityIUserWeaponSkill
        {
            UserId = UserId,
            UserWeaponUuid = "target-weapon",
            SlotNumber = 1,
            Level = 2
        });
        userDb.EntityIUserConsumableItem.Add(new EntityIUserConsumableItem
        {
            UserId = UserId,
            ConsumableItemId = GoldItemId,
            Count = 50
        });
        userDb.EntityIUserMaterial.Add(new EntityIUserMaterial
        {
            UserId = UserId,
            MaterialId = SkillMaterialId,
            Count = 10
        });

        WeaponServiceImpl service = CreateService(masterDb, userDb);
        EnhanceSkillRequest request = new()
        {
            UserWeaponUuid = "target-weapon",
            SkillId = SkillId,
            AddLevelCount = 5
        };

        EnhanceSkillResponse response = await service.EnhanceSkill(request, FakeServerCallContext.For(UserId));

        Assert.NotNull(response);
        Assert.Equal(3, Assert.Single(userDb.EntityIUserWeaponSkill).Level);
        Assert.Equal(35, Assert.Single(userDb.EntityIUserConsumableItem).Count);
        Assert.Equal(6, Assert.Single(userDb.EntityIUserMaterial).Count);
    }

    [Fact]
    public async Task EnhanceAbility_ConsumesCostsForEachGrantedLevelUntilMax()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        ConfigureMainWeapon(
            masterDb,
            weaponAbilityGroupId: WeaponAbilityGroupId,
            maxAbilityLevelFunctionId: MaxAbilityLevelFunctionId,
            abilityCostFunctionId: AbilityCostFunctionId);
        AddLinearFunction(masterDb, MaxAbilityLevelFunctionId, slope: 0, intercept: 3);
        AddLinearFunction(masterDb, AbilityCostFunctionId, slope: 2, intercept: 1);
        masterDb.EntityMWeaponAbilityGroup.Add(new EntityMWeaponAbilityGroup
        {
            WeaponAbilityGroupId = WeaponAbilityGroupId,
            SlotNumber = 2,
            AbilityId = AbilityId,
            WeaponAbilityEnhancementMaterialId = WeaponAbilityMaterialGroupId
        });
        masterDb.EntityMWeaponAbilityEnhancementMaterial.AddRange(
        [
            new EntityMWeaponAbilityEnhancementMaterial
            {
                WeaponAbilityEnhancementMaterialId = WeaponAbilityMaterialGroupId,
                AbilityLevel = 1,
                MaterialId = AbilityMaterialId,
                Count = 2
            },
            new EntityMWeaponAbilityEnhancementMaterial
            {
                WeaponAbilityEnhancementMaterialId = WeaponAbilityMaterialGroupId,
                AbilityLevel = 2,
                MaterialId = AbilityMaterialId,
                Count = 1
            }
        ]);

        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserWeapon.Add(new EntityIUserWeapon
        {
            UserId = UserId,
            UserWeaponUuid = "target-weapon",
            WeaponId = TargetWeaponId,
            LimitBreakCount = 1
        });
        userDb.EntityIUserWeaponAbility.Add(new EntityIUserWeaponAbility
        {
            UserId = UserId,
            UserWeaponUuid = "target-weapon",
            SlotNumber = 2,
            Level = 1
        });
        userDb.EntityIUserConsumableItem.Add(new EntityIUserConsumableItem
        {
            UserId = UserId,
            ConsumableItemId = GoldItemId,
            Count = 40
        });
        userDb.EntityIUserMaterial.Add(new EntityIUserMaterial
        {
            UserId = UserId,
            MaterialId = AbilityMaterialId,
            Count = 10
        });

        WeaponServiceImpl service = CreateService(masterDb, userDb);
        EnhanceAbilityRequest request = new()
        {
            UserWeaponUuid = "target-weapon",
            AbilityId = AbilityId,
            AddLevelCount = 5
        };

        EnhanceAbilityResponse response = await service.EnhanceAbility(request, FakeServerCallContext.For(UserId));

        Assert.NotNull(response);
        Assert.Equal(3, Assert.Single(userDb.EntityIUserWeaponAbility).Level);
        Assert.Equal(28, Assert.Single(userDb.EntityIUserConsumableItem).Count);
        Assert.Equal(7, Assert.Single(userDb.EntityIUserMaterial).Count);
    }

    [Fact]
    public async Task LimitBreakByMaterial_CapsConsumptionAtConfiguredRemainingCountAndUpdatesNote()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        ConfigureMainWeapon(masterDb, limitBreakByMaterialCostFunctionId: LimitBreakByMaterialCostFunctionId);
        AddLinearFunction(masterDb, LimitBreakByMaterialCostFunctionId, slope: 20, intercept: 0);

        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserWeapon.Add(new EntityIUserWeapon
        {
            UserId = UserId,
            UserWeaponUuid = "target-weapon",
            WeaponId = TargetWeaponId,
            LimitBreakCount = 3
        });
        userDb.EntityIUserWeaponNote.Add(new EntityIUserWeaponNote
        {
            UserId = UserId,
            WeaponId = TargetWeaponId,
            MaxLimitBreakCount = 3
        });
        userDb.EntityIUserConsumableItem.Add(new EntityIUserConsumableItem
        {
            UserId = UserId,
            ConsumableItemId = GoldItemId,
            Count = 100
        });
        userDb.EntityIUserMaterial.Add(new EntityIUserMaterial
        {
            UserId = UserId,
            MaterialId = LimitBreakMaterialId,
            Count = 3
        });

        WeaponServiceImpl service = CreateService(masterDb, userDb);
        LimitBreakByMaterialRequest request = new() { UserWeaponUuid = "target-weapon" };
        request.Materials[LimitBreakMaterialId] = 3;

        LimitBreakByMaterialResponse response = await service.LimitBreakByMaterial(request, FakeServerCallContext.For(UserId));

        Assert.NotNull(response);
        Assert.Equal(4, Assert.Single(userDb.EntityIUserWeapon).LimitBreakCount);
        Assert.Equal(4, Assert.Single(userDb.EntityIUserWeaponNote).MaxLimitBreakCount);
        Assert.Equal(80, Assert.Single(userDb.EntityIUserConsumableItem).Count);
        Assert.Equal(2, Assert.Single(userDb.EntityIUserMaterial).Count);
    }

    [Fact]
    public async Task LimitBreakByWeapon_ConsumesOnlyRemainingFoddersAndCleansTheirRecords()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        ConfigureMainWeapon(masterDb, limitBreakByWeaponCostFunctionId: LimitBreakByWeaponCostFunctionId);
        ConfigureFodderWeapon(masterDb, weaponType: WeaponType.GUN, baseEnhancementObtainedExp: 0);
        AddLinearFunction(masterDb, LimitBreakByWeaponCostFunctionId, slope: 12, intercept: 0);
        masterDb.EntityMWeaponConsumeExchangeConsumableItemGroup.Add(new EntityMWeaponConsumeExchangeConsumableItemGroup
        {
            WeaponId = FodderWeaponId,
            ConsumableItemId = MedalItemId,
            Count = 2
        });

        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserWeapon.AddRange(
        [
            new EntityIUserWeapon
            {
                UserId = UserId,
                UserWeaponUuid = "target-weapon",
                WeaponId = TargetWeaponId,
                LimitBreakCount = 2
            },
            new EntityIUserWeapon
            {
                UserId = UserId,
                UserWeaponUuid = "fodder-1",
                WeaponId = FodderWeaponId
            },
            new EntityIUserWeapon
            {
                UserId = UserId,
                UserWeaponUuid = "fodder-2",
                WeaponId = FodderWeaponId
            },
            new EntityIUserWeapon
            {
                UserId = UserId,
                UserWeaponUuid = "fodder-3",
                WeaponId = FodderWeaponId
            }
        ]);
        userDb.EntityIUserWeaponSkill.AddRange(
        [
            new EntityIUserWeaponSkill
            {
                UserId = UserId,
                UserWeaponUuid = "fodder-1",
                SlotNumber = 1,
                Level = 1
            },
            new EntityIUserWeaponSkill
            {
                UserId = UserId,
                UserWeaponUuid = "fodder-2",
                SlotNumber = 1,
                Level = 1
            },
            new EntityIUserWeaponSkill
            {
                UserId = UserId,
                UserWeaponUuid = "fodder-3",
                SlotNumber = 1,
                Level = 1
            }
        ]);
        userDb.EntityIUserWeaponAbility.AddRange(
        [
            new EntityIUserWeaponAbility
            {
                UserId = UserId,
                UserWeaponUuid = "fodder-1",
                SlotNumber = 1,
                Level = 1
            },
            new EntityIUserWeaponAbility
            {
                UserId = UserId,
                UserWeaponUuid = "fodder-2",
                SlotNumber = 1,
                Level = 1
            },
            new EntityIUserWeaponAbility
            {
                UserId = UserId,
                UserWeaponUuid = "fodder-3",
                SlotNumber = 1,
                Level = 1
            }
        ]);
        userDb.EntityIUserWeaponAwaken.AddRange(
        [
            new EntityIUserWeaponAwaken
            {
                UserId = UserId,
                UserWeaponUuid = "fodder-1"
            },
            new EntityIUserWeaponAwaken
            {
                UserId = UserId,
                UserWeaponUuid = "fodder-2"
            },
            new EntityIUserWeaponAwaken
            {
                UserId = UserId,
                UserWeaponUuid = "fodder-3"
            }
        ]);
        userDb.EntityIUserWeaponNote.Add(new EntityIUserWeaponNote
        {
            UserId = UserId,
            WeaponId = TargetWeaponId,
            MaxLimitBreakCount = 2
        });
        userDb.EntityIUserConsumableItem.AddRange(
        [
            new EntityIUserConsumableItem
            {
                UserId = UserId,
                ConsumableItemId = GoldItemId,
                Count = 100
            },
            new EntityIUserConsumableItem
            {
                UserId = UserId,
                ConsumableItemId = MedalItemId,
                Count = 1
            }
        ]);

        WeaponServiceImpl service = CreateService(masterDb, userDb);
        LimitBreakByWeaponRequest request = new() { UserWeaponUuid = "target-weapon" };
        request.MaterialUserWeaponUuids.Add("fodder-1");
        request.MaterialUserWeaponUuids.Add("fodder-2");
        request.MaterialUserWeaponUuids.Add("fodder-3");

        LimitBreakByWeaponResponse response = await service.LimitBreakByWeapon(request, FakeServerCallContext.For(UserId));

        Assert.NotNull(response);
        Assert.Equal(4, Assert.Single(userDb.EntityIUserWeapon, weapon => weapon.UserWeaponUuid == "target-weapon").LimitBreakCount);
        Assert.Equal("fodder-3", Assert.Single(userDb.EntityIUserWeapon, weapon => weapon.UserWeaponUuid != "target-weapon").UserWeaponUuid);
        Assert.Equal("fodder-3", Assert.Single(userDb.EntityIUserWeaponSkill).UserWeaponUuid);
        Assert.Equal("fodder-3", Assert.Single(userDb.EntityIUserWeaponAbility).UserWeaponUuid);
        Assert.Equal("fodder-3", Assert.Single(userDb.EntityIUserWeaponAwaken).UserWeaponUuid);
        Assert.Equal(76, Assert.Single(userDb.EntityIUserConsumableItem, item => item.ConsumableItemId == GoldItemId).Count);
        Assert.Equal(5, Assert.Single(userDb.EntityIUserConsumableItem, item => item.ConsumableItemId == MedalItemId).Count);
        Assert.Equal(4, Assert.Single(userDb.EntityIUserWeaponNote).MaxLimitBreakCount);
    }

    [Fact]
    public async Task Evolve_ChangesWeaponIdRebuildsAbilitySlotsAndUnlocksEvolutionStory()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        ConfigureMainWeapon(
            masterDb,
            weaponAbilityGroupId: WeaponAbilityGroupId,
            weaponEvolutionMaterialGroupId: WeaponEvolutionMaterialGroupId,
            evolutionCostFunctionId: EvolutionCostFunctionId);
        ConfigureFodderWeapon(masterDb, weaponId: EvolvedWeaponId, weaponType: WeaponType.SWORD, enhanceId: FodderEnhanceId, weaponAbilityGroupId: WeaponAbilityGroupId + 1, weaponStoryReleaseConditionGroupId: EvolvedWeaponStoryGroupId);
        AddLinearFunction(masterDb, EvolutionCostFunctionId, slope: 30, intercept: 0);
        masterDb.EntityMWeaponAbilityGroup.AddRange(
        [
            new EntityMWeaponAbilityGroup
            {
                WeaponAbilityGroupId = WeaponAbilityGroupId,
                SlotNumber = 1,
                AbilityId = AbilityId,
                WeaponAbilityEnhancementMaterialId = WeaponAbilityMaterialGroupId
            },
            new EntityMWeaponAbilityGroup
            {
                WeaponAbilityGroupId = WeaponAbilityGroupId,
                SlotNumber = 2,
                AbilityId = AbilityId + 1,
                WeaponAbilityEnhancementMaterialId = WeaponAbilityMaterialGroupId
            },
            new EntityMWeaponAbilityGroup
            {
                WeaponAbilityGroupId = WeaponAbilityGroupId + 1,
                SlotNumber = 3,
                AbilityId = AbilityId + 2,
                WeaponAbilityEnhancementMaterialId = WeaponAbilityMaterialGroupId
            },
            new EntityMWeaponAbilityGroup
            {
                WeaponAbilityGroupId = WeaponAbilityGroupId + 1,
                SlotNumber = 4,
                AbilityId = AbilityId + 3,
                WeaponAbilityEnhancementMaterialId = WeaponAbilityMaterialGroupId
            }
        ]);
        masterDb.EntityMWeaponEvolutionGroup.AddRange(
        [
            new EntityMWeaponEvolutionGroup
            {
                WeaponEvolutionGroupId = WeaponEvolutionGroupId,
                EvolutionOrder = 0,
                WeaponId = TargetWeaponId
            },
            new EntityMWeaponEvolutionGroup
            {
                WeaponEvolutionGroupId = WeaponEvolutionGroupId,
                EvolutionOrder = 1,
                WeaponId = EvolvedWeaponId
            }
        ]);
        masterDb.EntityMWeaponEvolutionMaterialGroup.AddRange(
        [
            new EntityMWeaponEvolutionMaterialGroup
            {
                WeaponEvolutionMaterialGroupId = WeaponEvolutionMaterialGroupId,
                MaterialId = EvolutionMaterialAId,
                Count = 2,
                SortOrder = 1
            },
            new EntityMWeaponEvolutionMaterialGroup
            {
                WeaponEvolutionMaterialGroupId = WeaponEvolutionMaterialGroupId,
                MaterialId = EvolutionMaterialBId,
                Count = 1,
                SortOrder = 2
            }
        ]);
        AddStoryCondition(masterDb, EvolvedWeaponStoryGroupId, storyIndex: 5, WeaponStoryReleaseConditionType.REACH_SPECIFIED_EVOLUTION_COUNT, conditionValue: 1);

        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserWeapon.Add(new EntityIUserWeapon
        {
            UserId = UserId,
            UserWeaponUuid = "target-weapon",
            WeaponId = TargetWeaponId,
            Level = 2
        });
        userDb.EntityIUserWeaponAbility.AddRange(
        [
            new EntityIUserWeaponAbility
            {
                UserId = UserId,
                UserWeaponUuid = "target-weapon",
                SlotNumber = 1,
                Level = 4
            },
            new EntityIUserWeaponAbility
            {
                UserId = UserId,
                UserWeaponUuid = "target-weapon",
                SlotNumber = 2,
                Level = 5
            }
        ]);
        userDb.EntityIUserConsumableItem.Add(new EntityIUserConsumableItem
        {
            UserId = UserId,
            ConsumableItemId = GoldItemId,
            Count = 100
        });
        userDb.EntityIUserMaterial.AddRange(
        [
            new EntityIUserMaterial
            {
                UserId = UserId,
                MaterialId = EvolutionMaterialAId,
                Count = 2
            },
            new EntityIUserMaterial
            {
                UserId = UserId,
                MaterialId = EvolutionMaterialBId,
                Count = 2
            }
        ]);

        WeaponServiceImpl service = CreateService(masterDb, userDb);
        EvolveRequest request = new() { UserWeaponUuid = "target-weapon" };

        EvolveResponse response = await service.Evolve(request, FakeServerCallContext.For(UserId));

        EntityIUserWeapon weapon = Assert.Single(userDb.EntityIUserWeapon);
        Assert.NotNull(response);
        Assert.Equal(EvolvedWeaponId, weapon.WeaponId);
        Assert.Equal(10, Assert.Single(userDb.EntityIUserConsumableItem).Count);
        Assert.Equal(0, Assert.Single(userDb.EntityIUserMaterial, material => material.MaterialId == EvolutionMaterialAId).Count);
        Assert.Equal(1, Assert.Single(userDb.EntityIUserMaterial, material => material.MaterialId == EvolutionMaterialBId).Count);
        Assert.Equal(
            [3, 4],
            userDb.EntityIUserWeaponAbility
                .OrderBy(ability => ability.SlotNumber)
                .Select(ability => ability.SlotNumber));
        Assert.All(userDb.EntityIUserWeaponAbility, ability => Assert.Equal(1, ability.Level));
        Assert.Equal(5, Assert.Single(userDb.EntityIUserWeaponStory).ReleasedMaxStoryIndex);
    }

    [Fact]
    public async Task Awaken_ConsumesConfiguredResourcesAndSecondCallDoesNothing()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        ConfigureMainWeapon(masterDb);
        masterDb.EntityMWeaponAwaken.Add(new EntityMWeaponAwaken
        {
            WeaponId = TargetWeaponId,
            WeaponAwakenMaterialGroupId = WeaponAwakenMaterialGroupId,
            ConsumeGold = 25
        });
        masterDb.EntityMWeaponAwakenMaterialGroup.AddRange(
        [
            new EntityMWeaponAwakenMaterialGroup
            {
                WeaponAwakenMaterialGroupId = WeaponAwakenMaterialGroupId,
                MaterialId = AwakenMaterialAId,
                Count = 2,
                SortOrder = 1
            },
            new EntityMWeaponAwakenMaterialGroup
            {
                WeaponAwakenMaterialGroupId = WeaponAwakenMaterialGroupId,
                MaterialId = AwakenMaterialBId,
                Count = 5,
                SortOrder = 2
            }
        ]);

        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserWeapon.Add(new EntityIUserWeapon
        {
            UserId = UserId,
            UserWeaponUuid = "target-weapon",
            WeaponId = TargetWeaponId
        });
        userDb.EntityIUserConsumableItem.Add(new EntityIUserConsumableItem
        {
            UserId = UserId,
            ConsumableItemId = GoldItemId,
            Count = 100
        });
        userDb.EntityIUserMaterial.AddRange(
        [
            new EntityIUserMaterial
            {
                UserId = UserId,
                MaterialId = AwakenMaterialAId,
                Count = 2
            },
            new EntityIUserMaterial
            {
                UserId = UserId,
                MaterialId = AwakenMaterialBId,
                Count = 1
            }
        ]);

        WeaponServiceImpl service = CreateService(masterDb, userDb);
        AwakenRequest request = new() { UserWeaponUuid = "target-weapon" };

        AwakenResponse firstResponse = await service.Awaken(request, FakeServerCallContext.For(UserId));
        AwakenResponse secondResponse = await service.Awaken(request, FakeServerCallContext.For(UserId));

        Assert.NotNull(firstResponse);
        Assert.NotNull(secondResponse);
        Assert.Single(userDb.EntityIUserWeaponAwaken);
        Assert.Equal("target-weapon", userDb.EntityIUserWeaponAwaken.Single().UserWeaponUuid);
        Assert.Equal(75, Assert.Single(userDb.EntityIUserConsumableItem).Count);
        Assert.Equal(0, Assert.Single(userDb.EntityIUserMaterial, material => material.MaterialId == AwakenMaterialAId).Count);
        Assert.Equal(0, Assert.Single(userDb.EntityIUserMaterial, material => material.MaterialId == AwakenMaterialBId).Count);
    }

    private static WeaponServiceImpl CreateService(
        DarkMasterMemoryDatabase masterDb,
        DarkUserMemoryDatabase userDb,
        int weaponLimitBreakAvailableCount = 4)
    {
        UserDataStore store = new(TimeProvider.System);
        store.Set(UserId, userDb);
        return new WeaponServiceImpl(
            masterDb,
            store,
            new GameConfig
            {
                ConsumableItemIdForGold = GoldItemId,
                MaterialSameWeaponExpCoefficientPermil = 1500,
                WeaponLimitBreakAvailableCount = weaponLimitBreakAvailableCount
            }, TimeProvider.System);
    }

    private static DarkMasterMemoryDatabase CreateMasterDb() => new();

    private static DarkUserMemoryDatabase CreateUserDb() => new() { UserId = UserId };

    private static void ConfigureMainWeapon(
        DarkMasterMemoryDatabase masterDb,
        int sellPriceFunctionId = 0,
        WeaponType weaponType = WeaponType.SWORD,
        int maxLevelFunctionId = 0,
        int maxSkillLevelFunctionId = 0,
        int maxAbilityLevelFunctionId = 0,
        int requiredExpMapId = 0,
        int enhancementCostByWeaponFunctionId = 0,
        int enhancementCostByMaterialFunctionId = 0,
        int skillCostFunctionId = 0,
        int abilityCostFunctionId = 0,
        int limitBreakByWeaponCostFunctionId = 0,
        int limitBreakByMaterialCostFunctionId = 0,
        int evolutionCostFunctionId = 0,
        int weaponSkillGroupId = 0,
        int weaponAbilityGroupId = 0,
        int weaponEvolutionMaterialGroupId = 0,
        int weaponStoryReleaseConditionGroupId = 0)
    {
        ConfigureWeapon(
            masterDb,
            weaponId: TargetWeaponId,
            enhanceId: MainEnhanceId,
            weaponType: weaponType,
            sellPriceFunctionId: sellPriceFunctionId,
            maxLevelFunctionId: maxLevelFunctionId,
            maxSkillLevelFunctionId: maxSkillLevelFunctionId,
            maxAbilityLevelFunctionId: maxAbilityLevelFunctionId,
            requiredExpMapId: requiredExpMapId,
            enhancementCostByWeaponFunctionId: enhancementCostByWeaponFunctionId,
            enhancementCostByMaterialFunctionId: enhancementCostByMaterialFunctionId,
            skillCostFunctionId: skillCostFunctionId,
            abilityCostFunctionId: abilityCostFunctionId,
            limitBreakByWeaponCostFunctionId: limitBreakByWeaponCostFunctionId,
            limitBreakByMaterialCostFunctionId: limitBreakByMaterialCostFunctionId,
            evolutionCostFunctionId: evolutionCostFunctionId,
            weaponSkillGroupId: weaponSkillGroupId,
            weaponAbilityGroupId: weaponAbilityGroupId,
            weaponEvolutionMaterialGroupId: weaponEvolutionMaterialGroupId,
            weaponStoryReleaseConditionGroupId: weaponStoryReleaseConditionGroupId);
    }

    private static void ConfigureFodderWeapon(
        DarkMasterMemoryDatabase masterDb,
        WeaponType weaponType,
        int baseEnhancementObtainedExp = 0,
        int weaponId = FodderWeaponId,
        int enhanceId = FodderEnhanceId,
        int weaponAbilityGroupId = 0,
        int weaponStoryReleaseConditionGroupId = 0)
    {
        ConfigureWeapon(
            masterDb,
            weaponId: weaponId,
            enhanceId: enhanceId,
            weaponType: weaponType,
            baseEnhancementObtainedExp: baseEnhancementObtainedExp,
            weaponAbilityGroupId: weaponAbilityGroupId,
            weaponStoryReleaseConditionGroupId: weaponStoryReleaseConditionGroupId);
    }

    private static void ConfigureWeapon(
        DarkMasterMemoryDatabase masterDb,
        int weaponId,
        int enhanceId,
        WeaponType weaponType = WeaponType.SWORD,
        int baseEnhancementObtainedExp = 0,
        int sellPriceFunctionId = 0,
        int maxLevelFunctionId = 0,
        int maxSkillLevelFunctionId = 0,
        int maxAbilityLevelFunctionId = 0,
        int requiredExpMapId = 0,
        int enhancementCostByWeaponFunctionId = 0,
        int enhancementCostByMaterialFunctionId = 0,
        int skillCostFunctionId = 0,
        int abilityCostFunctionId = 0,
        int limitBreakByWeaponCostFunctionId = 0,
        int limitBreakByMaterialCostFunctionId = 0,
        int evolutionCostFunctionId = 0,
        int weaponSkillGroupId = 0,
        int weaponAbilityGroupId = 0,
        int weaponEvolutionMaterialGroupId = 0,
        int weaponStoryReleaseConditionGroupId = 0)
    {
        masterDb.EntityMWeapon.Add(new EntityMWeapon
        {
            WeaponId = weaponId,
            WeaponType = weaponType,
            WeaponSpecificEnhanceId = enhanceId,
            WeaponSkillGroupId = weaponSkillGroupId,
            WeaponAbilityGroupId = weaponAbilityGroupId,
            WeaponEvolutionMaterialGroupId = weaponEvolutionMaterialGroupId,
            WeaponStoryReleaseConditionGroupId = weaponStoryReleaseConditionGroupId
        });
        masterDb.EntityMWeaponSpecificEnhance.Add(new EntityMWeaponSpecificEnhance
        {
            WeaponSpecificEnhanceId = enhanceId,
            BaseEnhancementObtainedExp = baseEnhancementObtainedExp,
            SellPriceNumericalFunctionId = sellPriceFunctionId,
            MaxLevelNumericalFunctionId = maxLevelFunctionId,
            MaxSkillLevelNumericalFunctionId = maxSkillLevelFunctionId,
            MaxAbilityLevelNumericalFunctionId = maxAbilityLevelFunctionId,
            RequiredExpForLevelUpNumericalParameterMapId = requiredExpMapId,
            EnhancementCostByWeaponNumericalFunctionId = enhancementCostByWeaponFunctionId,
            EnhancementCostByMaterialNumericalFunctionId = enhancementCostByMaterialFunctionId,
            SkillEnhancementCostNumericalFunctionId = skillCostFunctionId,
            AbilityEnhancementCostNumericalFunctionId = abilityCostFunctionId,
            LimitBreakCostByWeaponNumericalFunctionId = limitBreakByWeaponCostFunctionId,
            LimitBreakCostByMaterialNumericalFunctionId = limitBreakByMaterialCostFunctionId,
            EvolutionCostNumericalFunctionId = evolutionCostFunctionId
        });
    }

    private static void AddMaterial(
        DarkMasterMemoryDatabase masterDb,
        int materialId,
        int effectValue = 0,
        WeaponType weaponType = WeaponType.UNKNOWN)
    {
        masterDb.EntityMMaterial.Add(new EntityMMaterial
        {
            MaterialId = materialId,
            WeaponType = weaponType,
            EffectValue = effectValue,
            AssetName = string.Empty
        });
    }

    private static void AddStoryCondition(
        DarkMasterMemoryDatabase masterDb,
        int groupId,
        int storyIndex,
        WeaponStoryReleaseConditionType conditionType,
        int conditionValue)
    {
        masterDb.EntityMWeaponStoryReleaseConditionGroup.Add(new EntityMWeaponStoryReleaseConditionGroup
        {
            WeaponStoryReleaseConditionGroupId = groupId,
            StoryIndex = storyIndex,
            WeaponStoryReleaseConditionType = conditionType,
            ConditionValue = conditionValue
        });
    }

    private static void AddLevelThreshold(DarkMasterMemoryDatabase masterDb, int mapId, int level, int requiredExp)
    {
        masterDb.EntityMNumericalParameterMap.Add(new EntityMNumericalParameterMap
        {
            NumericalParameterMapId = mapId,
            ParameterKey = level,
            ParameterValue = requiredExp
        });
    }

    private static void AddLinearFunction(DarkMasterMemoryDatabase masterDb, int functionId, int slope, int intercept)
    {
        masterDb.EntityMNumericalFunction.Add(new EntityMNumericalFunction
        {
            NumericalFunctionId = functionId,
            NumericalFunctionType = NumericalFunctionType.LINEAR,
            NumericalFunctionParameterGroupId = functionId
        });
        masterDb.EntityMNumericalFunctionParameterGroup.AddRange(
        [
            new EntityMNumericalFunctionParameterGroup
            {
                NumericalFunctionParameterGroupId = functionId,
                ParameterIndex = 0,
                ParameterValue = slope
            },
            new EntityMNumericalFunctionParameterGroup
            {
                NumericalFunctionParameterGroupId = functionId,
                ParameterIndex = 1,
                ParameterValue = intercept
            }
        ]);
    }
}
