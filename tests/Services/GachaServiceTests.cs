using Google.Protobuf.WellKnownTypes;
using MariesWonderland.Data;
using MariesWonderland.Missions;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Gacha;
using MariesWonderland.Tests.Infrastructure;
using GachaServiceImpl = MariesWonderland.Services.GachaService;

namespace MariesWonderland.Tests.Services;

public class GachaServiceTests
{
    private const long UserId = 1L;
    private const int StepUpGroupId = 562;
    private const int StepUpFirstBannerId = 562001;
    private const int StepUpSecondBannerId = 562002;
    private const int PremiumBannerId = 1001;
    private const int ChapterBannerId = 200005;
    private const int MedalConsumableItemId = 9001;
    private const int ChapterMaterialId = 701;
    private const int RewardMaterialId = 801;

    [Fact]
    public async Task GetGachaList_FiltersCatalogAndReflectsSavedStepUpState()
    {
        DarkMasterMemoryDatabase masterDb = new();
        AddMedal(masterDb, PremiumBannerId, gachaMedalId: 301);
        AddMedal(masterDb, StepUpFirstBannerId, gachaMedalId: 302);
        AddMedal(masterDb, StepUpSecondBannerId, gachaMedalId: 303);

        AddBanner(masterDb, "limited_banner", PremiumBannerId, sortOrder: 20);
        AddBanner(masterDb, "step_up_first", StepUpFirstBannerId, sortOrder: 10);
        AddBanner(masterDb, "step_up_second", StepUpSecondBannerId, sortOrder: 9);
        AddBanner(masterDb, "common_chapter_5", ChapterBannerId, sortOrder: 30);
        AddBanner(masterDb, "premium_missing_medal", 1111, sortOrder: 40);
        AddBanner(masterDb, "shop_banner", 2222, sortOrder: 50, domainType: DomainType.SHOP);

        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntitySGachaBannerState.Add(new EntitySGachaBannerState
        {
            UserId = UserId,
            GachaId = StepUpGroupId,
            StepNumber = 2,
            LoopCount = 3
        });

        GachaServiceImpl service = CreateService(masterDb, userDb);

        GetGachaListResponse response = await service.GetGachaList(
            new GetGachaListRequest(),
            FakeServerCallContext.For(UserId));

        Assert.Equal(3, response.Gacha.Count);

        Gacha stepUp = Assert.Single(response.Gacha, gacha => gacha.GachaId == StepUpGroupId);
        Assert.Equal((int)GachaModeType.STEPUP, stepUp.GachaModeType);
        Assert.Equal(2, stepUp.GachaPricePhase.Count);
        Assert.False(stepUp.GachaPricePhase.Single(phase => phase.GachaPricePhaseId == StepUpGroupId * 10 + 1).IsEnabled);
        Assert.True(stepUp.GachaPricePhase.Single(phase => phase.GachaPricePhaseId == StepUpGroupId * 10 + 2).IsEnabled);

        GachaModeStepupComposition stepUpMode = GachaModeStepupComposition.Parser.ParseFrom(stepUp.GachaMode);
        Assert.Equal(2, stepUpMode.CurrentStepNumber);
        Assert.Equal(3, stepUpMode.CurrentLoopCount);

        Gacha premium = Assert.Single(response.Gacha, gacha => gacha.GachaId == PremiumBannerId);
        Assert.Equal((int)GachaDecorationType.FESTIVAL, premium.GachaDecorationType);
        Assert.Equal(3, premium.GachaPricePhase.Count);

        Gacha chapter = Assert.Single(response.Gacha, gacha => gacha.GachaId == ChapterBannerId);
        Assert.Equal((int)GachaLabelType.CHAPTER, chapter.GachaLabelType);
        Assert.Equal((int)GachaModeType.BOX, chapter.GachaModeType);
        Assert.Equal(5, chapter.RelatedMainQuestChapterId);
        Assert.All(chapter.GachaPricePhase, phase =>
        {
            Assert.Equal((int)PriceType.CONSUMABLE_ITEM, phase.PriceType);
            Assert.Equal(2, phase.PriceId);
        });

        GetGachaListResponse chapterOnlyResponse = await service.GetGachaList(
            new GetGachaListRequest { GachaLabelType = { (int)GachaLabelType.CHAPTER } },
            FakeServerCallContext.For(UserId));

        Gacha onlyChapter = Assert.Single(chapterOnlyResponse.Gacha);
        Assert.Equal(ChapterBannerId, onlyChapter.GachaId);
    }

