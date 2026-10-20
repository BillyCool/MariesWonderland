using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Costume;
using MariesWonderland.Tests.Infrastructure;
using CostumeServiceImpl = MariesWonderland.Services.CostumeService;

namespace MariesWonderland.Tests.Services;

public class CostumeServiceTests
{
    private const long UserId = 1L;
    private const int CostumeId = 1001;
    private const int GoldItemId = 900;
    private const int EnhanceMaterialId = 501;
    private const int OffWeaponEnhanceMaterialId = 502;
    private const int InsufficientEnhanceMaterialId = 503;
    private const int LimitBreakMaterialAId = 601;
    private const int LimitBreakMaterialBId = 602;
    private const int AwakenMaterialAId = 701;
    private const int AwakenMaterialBId = 702;
    private const int ActiveSkillMaterialTierOneId = 801;
    private const int ActiveSkillMaterialTierTwoId = 802;
    private const int ThoughtId = 901;

    [Fact]
    public async Task Enhance_AppliesSameWeaponBonusAndCapsLevelFromUsedMaterialsOnly()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        ConfigureCostume(masterDb, skillfulWeaponType: WeaponType.SWORD, rarityType: RarityType.RARE);
        ConfigureRarity(
            masterDb,
            enhancementCostFunctionId: 10,
            requiredExpMapId: 20,
            limitBreakCostFunctionId: 0,
            activeSkillMaxLevelFunctionId: 0,
            activeSkillEnhancementCostFunctionId: 0);
        AddLinearFunction(masterDb, functionId: 10, slope: 10, intercept: 0, parameterGroupId: 110);
        AddLevelThreshold(masterDb, mapId: 20, level: 1, requiredExp: 0);
        AddLevelThreshold(masterDb, mapId: 20, level: 2, requiredExp: 100);
        AddLevelThreshold(masterDb, mapId: 20, level: 3, requiredExp: 250);
        masterDb.EntityMMaterial.AddRange(
        [
            new EntityMMaterial
            {
                MaterialId = EnhanceMaterialId,
                MaterialType = MaterialType.COSTUME_ENHANCEMENT,
                WeaponType = WeaponType.SWORD,
                EffectValue = 100
            },
            new EntityMMaterial
            {
                MaterialId = OffWeaponEnhanceMaterialId,
                MaterialType = MaterialType.COSTUME_ENHANCEMENT,
                WeaponType = WeaponType.GUN,
                EffectValue = 50
            },
            new EntityMMaterial
            {
                MaterialId = InsufficientEnhanceMaterialId,
                MaterialType = MaterialType.COSTUME_ENHANCEMENT,
                WeaponType = WeaponType.SWORD,
                EffectValue = 999
            }
        ]);

        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserCostume.Add(new EntityIUserCostume
        {
            UserId = UserId,
            UserCostumeUuid = "costume-1",
            CostumeId = CostumeId,
            Level = 1,
            Exp = 10
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
                MaterialId = EnhanceMaterialId,
                Count = 2
            },
            new EntityIUserMaterial
            {
                UserId = UserId,
                MaterialId = OffWeaponEnhanceMaterialId,
                Count = 1
            },
            new EntityIUserMaterial
            {
                UserId = UserId,
                MaterialId = InsufficientEnhanceMaterialId,
                Count = 1
            }
        ]);

        CostumeServiceImpl service = CreateService(masterDb, userDb);
        EnhanceRequest request = new() { UserCostumeUuid = "costume-1" };
        request.Materials[EnhanceMaterialId] = 2;
        request.Materials[OffWeaponEnhanceMaterialId] = 1;
        request.Materials[InsufficientEnhanceMaterialId] = 2;
        request.Materials[9999] = 1;

        EnhanceResponse response = await service.Enhance(request, FakeServerCallContext.For(UserId));

        EntityIUserCostume costume = Assert.Single(userDb.EntityIUserCostume);
        Assert.NotNull(response);
        Assert.False(response.IsGreatSuccess);
        Assert.Equal(3, costume.Level);
        Assert.Equal(250, costume.Exp);
        Assert.Equal(70, Assert.Single(userDb.EntityIUserConsumableItem).Count);
        Assert.Equal(0, Assert.Single(userDb.EntityIUserMaterial, material => material.MaterialId == EnhanceMaterialId).Count);
        Assert.Equal(0, Assert.Single(userDb.EntityIUserMaterial, material => material.MaterialId == OffWeaponEnhanceMaterialId).Count);
        Assert.Equal(1, Assert.Single(userDb.EntityIUserMaterial, material => material.MaterialId == InsufficientEnhanceMaterialId).Count);
    }

    [Fact]
    public async Task LimitBreak_ConsumesAvailableMaterialsAndGoldThenIncrementsCount()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        ConfigureCostume(masterDb, skillfulWeaponType: WeaponType.SWORD, rarityType: RarityType.RARE);
        ConfigureRarity(
            masterDb,
            enhancementCostFunctionId: 0,
            requiredExpMapId: 0,
            limitBreakCostFunctionId: 11,
            activeSkillMaxLevelFunctionId: 0,
            activeSkillEnhancementCostFunctionId: 0);
        AddLinearFunction(masterDb, functionId: 11, slope: 10, intercept: 0, parameterGroupId: 111);

        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserCostume.Add(new EntityIUserCostume
        {
            UserId = UserId,
            UserCostumeUuid = "costume-1",
            CostumeId = CostumeId,
            LimitBreakCount = 1
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
                MaterialId = LimitBreakMaterialAId,
                Count = 3
            },
            new EntityIUserMaterial
            {
                UserId = UserId,
                MaterialId = LimitBreakMaterialBId,
                Count = 1
            }
        ]);

        CostumeServiceImpl service = CreateService(masterDb, userDb);
        LimitBreakRequest request = new() { UserCostumeUuid = "costume-1" };
        request.Materials[LimitBreakMaterialAId] = 2;
        request.Materials[LimitBreakMaterialBId] = 5;
        request.Materials[9999] = 4;

        LimitBreakResponse response = await service.LimitBreak(request, FakeServerCallContext.For(UserId));

        EntityIUserCostume costume = Assert.Single(userDb.EntityIUserCostume);
        Assert.NotNull(response);
        Assert.Equal(2, costume.LimitBreakCount);
        Assert.Equal(70, Assert.Single(userDb.EntityIUserConsumableItem).Count);
        Assert.Equal(1, Assert.Single(userDb.EntityIUserMaterial, material => material.MaterialId == LimitBreakMaterialAId).Count);
        Assert.Equal(0, Assert.Single(userDb.EntityIUserMaterial, material => material.MaterialId == LimitBreakMaterialBId).Count);
    }

    [Fact]
    public async Task LimitBreak_AtConfiguredCap_LeavesDataUntouched()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        ConfigureCostume(masterDb, skillfulWeaponType: WeaponType.SWORD, rarityType: RarityType.RARE);

        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserCostume.Add(new EntityIUserCostume
        {
            UserId = UserId,
            UserCostumeUuid = "costume-1",
            CostumeId = CostumeId,
            LimitBreakCount = 4
        });
        userDb.EntityIUserConsumableItem.Add(new EntityIUserConsumableItem
        {
            UserId = UserId,
            ConsumableItemId = GoldItemId,
            Count = 90
        });
        userDb.EntityIUserMaterial.Add(new EntityIUserMaterial
        {
            UserId = UserId,
            MaterialId = LimitBreakMaterialAId,
            Count = 2
        });

        CostumeServiceImpl service = CreateService(masterDb, userDb, costumeLimitBreakAvailableCount: 4);
        LimitBreakRequest request = new() { UserCostumeUuid = "costume-1" };
        request.Materials[LimitBreakMaterialAId] = 2;

        LimitBreakResponse response = await service.LimitBreak(request, FakeServerCallContext.For(UserId));

        Assert.NotNull(response);
        Assert.Equal(4, Assert.Single(userDb.EntityIUserCostume).LimitBreakCount);
        Assert.Equal(90, Assert.Single(userDb.EntityIUserConsumableItem).Count);
        Assert.Equal(2, Assert.Single(userDb.EntityIUserMaterial).Count);
    }

    [Fact]
    public async Task Awaken_AppliesStatusEffectsForNextStepAndUsesHighestEligiblePriceTier()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        ConfigureCostume(masterDb, skillfulWeaponType: WeaponType.SWORD, rarityType: RarityType.RARE);
        masterDb.EntityMCostumeAwaken.Add(new EntityMCostumeAwaken
        {
            CostumeId = CostumeId,
            CostumeAwakenEffectGroupId = 41,
            CostumeAwakenPriceGroupId = 51
        });
        masterDb.EntityMCostumeAwakenPriceGroup.AddRange(
        [
            new EntityMCostumeAwakenPriceGroup
            {
                CostumeAwakenPriceGroupId = 51,
                AwakenStepLowerLimit = 1,
                Gold = 100
            },
            new EntityMCostumeAwakenPriceGroup
            {
                CostumeAwakenPriceGroupId = 51,
                AwakenStepLowerLimit = 2,
                Gold = 200
            }
        ]);
        masterDb.EntityMCostumeAwakenEffectGroup.Add(new EntityMCostumeAwakenEffectGroup
        {
            CostumeAwakenEffectGroupId = 41,
            AwakenStep = 2,
            CostumeAwakenEffectType = CostumeAwakenEffectType.STATUS_UP,
            CostumeAwakenEffectId = 61
        });
        masterDb.EntityMCostumeAwakenStatusUpGroup.AddRange(
        [
            new EntityMCostumeAwakenStatusUpGroup
            {
                CostumeAwakenStatusUpGroupId = 61,
                SortOrder = 1,
                StatusKindType = StatusKindType.ATTACK,
                StatusCalculationType = StatusCalculationType.ADD,
                EffectValue = 15
            },
            new EntityMCostumeAwakenStatusUpGroup
            {
                CostumeAwakenStatusUpGroupId = 61,
                SortOrder = 2,
                StatusKindType = StatusKindType.HP,
                StatusCalculationType = StatusCalculationType.ADD,
                EffectValue = 20
            },
            new EntityMCostumeAwakenStatusUpGroup
            {
                CostumeAwakenStatusUpGroupId = 61,
                SortOrder = 3,
                StatusKindType = StatusKindType.AGILITY,
                StatusCalculationType = StatusCalculationType.MULTIPLY,
                EffectValue = 7
            }
        ]);

        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserCostume.Add(new EntityIUserCostume
        {
            UserId = UserId,
            UserCostumeUuid = "costume-1",
            CostumeId = CostumeId,
            AwakenCount = 1
        });
        userDb.EntityIUserConsumableItem.Add(new EntityIUserConsumableItem
        {
            UserId = UserId,
            ConsumableItemId = GoldItemId,
            Count = 300
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
        userDb.EntityIUserCostumeAwakenStatusUp.Add(new EntityIUserCostumeAwakenStatusUp
        {
            UserId = UserId,
            UserCostumeUuid = "costume-1",
            StatusCalculationType = StatusCalculationType.ADD,
            Attack = 5
        });

        CostumeServiceImpl service = CreateService(masterDb, userDb);
        AwakenRequest request = new() { UserCostumeUuid = "costume-1" };
        request.Materials[AwakenMaterialAId] = 5;
        request.Materials[AwakenMaterialBId] = 1;

        AwakenResponse response = await service.Awaken(request, FakeServerCallContext.For(UserId));

        Assert.NotNull(response);
        Assert.Equal(2, Assert.Single(userDb.EntityIUserCostume).AwakenCount);
        Assert.Equal(100, Assert.Single(userDb.EntityIUserConsumableItem).Count);
        Assert.Equal(0, Assert.Single(userDb.EntityIUserMaterial, material => material.MaterialId == AwakenMaterialAId).Count);
        Assert.Equal(0, Assert.Single(userDb.EntityIUserMaterial, material => material.MaterialId == AwakenMaterialBId).Count);

        EntityIUserCostumeAwakenStatusUp additive = Assert.Single(
            userDb.EntityIUserCostumeAwakenStatusUp,
            status => status.StatusCalculationType == StatusCalculationType.ADD);
        Assert.Equal(20, additive.Attack);
        Assert.Equal(20, additive.Hp);

        EntityIUserCostumeAwakenStatusUp multiplicative = Assert.Single(
            userDb.EntityIUserCostumeAwakenStatusUp,
            status => status.StatusCalculationType == StatusCalculationType.MULTIPLY);
        Assert.Equal(7, multiplicative.Agility);
    }

    [Fact]
    public async Task Awaken_ItemAcquireEffect_DoesNotDuplicateExistingThought()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        ConfigureCostume(masterDb, skillfulWeaponType: WeaponType.SWORD, rarityType: RarityType.RARE);
        masterDb.EntityMCostumeAwaken.Add(new EntityMCostumeAwaken
        {
            CostumeId = CostumeId,
            CostumeAwakenEffectGroupId = 42,
            CostumeAwakenPriceGroupId = 52
        });
        masterDb.EntityMCostumeAwakenPriceGroup.Add(new EntityMCostumeAwakenPriceGroup
        {
            CostumeAwakenPriceGroupId = 52,
            AwakenStepLowerLimit = 1,
            Gold = 50
        });
        masterDb.EntityMCostumeAwakenEffectGroup.Add(new EntityMCostumeAwakenEffectGroup
        {
            CostumeAwakenEffectGroupId = 42,
            AwakenStep = 1,
            CostumeAwakenEffectType = CostumeAwakenEffectType.ITEM_ACQUIRE,
            CostumeAwakenEffectId = 62
        });
        masterDb.EntityMCostumeAwakenItemAcquire.Add(new EntityMCostumeAwakenItemAcquire
        {
            CostumeAwakenItemAcquireId = 62,
            PossessionType = PossessionType.THOUGHT,
            PossessionId = ThoughtId,
            Count = 1
        });

        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserCostume.Add(new EntityIUserCostume
        {
            UserId = UserId,
            UserCostumeUuid = "costume-1",
            CostumeId = CostumeId
        });
        userDb.EntityIUserConsumableItem.Add(new EntityIUserConsumableItem
        {
            UserId = UserId,
            ConsumableItemId = GoldItemId,
            Count = 70
        });
        userDb.EntityIUserThought.Add(new EntityIUserThought
        {
            UserId = UserId,
            UserThoughtUuid = $"awaken-thought-{ThoughtId}",
            ThoughtId = ThoughtId,
            AcquisitionDatetime = 123
        });

        CostumeServiceImpl service = CreateService(masterDb, userDb);

        AwakenResponse response = await service.Awaken(
            new AwakenRequest { UserCostumeUuid = "costume-1" },
            FakeServerCallContext.For(UserId));

        Assert.NotNull(response);
        Assert.Equal(1, Assert.Single(userDb.EntityIUserCostume).AwakenCount);
        Assert.Equal(20, Assert.Single(userDb.EntityIUserConsumableItem).Count);
        EntityIUserThought thought = Assert.Single(userDb.EntityIUserThought);
        Assert.Equal($"awaken-thought-{ThoughtId}", thought.UserThoughtUuid);
        Assert.Equal(123, thought.AcquisitionDatetime);
    }

    [Fact]
    public async Task EnhanceActiveSkill_UsesUnlockedTierCapsToMaxAndCreatesSkillRecord()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        ConfigureCostume(masterDb, skillfulWeaponType: WeaponType.SWORD, rarityType: RarityType.RARE, activeSkillGroupId: 70);
        ConfigureRarity(
            masterDb,
            enhancementCostFunctionId: 0,
            requiredExpMapId: 0,
            limitBreakCostFunctionId: 0,
            activeSkillMaxLevelFunctionId: 12,
            activeSkillEnhancementCostFunctionId: 13);
        AddLinearFunction(masterDb, functionId: 12, slope: 0, intercept: 3, parameterGroupId: 112);
        AddLinearFunction(masterDb, functionId: 13, slope: 10, intercept: 0, parameterGroupId: 113);
        masterDb.EntityMCostumeActiveSkillGroup.AddRange(
        [
            new EntityMCostumeActiveSkillGroup
            {
                CostumeActiveSkillGroupId = 70,
                CostumeLimitBreakCountLowerLimit = 0,
                CostumeActiveSkillEnhancementMaterialId = 71
            },
            new EntityMCostumeActiveSkillGroup
            {
                CostumeActiveSkillGroupId = 70,
                CostumeLimitBreakCountLowerLimit = 2,
                CostumeActiveSkillEnhancementMaterialId = 72
            }
        ]);
        masterDb.EntityMCostumeActiveSkillEnhancementMaterial.AddRange(
        [
            new EntityMCostumeActiveSkillEnhancementMaterial
            {
                CostumeActiveSkillEnhancementMaterialId = 71,
                SkillLevel = 0,
                MaterialId = ActiveSkillMaterialTierOneId,
                Count = 9
            },
            new EntityMCostumeActiveSkillEnhancementMaterial
            {
                CostumeActiveSkillEnhancementMaterialId = 72,
                SkillLevel = 0,
                MaterialId = ActiveSkillMaterialTierTwoId,
                Count = 1
            },
            new EntityMCostumeActiveSkillEnhancementMaterial
            {
                CostumeActiveSkillEnhancementMaterialId = 72,
                SkillLevel = 1,
                MaterialId = ActiveSkillMaterialTierTwoId,
                Count = 2
            },
            new EntityMCostumeActiveSkillEnhancementMaterial
            {
                CostumeActiveSkillEnhancementMaterialId = 72,
                SkillLevel = 2,
                MaterialId = ActiveSkillMaterialTierTwoId,
                Count = 3
            }
        ]);

        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserCostume.Add(new EntityIUserCostume
        {
            UserId = UserId,
            UserCostumeUuid = "costume-1",
            CostumeId = CostumeId,
            LimitBreakCount = 2
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
                MaterialId = ActiveSkillMaterialTierOneId,
                Count = 20
            },
            new EntityIUserMaterial
            {
                UserId = UserId,
                MaterialId = ActiveSkillMaterialTierTwoId,
                Count = 10
            }
        ]);

        CostumeServiceImpl service = CreateService(masterDb, userDb);

        EnhanceActiveSkillResponse response = await service.EnhanceActiveSkill(
            new EnhanceActiveSkillRequest
            {
                UserCostumeUuid = "costume-1",
                AddLevelCount = 5
            },
            FakeServerCallContext.For(UserId));

        Assert.NotNull(response);
        EntityIUserCostumeActiveSkill skill = Assert.Single(userDb.EntityIUserCostumeActiveSkill);
        Assert.Equal(3, skill.Level);
        Assert.True(skill.AcquisitionDatetime > 0);
        Assert.Equal(40, Assert.Single(userDb.EntityIUserConsumableItem).Count);
        Assert.Equal(20, Assert.Single(userDb.EntityIUserMaterial, material => material.MaterialId == ActiveSkillMaterialTierOneId).Count);
        Assert.Equal(4, Assert.Single(userDb.EntityIUserMaterial, material => material.MaterialId == ActiveSkillMaterialTierTwoId).Count);
    }

    [Fact]
    public async Task EnhanceActiveSkill_WhenAlreadyAtMaxLevel_LeavesResourcesUntouched()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        ConfigureCostume(masterDb, skillfulWeaponType: WeaponType.SWORD, rarityType: RarityType.RARE, activeSkillGroupId: 70);
        ConfigureRarity(
            masterDb,
            enhancementCostFunctionId: 0,
            requiredExpMapId: 0,
            limitBreakCostFunctionId: 0,
            activeSkillMaxLevelFunctionId: 12,
            activeSkillEnhancementCostFunctionId: 13);
        AddLinearFunction(masterDb, functionId: 12, slope: 0, intercept: 3, parameterGroupId: 112);
        AddLinearFunction(masterDb, functionId: 13, slope: 10, intercept: 0, parameterGroupId: 113);
        masterDb.EntityMCostumeActiveSkillGroup.Add(new EntityMCostumeActiveSkillGroup
        {
            CostumeActiveSkillGroupId = 70,
            CostumeLimitBreakCountLowerLimit = 0,
            CostumeActiveSkillEnhancementMaterialId = 72
        });
        masterDb.EntityMCostumeActiveSkillEnhancementMaterial.Add(new EntityMCostumeActiveSkillEnhancementMaterial
        {
            CostumeActiveSkillEnhancementMaterialId = 72,
            SkillLevel = 3,
            MaterialId = ActiveSkillMaterialTierTwoId,
            Count = 9
        });

        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserCostume.Add(new EntityIUserCostume
        {
            UserId = UserId,
            UserCostumeUuid = "costume-1",
            CostumeId = CostumeId
        });
        userDb.EntityIUserConsumableItem.Add(new EntityIUserConsumableItem
        {
            UserId = UserId,
            ConsumableItemId = GoldItemId,
            Count = 80
        });
        userDb.EntityIUserMaterial.Add(new EntityIUserMaterial
        {
            UserId = UserId,
            MaterialId = ActiveSkillMaterialTierTwoId,
            Count = 7
        });
        userDb.EntityIUserCostumeActiveSkill.Add(new EntityIUserCostumeActiveSkill
        {
            UserId = UserId,
            UserCostumeUuid = "costume-1",
            Level = 3,
            AcquisitionDatetime = 123
        });

        CostumeServiceImpl service = CreateService(masterDb, userDb);

        EnhanceActiveSkillResponse response = await service.EnhanceActiveSkill(
            new EnhanceActiveSkillRequest
            {
                UserCostumeUuid = "costume-1",
                AddLevelCount = 1
            },
            FakeServerCallContext.For(UserId));

        Assert.NotNull(response);
        EntityIUserCostumeActiveSkill skill = Assert.Single(userDb.EntityIUserCostumeActiveSkill);
        Assert.Equal(3, skill.Level);
        Assert.Equal(123, skill.AcquisitionDatetime);
        Assert.Equal(80, Assert.Single(userDb.EntityIUserConsumableItem).Count);
        Assert.Equal(7, Assert.Single(userDb.EntityIUserMaterial).Count);
    }

    private static CostumeServiceImpl CreateService(
        DarkMasterMemoryDatabase masterDb,
        DarkUserMemoryDatabase userDb,
        int sameWeaponExpCoefficientPermil = 1500,
        int costumeLimitBreakAvailableCount = 4)
    {
        userDb.UserId = UserId;
        UserDataStore store = new(masterDb, TimeProvider.System);
        store.Set(UserId, userDb);
        return new CostumeServiceImpl(
            masterDb,
            store,
            new GameConfig
            {
                ConsumableItemIdForGold = GoldItemId,
                CostumeLimitBreakAvailableCount = costumeLimitBreakAvailableCount,
                MaterialSameWeaponExpCoefficientPermil = sameWeaponExpCoefficientPermil
            }, TimeProvider.System);
    }

    private static DarkMasterMemoryDatabase CreateMasterDb() => new();

    private static DarkUserMemoryDatabase CreateUserDb() => new() { UserId = UserId };

    private static void ConfigureCostume(
        DarkMasterMemoryDatabase masterDb,
        WeaponType skillfulWeaponType,
        RarityType rarityType,
        int activeSkillGroupId = 0)
    {
        masterDb.EntityMCostume.Add(new EntityMCostume
        {
            CostumeId = CostumeId,
            SkillfulWeaponType = skillfulWeaponType,
            RarityType = rarityType,
            CostumeActiveSkillGroupId = activeSkillGroupId
        });
    }

    private static void ConfigureRarity(
        DarkMasterMemoryDatabase masterDb,
        int enhancementCostFunctionId,
        int requiredExpMapId,
        int limitBreakCostFunctionId,
        int activeSkillMaxLevelFunctionId,
        int activeSkillEnhancementCostFunctionId)
    {
        masterDb.EntityMCostumeRarity.Add(new EntityMCostumeRarity
        {
            RarityType = RarityType.RARE,
            EnhancementCostByMaterialNumericalFunctionId = enhancementCostFunctionId,
            LimitBreakCostNumericalFunctionId = limitBreakCostFunctionId,
            RequiredExpForLevelUpNumericalParameterMapId = requiredExpMapId,
            ActiveSkillMaxLevelNumericalFunctionId = activeSkillMaxLevelFunctionId,
            ActiveSkillEnhancementCostNumericalFunctionId = activeSkillEnhancementCostFunctionId
        });
    }

    private static void AddLinearFunction(
        DarkMasterMemoryDatabase masterDb,
        int functionId,
        int slope,
        int intercept,
        int parameterGroupId)
    {
        masterDb.EntityMNumericalFunction.Add(new EntityMNumericalFunction
        {
            NumericalFunctionId = functionId,
            NumericalFunctionType = NumericalFunctionType.LINEAR,
            NumericalFunctionParameterGroupId = parameterGroupId
        });
        masterDb.EntityMNumericalFunctionParameterGroup.AddRange(
        [
            new EntityMNumericalFunctionParameterGroup
            {
                NumericalFunctionParameterGroupId = parameterGroupId,
                ParameterIndex = 0,
                ParameterValue = slope
            },
            new EntityMNumericalFunctionParameterGroup
            {
                NumericalFunctionParameterGroupId = parameterGroupId,
                ParameterIndex = 1,
                ParameterValue = intercept
            }
        ]);
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
}
