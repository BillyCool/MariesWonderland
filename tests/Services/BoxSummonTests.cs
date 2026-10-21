using MariesWonderland.Data;
using MariesWonderland.Helpers;
using MariesWonderland.Missions;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Gacha;
using MariesWonderland.Proto.Gift;
using MariesWonderland.Summons;
using MariesWonderland.Tests.Infrastructure;
using Microsoft.Extensions.Time.Testing;
using GachaServiceImpl = MariesWonderland.Services.GachaService;
using GiftServiceImpl = MariesWonderland.Services.GiftService;

namespace MariesWonderland.Tests.Services;

/// <summary>
/// Box summons: chapter summons (one ticket per draw, per-player featured prizes refilled monthly, listed once the
/// chapter is reached) and Mama's Dream (limited prizes shared by every player, gems sent to the gift box).
/// </summary>
public class BoxSummonTests(MasterDatabaseFixture fixture) : ServiceTestBase(fixture), IClassFixture<MasterDatabaseFixture>
{
    private const long UserId = 1L;
    private const int Chapter1Summon = 300001, Chapter1Ticket = 1008;
    private static readonly DateTimeOffset Now = new(2026, 10, 21, 12, 0, 0, TimeSpan.Zero);

    private static BoxSummonTable RealTable => BoxSummonTable.Load(BoxSummonTable.DefaultPath);

    private GachaServiceImpl CreateService(UserDataStore store, BoxSummonTable table, TimeProvider time)
        => new(MasterDb, store, GameConfig, new MissionTracker(MasterDb, new MissionCatalog(MasterDb, MissionFilterTable.Empty)), table, time);

    private static DarkUserMemoryDatabase UserWithTickets(int ticketItemId, int count)
    {
        DarkUserMemoryDatabase userDb = new() { UserId = UserId };
        userDb.EntityIUserConsumableItem.Add(new EntityIUserConsumableItem { UserId = UserId, ConsumableItemId = ticketItemId, Count = count });
        return userDb;
    }

    /// <summary>A pool whose limited prize (gems) all but always comes up while any is left.</summary>
    private static BoxSummonTable GemFirstTable(BoxSummonKind kind, int gachaId, int ticketItemId, int stock)
    {
        BoxSummonPool pool = new("test",
            [new BoxSummonPrize(PossessionType.FREE_GEM, 0, 500, Weight: 1_000_000_000, Stock: stock)],
            [new BoxSummonPrize(PossessionType.MATERIAL, 315001, 10, Weight: 1)]);
        return new BoxSummonTable([new BoxSummon(gachaId, kind, kind == BoxSummonKind.CHAPTER ? "chapter_1" : "common_200003", ticketItemId, kind == BoxSummonKind.CHAPTER ? 2 : 0, pool)]);
    }

    private static int Tickets(DarkUserMemoryDatabase userDb, int ticketItemId)
        => userDb.EntityIUserConsumableItem.Single(c => c.ConsumableItemId == ticketItemId).Count;

    [Fact]
    public void Table_PointsAtRealTicketsAndPrizes()
    {
        BoxSummonTable table = RealTable;
        HashSet<int> consumables = [.. MasterDb.EntityMConsumableItem.Select(c => c.ConsumableItemId)];
        HashSet<int> materials = [.. MasterDb.EntityMMaterial.Select(m => m.MaterialId)];
        HashSet<int> chapters = [.. MasterDb.EntityMMainQuestChapter.Select(c => c.MainQuestChapterId)];

        Assert.Equal(25, table.All.Count(s => s.Kind == BoxSummonKind.CHAPTER));
        Assert.Equal(3, table.All.Count(s => s.Kind == BoxSummonKind.MAMAS_DREAM));
        Assert.All(table.All, s => Assert.Contains(s.TicketItemId, consumables));
        Assert.All(table.All.Where(s => s.MainQuestChapterId != 0), s => Assert.Contains(s.MainQuestChapterId, chapters));
        Assert.All(table.All.SelectMany(s => s.Pool.Limited.Concat(s.Pool.Unlimited)), p =>
        {
            switch (p.PossessionType)
            {
                case PossessionType.MATERIAL: Assert.Contains(p.PossessionId, materials); break;
                case PossessionType.CONSUMABLE_ITEM: Assert.Contains(p.PossessionId, consumables); break;
                default: Assert.Equal(PossessionType.FREE_GEM, p.PossessionType); break;
            }
        });

        // Featured (limited) prizes are rarer than any regular one.
        Assert.All(table.All.Select(s => s.Pool), pool => Assert.True(pool.Limited.Max(p => p.Weight) < pool.Unlimited.Min(p => p.Weight), pool.PoolId));
    }