    [Fact]
    public async Task Draw_StepUpBanner_ConsumesPaidGemsAndLoopsAfterFinalStep()
    {
        DarkMasterMemoryDatabase masterDb = new();
        AddMedal(masterDb, StepUpFirstBannerId, gachaMedalId: 401);
        AddMedal(masterDb, StepUpSecondBannerId, gachaMedalId: 402);
        AddBanner(masterDb, "step_up_first", StepUpFirstBannerId, sortOrder: 10);
        AddBanner(masterDb, "step_up_second", StepUpSecondBannerId, sortOrder: 9);

        AddWeaponPoolItem(masterDb, 201, RarityType.RARE);
        AddWeaponPoolItem(masterDb, 202, RarityType.S_RARE);
        AddWeaponPoolItem(masterDb, 203, RarityType.SS_RARE);

        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserGem.Add(new EntityIUserGem
        {
            UserId = UserId,
            PaidGem = 5000,
            FreeGem = 100
        });

        GachaServiceImpl service = CreateService(masterDb, userDb);

        DrawResponse firstResponse = await service.Draw(
            new DrawRequest
            {
                GachaId = StepUpGroupId,
                GachaPricePhaseId = StepUpGroupId * 10 + 1
            },
            FakeServerCallContext.For(UserId));

        Assert.Equal(10, firstResponse.GachaResult.Count);
        Assert.Equal(10, userDb.EntityIUserWeapon.Count);
        Assert.Equal(3000, Assert.Single(userDb.EntityIUserGem).PaidGem);
        Assert.Equal(100, Assert.Single(userDb.EntityIUserGem).FreeGem);
        Assert.Equal(10, Assert.Single(userDb.EntityIUserConsumableItem, item => item.ConsumableItemId == MedalConsumableItemId).Count);

        EntitySGachaBannerState bannerState = Assert.Single(userDb.EntitySGachaBannerState);
        Assert.Equal(StepUpGroupId, bannerState.GachaId);
        Assert.Equal(2, bannerState.StepNumber);
        Assert.Equal(0, bannerState.LoopCount);
        Assert.Equal(10, bannerState.DrawCount);
        Assert.Equal(10, bannerState.MedalCount);

        Assert.All(firstResponse.GachaResult, result =>
        {
            Assert.Equal((int)PossessionType.WEAPON, result.GachaItem.PossessionType);
            Assert.True(new[] { 201, 202, 203 }.Contains(result.GachaItem.PossessionId));
            Assert.Equal(MedalConsumableItemId, result.MedalBonus.PossessionId);
        });

        GachaModeStepupComposition firstNextMode = GachaModeStepupComposition.Parser.ParseFrom(firstResponse.NextGacha.GachaMode);
        Assert.Equal(2, firstNextMode.CurrentStepNumber);
        Assert.True(firstResponse.NextGacha.GachaPricePhase.Single(phase => phase.GachaPricePhaseId == StepUpGroupId * 10 + 2).IsEnabled);

        DrawResponse secondResponse = await service.Draw(
            new DrawRequest
            {
                GachaId = StepUpGroupId,
                GachaPricePhaseId = StepUpGroupId * 10 + 2
            },
            FakeServerCallContext.For(UserId));

        Assert.Equal(20, userDb.EntityIUserWeapon.Count);
        Assert.Equal(3000, Assert.Single(userDb.EntityIUserGem).PaidGem);
        Assert.Equal(20, Assert.Single(userDb.EntityIUserConsumableItem, item => item.ConsumableItemId == MedalConsumableItemId).Count);
        Assert.Equal(1, bannerState.StepNumber);
        Assert.Equal(1, bannerState.LoopCount);
        Assert.Equal(20, bannerState.DrawCount);
        Assert.Equal(20, bannerState.MedalCount);

        GachaModeStepupComposition secondNextMode = GachaModeStepupComposition.Parser.ParseFrom(secondResponse.NextGacha.GachaMode);
        Assert.Equal(1, secondNextMode.CurrentStepNumber);
    }

