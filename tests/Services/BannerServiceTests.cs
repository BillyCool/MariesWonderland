using MariesWonderland.Data;
using MariesWonderland.Missions;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Banner;
using MariesWonderland.Summons;
using MariesWonderland.Tests.Infrastructure;
using BannerServiceImpl = MariesWonderland.Services.BannerService;

namespace MariesWonderland.Tests.Services;

public class BannerServiceTests
{
    private const long UserId = 1L;

    [Fact]
    public async Task GetMamaBanner_FiltersUnsupportedBannersAndListsMamasDreamAsEvent()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        AddMedal(masterDb, 1001);
        AddMedal(masterDb, 1002);
        AddMedal(masterDb, 1004);
        AddMedal(masterDb, 1005);

        AddBanner(masterDb, "limited_premium", 1001);
        AddBanner(masterDb, "another_limited", 1002);
        AddBanner(masterDb, "common_200003", 200003);
        AddBanner(masterDb, "missing_medal", 1003);
        AddBanner(masterDb, "portal_cage_hidden", 1004);
        AddBanner(masterDb, "recycle_hidden", 1005);
        AddBanner(masterDb, "shop_banner", 9999, DomainType.SHOP);

        BannerServiceImpl service = CreateService(masterDb, new BoxSummonTable([Summon(200003, BoxSummonKind.MAMAS_DREAM, "common_200003", 0)]));

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
            },
            banner =>
            {
                Assert.Equal("common_200003", banner.GachaAssetName);
                Assert.Equal(200003, banner.GachaId);
                Assert.Equal((int)GachaLabelType.EVENT, banner.GachaLabelType);
            });
        Assert.Null(response.LatestChapterGacha);
    }

    [Fact]
    public async Task GetMamaBanner_LatestChapterSummonIsTheFurthestChapterReached()
    {
        DarkMasterMemoryDatabase masterDb = CreateMasterDb();
        AddMainChapter(masterDb, chapterId: 2, questId: 20001);
        AddMainChapter(masterDb, chapterId: 3, questId: 30001);
        AddMainChapter(masterDb, chapterId: 4, questId: 40001);
        BoxSummonTable summons = new(
        [
            Summon(300001, BoxSummonKind.CHAPTER, "chapter_1", 2),
            Summon(300002, BoxSummonKind.CHAPTER, "chapter_2", 3),
            Summon(300003, BoxSummonKind.CHAPTER, "chapter_3", 4),
            Summon(300099, BoxSummonKind.CHAPTER, "chapter_ex", 0)
        ]);
        UserDataStore store = new(TimeProvider.System);
        DarkUserMemoryDatabase userDb = store.GetOrCreate(UserId);
        userDb.EntityIUserQuest.Add(new EntityIUserQuest { UserId = UserId, QuestId = 20001 });
        userDb.EntityIUserQuest.Add(new EntityIUserQuest { UserId = UserId, QuestId = 30001 });

        GetMamaBannerResponse response = await CreateService(masterDb, summons, store).GetMamaBanner(
            new GetMamaBannerRequest(),
            FakeServerCallContext.For(UserId));

        GachaBanner latestChapter = Assert.IsType<GachaBanner>(response.LatestChapterGacha);
        Assert.Equal("chapter_2", latestChapter.GachaAssetName);
        Assert.Equal(300002, latestChapter.GachaId);
        Assert.Equal((int)GachaLabelType.CHAPTER, latestChapter.GachaLabelType);
        Assert.Empty(response.TermLimitedGacha);
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

    private static BannerServiceImpl CreateService(DarkMasterMemoryDatabase masterDb, BoxSummonTable? summons = null, UserDataStore? store = null)
        => new(store ?? new UserDataStore(TimeProvider.System), masterDb, new MissionTracker(masterDb, new MissionCatalog(masterDb, MissionFilterTable.Empty)),
            summons ?? BoxSummonTable.Empty, TimeProvider.System);

    private static BoxSummon Summon(int gachaId, BoxSummonKind kind, string assetName, int mainQuestChapterId)
        => new(gachaId, kind, assetName, 0, mainQuestChapterId, new BoxSummonPool("pool", [], []));

    private static void AddMainChapter(DarkMasterMemoryDatabase masterDb, int chapterId, int questId)
    {
        masterDb.EntityMMainQuestChapter.Add(new EntityMMainQuestChapter { MainQuestChapterId = chapterId, MainQuestSequenceGroupId = chapterId });
        masterDb.EntityMMainQuestSequenceGroup.Add(new EntityMMainQuestSequenceGroup { MainQuestSequenceGroupId = chapterId, MainQuestSequenceId = chapterId });
        masterDb.EntityMMainQuestSequence.Add(new EntityMMainQuestSequence { MainQuestSequenceId = chapterId, QuestId = questId });
    }

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