    [Fact]
    public async Task GachaList_ShowsAChapterSummonOnceTheChapterIsReached()
    {
        DarkUserMemoryDatabase userDb = UserWithTickets(Chapter1Ticket, 5);
        GachaServiceImpl service = CreateService(CreateStore(UserId, userDb), RealTable, new FakeTimeProvider(Now));

        GetGachaListResponse before = await service.GetGachaList(new GetGachaListRequest(), ContextFor(UserId));
        Assert.DoesNotContain(before.Gacha, g => g.GachaLabelType == (int)GachaLabelType.CHAPTER);
        Assert.Contains(before.Gacha, g => g.GachaId == 200003 && g.GachaLabelType == (int)GachaLabelType.EVENT);

        // Chapter 1 is main quest chapter 2; its first quest is quest 1 of that chapter's sequence.
        int chapterQuestId = MasterDb.EntityMMainQuestSequence.Where(s => MasterDb.EntityMMainQuestSequenceGroup
            .Any(g => g.MainQuestSequenceGroupId == MasterDb.EntityMMainQuestChapter.Single(c => c.MainQuestChapterId == 2).MainQuestSequenceGroupId && g.MainQuestSequenceId == s.MainQuestSequenceId))
            .OrderBy(s => s.SortOrder).First().QuestId;
        userDb.EntityIUserQuest.Add(new EntityIUserQuest { UserId = UserId, QuestId = chapterQuestId, QuestStateType = (int)QuestStateType.ACTIVE });

        GetGachaListResponse after = await service.GetGachaList(new GetGachaListRequest(), ContextFor(UserId));
        Proto.Gacha.Gacha chapter = Assert.Single(after.Gacha, g => g.GachaLabelType == (int)GachaLabelType.CHAPTER);
        Assert.Equal(Chapter1Summon, chapter.GachaId);
        Assert.Equal((int)GachaAutoResetType.MONTHLY, chapter.GachaAutoResetType);
        Assert.Equal(GameDayHelper.StartOfNextMonth(Now.ToUnixTimeMilliseconds()), chapter.NextAutoResetDatetime.ToDateTimeOffset().ToUnixTimeMilliseconds());
        Assert.Equal([1, 10, 100], chapter.GachaPricePhase.Select(p => p.Price));
        Assert.All(chapter.GachaPricePhase, p => Assert.Equal(Chapter1Ticket, p.PriceId));
    }

    [Fact]
    public async Task Draw_SpendsOneTicketPerPrize_CappedAtTheTicketsOwned()
    {
        DarkUserMemoryDatabase userDb = UserWithTickets(Chapter1Ticket, 15);
        GachaServiceImpl service = CreateService(CreateStore(UserId, userDb), RealTable, new FakeTimeProvider(Now));

        DrawResponse response = await service.Draw(new DrawRequest { GachaId = Chapter1Summon, GachaPricePhaseId = Chapter1Summon * 10 + 3, ExecCount = 1 }, ContextFor(UserId));

        Assert.Equal(15, response.GachaResult.Count);
        Assert.Equal(0, Tickets(userDb, Chapter1Ticket));
    }