    [Fact]
    public async Task Draw_ChapterBanner_UsesTicketsAndResetBoxGachaAdvancesBoxNumber()
    {
        DarkMasterMemoryDatabase masterDb = new();
        AddBanner(masterDb, "common_chapter_5", ChapterBannerId, sortOrder: 10);
        masterDb.EntityMMaterial.Add(new EntityMMaterial
        {
            MaterialId = ChapterMaterialId,
            RarityType = RarityType.RARE,
            AssetName = "chapter_material"
        });

        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserConsumableItem.Add(new EntityIUserConsumableItem
        {
            UserId = UserId,
            ConsumableItemId = 2,
            Count = 11
        });

        GachaServiceImpl service = CreateService(masterDb, userDb);

        DrawResponse drawResponse = await service.Draw(
            new DrawRequest
            {
                GachaId = ChapterBannerId,
                GachaPricePhaseId = ChapterBannerId * 10 + 1
            },
            FakeServerCallContext.For(UserId));

        Assert.Single(drawResponse.GachaResult);
        Assert.Equal((int)PossessionType.MATERIAL, drawResponse.GachaResult[0].GachaItem.PossessionType);
        Assert.Equal(ChapterMaterialId, drawResponse.GachaResult[0].GachaItem.PossessionId);
        Assert.Equal(10, Assert.Single(userDb.EntityIUserConsumableItem).Count);
        Assert.Equal(1, Assert.Single(userDb.EntityIUserMaterial).Count);

        EntitySGachaBannerState bannerState = Assert.Single(userDb.EntitySGachaBannerState);
        Assert.Equal(1, bannerState.BoxNumber);
        Assert.Equal(1, bannerState.DrawCount);

        GachaModeBoxComposition firstBoxMode = GachaModeBoxComposition.Parser.ParseFrom(drawResponse.NextGacha.GachaMode);
        Assert.Equal(1, firstBoxMode.CurrentBoxNumber);

        bannerState.BoxDrewCounts[ChapterMaterialId] = 3;

        ResetBoxGachaResponse resetResponse = await service.ResetBoxGacha(
            new ResetBoxGachaRequest { GachaId = ChapterBannerId },
            FakeServerCallContext.For(UserId));

        Assert.Equal(2, bannerState.BoxNumber);
        Assert.Empty(bannerState.BoxDrewCounts);

        GachaModeBoxComposition resetBoxMode = GachaModeBoxComposition.Parser.ParseFrom(resetResponse.Gacha.GachaMode);
        Assert.Equal(2, resetBoxMode.CurrentBoxNumber);
    }

