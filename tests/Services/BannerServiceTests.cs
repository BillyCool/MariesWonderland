using MariesWonderland.Data;
using MariesWonderland.Missions;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Banner;
using MariesWonderland.Tests.Infrastructure;
using BannerServiceImpl = MariesWonderland.Services.BannerService;

namespace MariesWonderland.Tests.Services;

public class BannerServiceTests
{
    private const long UserId = 1L;

    [Fact]
    public async Task GetMamaBanner_FiltersUnsupportedBannersAndReturnsTermLimitedWithLatestChapter()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        AddMedal(masterDb, 1001);
        AddMedal(masterDb, 1002);
        AddMedal(masterDb, 1004);
        AddMedal(masterDb, 1005);

        AddBanner(masterDb, "limited_premium", 1001);
        AddBanner(masterDb, "another_limited", 1002);
        AddBanner(masterDb, "common_chapter_old", 2001);
        AddBanner(masterDb, "common_chapter_latest", 2005);
        AddBanner(masterDb, "missing_medal", 1003);
        AddBanner(masterDb, "portal_cage_hidden", 1004);
        AddBanner(masterDb, "recycle_hidden", 1005);
        AddBanner(masterDb, "shop_banner", 9999, DomainType.SHOP);

        BannerServiceImpl service = CreateService(masterDb);

        GetMamaBannerResponse response = await service.GetMamaBanner(
            new GetMamaBannerRequest(),
            FakeServerCallContext.For(UserId));

        Assert.False(response.IsExistUnreadPop);
        Assert.Collection(
            response.TermLimitedGacha,
            banner =>
            {
                Assert.Equal("limited_premium", banner.GachaAssetName);
                Assert.Equal(1001, banner.GachaId);
                Assert.Equal((int)GachaLabelType.PREMIUM, banner.GachaLabelType);
            },
            banner =>
            {
                Assert.Equal("another_limited", banner.GachaAssetName);
                Assert.Equal(1002, banner.GachaId);
                Assert.Equal((int)GachaLabelType.PREMIUM, banner.GachaLabelType);
            });

        GachaBanner latestChapter = Assert.IsType<GachaBanner>(response.LatestChapterGacha);
        Assert.Equal("common_chapter_latest", latestChapter.GachaAssetName);
        Assert.Equal(2005, latestChapter.GachaId);
        Assert.Equal((int)GachaLabelType.CHAPTER, latestChapter.GachaLabelType);
    }

    [Fact]
    public async Task GetMamaBanner_DeduplicatesStepUpBannersByGroupId()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        AddMedal(masterDb, 562001);
        AddMedal(masterDb, 562002);
        AddMedal(masterDb, 563001);

        AddBanner(masterDb, "step_up_first", 562001);
        AddBanner(masterDb, "step_up_duplicate", 562002);
        AddBanner(masterDb, "step_up_other_group", 563001);

        BannerServiceImpl service = CreateService(masterDb);

        GetMamaBannerResponse response = await service.GetMamaBanner(
            new GetMamaBannerRequest(),
            FakeServerCallContext.For(UserId));

        Assert.Null(response.LatestChapterGacha);
        Assert.Collection(
            response.TermLimitedGacha,
            banner =>
            {
                Assert.Equal("step_up_first", banner.GachaAssetName);
                Assert.Equal(562, banner.GachaId);
            },
            banner =>
            {
                Assert.Equal("step_up_other_group", banner.GachaAssetName);
                Assert.Equal(563, banner.GachaId);
            });
    }

    [Fact]
    public async Task GetMamaBanner_OnlyExcludedBanners_ReturnsEmptyResult()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        AddMedal(masterDb, 7001);
        AddMedal(masterDb, 7002);

        AddBanner(masterDb, "portal_cage_hidden", 7001);
        AddBanner(masterDb, "recycle_hidden", 7002);
        AddBanner(masterDb, "missing_medal", 7003);
        AddBanner(masterDb, "shop_banner", 7004, DomainType.SHOP);

        BannerServiceImpl service = CreateService(masterDb);

        GetMamaBannerResponse response = await service.GetMamaBanner(
            new GetMamaBannerRequest(),
            FakeServerCallContext.For(UserId));

        Assert.Empty(response.TermLimitedGacha);
        Assert.Null(response.LatestChapterGacha);
        Assert.False(response.IsExistUnreadPop);
    }

    private static BannerServiceImpl CreateService(DarkMasterMemoryDatabase masterDb)
        => new(new UserDataStore(TimeProvider.System), masterDb, new MissionTracker(masterDb, new MissionCatalog(masterDb, MissionFilterTable.Empty)), TimeProvider.System);

    private static DarkMasterMemoryDatabase CreateMasterDb() => new();

    private static void AddMedal(DarkMasterMemoryDatabase masterDb, int gachaId)
    {
        masterDb.EntityMGachaMedal.Add(new EntityMGachaMedal
        {
            ShopTransitionGachaId = gachaId
        });
    }

    private static void AddBanner(
        DarkMasterMemoryDatabase masterDb,
        string assetName,
        int gachaId,
        DomainType domainType = DomainType.GACHA)
    {
        masterDb.EntityMMomBanner.Add(new EntityMMomBanner
        {
            DestinationDomainType = domainType,
            DestinationDomainId = gachaId,
            BannerAssetName = assetName
        });
    }
}