    [Fact]
    public async Task ChapterSummon_FeaturedPrizesArePerPlayer_AndRefillOnTheFirst()
    {
        DarkUserMemoryDatabase userDb = UserWithTickets(Chapter1Ticket, 10);
        FakeTimeProvider time = new(Now);
        GachaServiceImpl service = CreateService(CreateStore(UserId, userDb), GemFirstTable(BoxSummonKind.CHAPTER, Chapter1Summon, Chapter1Ticket, stock: 2), time);
        DrawRequest three = new() { GachaId = Chapter1Summon, GachaPricePhaseId = Chapter1Summon * 10 + 1, ExecCount = 3 };

        DrawResponse first = await service.Draw(three, ContextFor(UserId));
        Assert.Equal([PossessionType.FREE_GEM, PossessionType.FREE_GEM, PossessionType.MATERIAL], first.GachaResult.Select(r => (PossessionType)r.GachaItem.PossessionType));
        Assert.Equal(2, userDb.EntitySUserGift.Count(g => g.ReceivedDatetime == 0));

        // Same month: the featured gems are gone. Next month (the 1st, 08:00 UTC) they're back.
        DrawResponse sameMonth = await service.Draw(three, ContextFor(UserId));
        Assert.All(sameMonth.GachaResult, r => Assert.Equal((int)PossessionType.MATERIAL, r.GachaItem.PossessionType));
        time.SetUtcNow(DateTimeOffset.FromUnixTimeMilliseconds(GameDayHelper.StartOfNextMonth(Now.ToUnixTimeMilliseconds())));
        DrawResponse nextMonth = await service.Draw(three, ContextFor(UserId));
        Assert.Equal((int)PossessionType.FREE_GEM, nextMonth.GachaResult[0].GachaItem.PossessionType);
    }

    [Fact]
    public async Task MamasDream_LimitedPrizesAreSharedByEveryPlayer()
    {
        const int Summon = 200003, Ticket = 6072;
        UserDataStore store = new(TimeProvider.System);
        DarkUserMemoryDatabase first = UserWithTickets(Ticket, 5);
        DarkUserMemoryDatabase second = UserWithTickets(Ticket, 5);
        second.UserId = 2;
        store.Set(1, first);
        store.Set(2, second);
        GachaServiceImpl service = CreateService(store, GemFirstTable(BoxSummonKind.MAMAS_DREAM, Summon, Ticket, stock: 2), new FakeTimeProvider(Now));
        DrawRequest one = new() { GachaId = Summon, GachaPricePhaseId = Summon * 10 + 1, ExecCount = 1 };

        Assert.Equal((int)PossessionType.FREE_GEM, (await service.Draw(one, ContextFor(1))).GachaResult[0].GachaItem.PossessionType);
        Assert.Equal((int)PossessionType.FREE_GEM, (await service.Draw(one, ContextFor(2))).GachaResult[0].GachaItem.PossessionType);
        Assert.Equal((int)PossessionType.MATERIAL, (await service.Draw(one, ContextFor(1))).GachaResult[0].GachaItem.PossessionType);
        Assert.Equal(2, store.SharedPrizeClaimed(Summon, 0));
    }

    [Fact]
    public async Task ReceiveGift_GrantsTheGiftsContents()
    {
        DarkUserMemoryDatabase userDb = new() { UserId = UserId };
        userDb.EntityIUserGem.Add(new EntityIUserGem { UserId = UserId });
        userDb.EntitySUserGift.Add(new EntitySUserGift { UserId = UserId, UserGiftUuid = "gift-1", PossessionType = (int)PossessionType.FREE_GEM, Count = 500 });
        GiftServiceImpl service = new(CreateStore(UserId, userDb), MasterDb, new FakeTimeProvider(Now));

        await service.ReceiveGift(new ReceiveGiftRequest { UserGiftUuid = { "gift-1" } }, ContextFor(UserId));

        Assert.Equal(500, userDb.EntityIUserGem.Sum(g => g.FreeGem));
        Assert.NotEqual(0, userDb.EntitySUserGift.Single().ReceivedDatetime);
    }
}