    [Fact]
    public async Task RewardDraw_ResetsExpiredDailyStateAndStopsAtConfiguredLimit()
    {
        DarkMasterMemoryDatabase masterDb = new();
        masterDb.EntityMMaterial.Add(new EntityMMaterial
        {
            MaterialId = RewardMaterialId,
            RarityType = RarityType.NORMAL,
            AssetName = "reward_material"
        });

        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntitySGachaRewardState.Add(new EntitySGachaRewardState
        {
            UserId = UserId,
            LastRewardDrawDate = DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeMilliseconds(),
            TodaysCurrentDrawCount = 3,
            DailyMaxCount = 3,
            RewardAvailable = false
        });

        GachaServiceImpl service = CreateService(
            masterDb,
            userDb,
            new GameConfig
            {
                RewardGachaDailyMaxCount = 1
            });

        GetRewardGachaResponse availabilityResponse = await service.GetRewardGacha(
            new Empty(),
            FakeServerCallContext.For(UserId));

        Assert.True(availabilityResponse.Available);
        Assert.Equal(0, availabilityResponse.TodaysCurrentDrawCount);
        Assert.Equal(1, availabilityResponse.DailyMaxCount);

        RewardDrawResponse firstDraw = await service.RewardDraw(
            new RewardDrawRequest(),
            FakeServerCallContext.For(UserId));

        Assert.Single(firstDraw.RewardGachaResult);
        Assert.Equal(RewardMaterialId, firstDraw.RewardGachaResult[0].PossessionId);
        Assert.Equal(1, Assert.Single(userDb.EntityIUserMaterial).Count);

        EntitySGachaRewardState rewardState = Assert.Single(userDb.EntitySGachaRewardState);
        Assert.Equal(1, rewardState.TodaysCurrentDrawCount);
        Assert.Equal(1, rewardState.DailyMaxCount);
        Assert.False(rewardState.RewardAvailable);

        GetRewardGachaResponse exhaustedAvailability = await service.GetRewardGacha(
            new Empty(),
            FakeServerCallContext.For(UserId));

        Assert.False(exhaustedAvailability.Available);
        Assert.Equal(1, exhaustedAvailability.TodaysCurrentDrawCount);

        RewardDrawResponse secondDraw = await service.RewardDraw(
            new RewardDrawRequest(),
            FakeServerCallContext.For(UserId));

        Assert.Empty(secondDraw.RewardGachaResult);
        Assert.Equal(1, rewardState.TodaysCurrentDrawCount);
        Assert.Equal(1, Assert.Single(userDb.EntityIUserMaterial).Count);
    }

    private static GachaServiceImpl CreateService(DarkMasterMemoryDatabase masterDb, DarkUserMemoryDatabase userDb, GameConfig? gameConfig = null)
    {
        UserDataStore store = new(TimeProvider.System);
        store.Set(UserId, userDb);
        return new GachaServiceImpl(masterDb, store, gameConfig ?? new GameConfig(), new MissionTracker(masterDb, new MissionCatalog(masterDb, MissionFilterTable.Empty)), TimeProvider.System);
    }

    private static DarkUserMemoryDatabase CreateUserDb() => new() { UserId = UserId };

    private static void AddBanner(
        DarkMasterMemoryDatabase masterDb,
        string assetName,
        int gachaId,
        int sortOrder,
        DomainType domainType = DomainType.GACHA)
    {
        masterDb.EntityMMomBanner.Add(new EntityMMomBanner
        {
            BannerAssetName = assetName,
            DestinationDomainId = gachaId,
            DestinationDomainType = domainType,
            SortOrderDesc = sortOrder
        });
    }

    private static void AddMedal(DarkMasterMemoryDatabase masterDb, int gachaId, int gachaMedalId)
    {
        masterDb.EntityMGachaMedal.Add(new EntityMGachaMedal
        {
            GachaMedalId = gachaMedalId,
            ShopTransitionGachaId = gachaId,
            ConsumableItemId = MedalConsumableItemId,
            ConversionRate = 1
        });
    }

    private static void AddWeaponPoolItem(DarkMasterMemoryDatabase masterDb, int weaponId, RarityType rarityType)
    {
        masterDb.EntityMWeapon.Add(new EntityMWeapon
        {
            WeaponId = weaponId,
            RarityType = rarityType
        });
        masterDb.EntityMCatalogWeapon.Add(new EntityMCatalogWeapon
        {
            WeaponId = weaponId,
            CatalogTermId = weaponId
        });
    }
}
