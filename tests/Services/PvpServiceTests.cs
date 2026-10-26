using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MariesWonderland.Arena;
using MariesWonderland.Configuration;
using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.ConsumableItem;
using MariesWonderland.Proto.Deck;
using MariesWonderland.Proto.Pvp;
using MariesWonderland.Proto.Reward;
using MariesWonderland.Proto.User;
using MariesWonderland.Tests.Infrastructure;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using ConsumableItemServiceImpl = MariesWonderland.Services.ConsumableItemService;
using DeckServiceImpl = MariesWonderland.Services.DeckService;
using PvpServiceImpl = MariesWonderland.Services.PvpService;
using RewardServiceImpl = MariesWonderland.Services.RewardService;
using UserServiceImpl = MariesWonderland.Services.UserService;

namespace MariesWonderland.Tests.Services;

/// <summary>The Arena end to end against the real master data: entering, opponents, battles with bots, BP and rewards.</summary>
public class PvpServiceTests(MasterDatabaseFixture fixture) : ServiceTestBase(fixture)
{
    private const long Alice = 1001;
    private const int CostumeId = 32011;
    private const int WeaponId = 340101;

    // Saturday 2026-10-24 12:00 UTC, inside season 202072 (2026-10-19 to 2026-11-16).
    private readonly FakeTimeProvider _time = new(new DateTimeOffset(2026, 10, 24, 12, 0, 0, TimeSpan.Zero));
    private readonly UserDataStore _store = new(TimeProvider.System);

    private long NowMs => _time.GetUtcNow().ToUnixTimeMilliseconds();

    private PvpServiceImpl Service() => new(_store, MasterDb, GameConfig, TestServices.Missions(MasterDb), _time);

    private int SeasonId => ArenaHelper.CurrentSeason(MasterDb, NowMs)!.PvpSeasonId;

    /// <summary>A player with a one-character Arena deck (number 1, defending), full BP and a low force.</summary>
    private DarkUserMemoryDatabase AddPlayer(long userId, string name)
    {
        DarkUserMemoryDatabase userDb = new() { UserId = userId };
        userDb.EntityIUser.Add(new EntityIUser { UserId = userId, PlayerId = userId * 10 });
        userDb.EntityIUserProfile.Add(new EntityIUserProfile { UserId = userId, Name = name, Message = "" });
        userDb.EntityIUserStatus.Add(new EntityIUserStatus { UserId = userId, Level = 50 });
        userDb.EntityIUserPvpStatus.Add(new EntityIUserPvpStatus { UserId = userId, StaminaMilliValue = 100_000, StaminaUpdateDatetime = NowMs });
        userDb.EntityIUserPvpDefenseDeck.Add(new EntityIUserPvpDefenseDeck { UserId = userId, UserDeckNumber = 1 });
        userDb.EntityIUserCostume.Add(new EntityIUserCostume { UserId = userId, UserCostumeUuid = "costume", CostumeId = CostumeId, Level = 10 });
        userDb.EntityIUserWeapon.Add(new EntityIUserWeapon { UserId = userId, UserWeaponUuid = "weapon", WeaponId = WeaponId, Level = 10 });
        userDb.EntityIUserDeckCharacter.Add(new EntityIUserDeckCharacter
        {
            UserId = userId, UserDeckCharacterUuid = "slot1", UserCostumeUuid = "costume", MainUserWeaponUuid = "weapon", Power = 3000,
        });
        userDb.EntityIUserDeck.Add(new EntityIUserDeck { UserId = userId, DeckType = DeckType.PVP, UserDeckNumber = 1, UserDeckCharacterUuid01 = "slot1", Power = 3000 });
        _store.Set(userId, userDb);
        return userDb;
    }

    private static int BattlePoints(DarkUserMemoryDatabase userDb) => userDb.EntityIUserPvpStatus[0].StaminaMilliValue / 1000;

    private async Task<(MatchingOpponent Opponent, FinishBattleResponse Finish)> FightAsync(bool win)
    {
        PvpServiceImpl service = Service();
        await service.GetTopData(new Empty(), ContextFor(Alice));
        MatchingOpponent opponent = (await service.GetMatchingList(new Empty(), ContextFor(Alice))).Matching[0];
        await service.StartBattle(new StartBattleRequest { OpponentPlayerId = opponent.PlayerId, UseDeckNumber = 1 }, ContextFor(Alice));
        FinishBattleResponse finish = await service.FinishBattle(new FinishBattleRequest { OpponentPlayerId = opponent.PlayerId, IsVictory = win }, ContextFor(Alice));
        return (opponent, finish);
    }

    [Fact]
    public async Task TopData_EntersTheRunningSeason_UnderTheBots()
    {
        AddPlayer(Alice, "Alice");

        GetTopDataResponse top = await Service().GetTopData(new Empty(), ContextFor(Alice));

        Assert.Equal(202072, top.CurrentSeasonId);
        Assert.Equal(18_000, top.PvpPoint);
        Assert.Equal(ArenaBots.Count + 1, top.Rank);
        Assert.Null(top.WeeklyGradeResult);
        Assert.Equal(ArenaBots.Count + 1, (await Service().GetRanking(new GetRankingRequest { RankFrom = 1 }, ContextFor(Alice))).UserCount);
    }

    [Fact]
    public async Task Matching_OffersThreeBotsInRange_AndANewListCostsBp()
    {
        DarkUserMemoryDatabase alice = AddPlayer(Alice, "Alice");
        PvpServiceImpl service = Service();
        await service.GetTopData(new Empty(), ContextFor(Alice));

        GetMatchingListResponse list = await service.GetMatchingList(new Empty(), ContextFor(Alice));
        Assert.Equal(ArenaStore.MatchingCount, list.Matching.Count);
        Assert.All(list.Matching, o =>
        {
            Assert.True(ArenaBots.IsBot(o.PlayerId));
            Assert.NotEmpty(o.Name);
            Assert.InRange(o.PvpPoint, 18_000, 18_000 + ArenaStore.MatchingRange);
            Assert.Equal(3, o.DeckMainWeaponAttributeType.Count);
            Assert.NotEqual(0, o.MostPowerfulCostumeId);
        });
        Assert.Equal(list.Matching.Select(o => o.PlayerId), (await service.GetMatchingList(new Empty(), ContextFor(Alice))).Matching.Select(o => o.PlayerId));

        await service.UpdateMatchingList(new Empty(), ContextFor(Alice));
        Assert.Equal(95, BattlePoints(alice));

        alice.EntityIUserPvpStatus[0].StaminaMilliValue = 4_000;
        RpcException refused = await Assert.ThrowsAsync<RpcException>(() => service.UpdateMatchingList(new Empty(), ContextFor(Alice)));
        Assert.Equal(StatusCode.FailedPrecondition, refused.StatusCode);
    }

    [Fact]
    public async Task BattleAgainstABot_HandsOverItsDeck_AndAWinMovesPointsBothWays()
    {
        DarkUserMemoryDatabase alice = AddPlayer(Alice, "Alice");
        PvpServiceImpl service = Service();
        await service.GetTopData(new Empty(), ContextFor(Alice));
        MatchingOpponent bot = (await service.GetMatchingList(new Empty(), ContextFor(Alice))).Matching[0];

        StartBattleResponse start = await service.StartBattle(new StartBattleRequest { OpponentPlayerId = bot.PlayerId, UseDeckNumber = 1 }, ContextFor(Alice));
        Assert.Equal(3, start.OpponentDeckCharacter.Count);
        Assert.All(start.OpponentDeckCharacter, c =>
        {
            Assert.NotEqual(0, c.Costume.CostumeId);
            Assert.NotEqual(0, c.MainWeapon.WeaponId);
            Assert.Equal(2, c.SubWeapon.Count);
            Assert.NotEqual(0, c.Companion.CompanionId);
        });
        Assert.Equal(90, BattlePoints(alice));

        FinishBattleResponse finish = await service.FinishBattle(new FinishBattleRequest { OpponentPlayerId = bot.PlayerId, IsVictory = true }, ContextFor(Alice));

        Assert.Equal(18_000, finish.BeforePvpPoint);
        Assert.True(finish.AfterPvpPoint > finish.BeforePvpPoint);
        Assert.True(finish.AfterRank < finish.BeforeRank);
        Assert.NotEqual(0, finish.PvpGradeOneMatchRewardId);
        Assert.Equal(1, alice.EntityIUserPvpStatus[0].WinStreakCount);
        Assert.Contains(alice.EntityIUserConsumableItem, i => i.ConsumableItemId == GameConfig.ConsumableItemIdForArenaCoin && i.Count > 0);

        EntityIUserPvpWeeklyResult week = Assert.Single(alice.EntityIUserPvpWeeklyResult);
        Assert.Equal((SeasonId, finish.AfterPvpPoint, finish.AfterRank, ArenaHelper.GroupId), (week.PvpSeasonId, week.FinalPoint, week.FinalRank, week.GroupId));

        int gained = finish.AfterPvpPoint - finish.BeforePvpPoint;
        Assert.Equal(bot.PvpPoint - gained, _store.Arena.FindByPlayerId(SeasonId, bot.PlayerId, NowMs)!.Points);

        BattleLog log = Assert.Single((await service.GetAttackLogList(new Empty(), ContextFor(Alice))).AttackLog);
        Assert.Equal((bot.PlayerId, true, gained), (log.PlayerId, log.IsVictory, log.FluctuatedPvpPoint));
        Assert.Equal(3, log.DeckCostumeId.Count);

        GetSeasonResultResponse result = await service.GetSeasonResult(new Empty(), ContextFor(Alice));
        Assert.Equal((1, 0, gained), (result.AttackWinCount, result.AttackLoseCount, result.AttackPvpPoint));
    }

    [Fact]
    public async Task Loss_EndsTheStreak_AndFinishingWithoutABattleIsRefused()
    {
        DarkUserMemoryDatabase alice = AddPlayer(Alice, "Alice");
        await FightAsync(win: true);
        await FightAsync(win: true);
        Assert.Equal(2, alice.EntityIUserPvpStatus[0].WinStreakCount);

        (MatchingOpponent opponent, FinishBattleResponse finish) = await FightAsync(win: false);
        Assert.Equal(0, alice.EntityIUserPvpStatus[0].WinStreakCount);
        Assert.True(finish.AfterPvpPoint < finish.BeforePvpPoint);

        RpcException refused = await Assert.ThrowsAsync<RpcException>(() =>
            Service().FinishBattle(new FinishBattleRequest { OpponentPlayerId = opponent.PlayerId, IsVictory = true }, ContextFor(Alice)));
        Assert.Equal(StatusCode.FailedPrecondition, refused.StatusCode);
    }

    [Fact]
    public async Task Retire_IsALoss_AndTheBpStaysSpent()
    {
        // Retiring sets the client's IsVictory to false and finishes through the same FinishBattle call.
        DarkUserMemoryDatabase alice = AddPlayer(Alice, "Alice");
        await FightAsync(win: true);

        (_, FinishBattleResponse retired) = await FightAsync(win: false);

        Assert.True(retired.AfterPvpPoint < retired.BeforePvpPoint);
        Assert.Equal(0, alice.EntityIUserPvpStatus[0].WinStreakCount);
        Assert.Equal(80, BattlePoints(alice));
        GetSeasonResultResponse result = await Service().GetSeasonResult(new Empty(), ContextFor(Alice));
        Assert.Equal((1, 1), (result.AttackWinCount, result.AttackLoseCount));
    }

    [Fact]
    public async Task BattleStats_AreRecordedForTheArenaBattle_NotALeftoverQuest()
    {
        DarkUserMemoryDatabase alice = AddPlayer(Alice, "Alice");
        alice.EntityIUserQuest.Add(new EntityIUserQuest { UserId = Alice, QuestId = 1, QuestStateType = (int)QuestStateType.ACTIVE, LatestStartDatetime = NowMs - 60_000 });
        PvpServiceImpl service = Service();
        await service.GetTopData(new Empty(), ContextFor(Alice));
        MatchingOpponent bot = (await service.GetMatchingList(new Empty(), ContextFor(Alice))).Matching[0];
        await service.StartBattle(new StartBattleRequest { OpponentPlayerId = bot.PlayerId, UseDeckNumber = 1 }, ContextFor(Alice));
        MariesWonderland.Services.BattleService battles = new(_store, MasterDb, TestServices.Missions(MasterDb), _time);

        await battles.FinishWave(new MariesWonderland.Proto.Battle.FinishWaveRequest
        {
            ElapsedFrameCount = 900,
            BattleDetail = new MariesWonderland.Proto.Battle.BattleDetail { MaxDamage = 12_345, PlayerCostumeActiveSkillUsedCount = 2, CriticalCount = 3 },
        }, ContextFor(Alice));

        EntitySBattleDetail record = Assert.Single(_store.BattleDetails(Alice));
        Assert.Equal(0, record.QuestId);
        Assert.Equal(_store.Arena.LatestBattle(Alice, NowMs)!.Value.BattleId, record.ArenaBattleId);
        Assert.Equal((12_345, 2, 3, 900L), (record.MaxDamage, record.PlayerCostumeActiveSkillUsedCount, record.CriticalCount, record.ElapsedFrameCount));
    }

    [Fact]
    public async Task StreakFromAnEarlierSeason_EndsOnEntering()
    {
        DarkUserMemoryDatabase alice = AddPlayer(Alice, "Alice");
        alice.EntityIUserPvpStatus[0].WinStreakCount = 17;
        alice.EntityIUserPvpStatus[0].WinStreakCountUpdateDatetime = NowMs - (long)TimeSpan.FromDays(60).TotalMilliseconds;

        await Service().GetTopData(new Empty(), ContextFor(Alice));

        Assert.Equal(0, alice.EntityIUserPvpStatus[0].WinStreakCount);
    }

    [Fact]
    public async Task WeeklyRewards_ArePaidOnceTheWeekEnds()
    {
        DarkUserMemoryDatabase alice = AddPlayer(Alice, "Alice");
        (_, FinishBattleResponse finish) = await FightAsync(win: true);
        long weekStart = alice.EntityIUserPvpWeeklyResult[0].PvpWeeklyVersion;
        RewardServiceImpl rewards = new(_store, MasterDb, _time);

        Assert.Null((await rewards.ReceivePvpReward(new Empty(), ContextFor(Alice))).WeeklyGradeResult);

        _time.Advance(TimeSpan.FromDays(3));
        ReceivePvpRewardResponse paid = await rewards.ReceivePvpReward(new Empty(), ContextFor(Alice));

        Assert.Equal(SeasonId, paid.WeeklyGradeResult.TargetSeasonId);
        Assert.Equal(finish.AfterPvpPoint, paid.WeeklyGradeResult.PvpPoint);
        Assert.NotEqual(0, paid.WeeklyGradeResult.PvpGradeWeeklyRewardGroupId);
        Assert.Equal(finish.AfterRank, paid.WeeklyRankResult.Rank);
        Assert.Equal(weekStart, alice.EntityIUserPvpStatus[0].LatestRewardReceivePvpWeeklyVersion);
        Assert.Null((await rewards.ReceivePvpReward(new Empty(), ContextFor(Alice))).WeeklyGradeResult);
    }

    [Fact]
    public async Task BotProfile_ShowsItsNameDeckAndPlace()
    {
        AddPlayer(Alice, "Alice");
        await Service().GetTopData(new Empty(), ContextFor(Alice));
        UserServiceImpl users = new(_store, new UserDataSeeder(Options.Create(new ServerOptions())), MasterDb, TestServices.Missions(MasterDb), _time);

        GetUserProfileResponse profile = await users.GetUserProfile(new GetUserProfileRequest { PlayerId = ArenaBots.IdBase + 1 }, ContextFor(Alice));

        Assert.NotEmpty(profile.Name);
        Assert.Equal(3, profile.LatestUsedDeck.DeckCharacter.Count);
        Assert.True(profile.LatestUsedDeck.Power > 70_000);
        Assert.Equal(1, profile.PvpInfo.CurrentRank);
        Assert.NotEqual(0, profile.PvpInfo.CurrentGradeId);
    }

    [Fact]
    public async Task DefenseDeck_IsSaved_AndItsCardChanges()
    {
        DarkUserMemoryDatabase alice = AddPlayer(Alice, "Alice");
        alice.EntityIUserDeck.Add(new EntityIUserDeck { UserId = Alice, DeckType = DeckType.PVP, UserDeckNumber = 2, UserDeckCharacterUuid01 = "slot1", Power = 7777 });
        await Service().GetTopData(new Empty(), ContextFor(Alice));
        DeckServiceImpl decks = new(_store, MasterDb, _time);

        await decks.SetPvpDefenseDeck(new SetPvpDefenseDeckRequest { UserDeckNumber = 2 }, ContextFor(Alice));
        await decks.SetPvpDefenseDeck(new SetPvpDefenseDeckRequest { UserDeckNumber = 9 }, ContextFor(Alice));

        Assert.Equal(2, alice.EntityIUserPvpDefenseDeck[0].UserDeckNumber);
        Assert.Equal(7777, _store.Arena.Standing(SeasonId, Alice, NowMs)!.Defense.DeckPower);
    }

    [Fact]
    public async Task BpRecoveryItems_AddBp_PastTheCap()
    {
        DarkUserMemoryDatabase alice = AddPlayer(Alice, "Alice");
        alice.EntityIUserConsumableItem.Add(new EntityIUserConsumableItem { UserId = Alice, ConsumableItemId = 4003, Count = 2 });
        ConsumableItemServiceImpl items = new(MasterDb, _store, GameConfig, _time);

        await items.UseEffectItem(new UseEffectItemRequest { ConsumableItemId = 4003, Count = 1 }, ContextFor(Alice));

        Assert.Equal(200, BattlePoints(alice));
        Assert.Equal(1, alice.EntityIUserConsumableItem[0].Count);
    }

    [Fact]
    public void Bots_FillTheLadder_AndRebuildTheSameDeck()
    {
        ArenaSeason season = new(SeasonId, NowMs, NowMs + 1);
        List<ArenaEntry> bots = ArenaBots.Create(MasterDb, season);

        Assert.Equal(1601, bots.Count);
        Assert.Equal((50_000, 18_000), (bots[0].Points, bots[^1].Points));
        Assert.InRange(bots[0].DefenseDeckPower, 70_000, 80_000);
        Assert.InRange(bots[^1].DefenseDeckPower, 13_000, 17_000);
        Assert.True(bots[0].DefenseDeckPower > bots[800].DefenseDeckPower && bots[800].DefenseDeckPower > bots[^1].DefenseDeckPower);

        ArenaBotDeck deck = ArenaBots.Deck(MasterDb, bots[400].PlayerId, bots[400].BotSeed);
        Assert.Equal(bots[400].DefenseDeckPower, deck.Power);
        Assert.Equal(bots[400].DefenseCostumeIds, deck.Characters.Select(c => c.CostumeId));
        Assert.Equal(3, deck.Characters.Select(c => MasterDb.EntityMCostume.Find(c.CostumeId)!.CharacterId).Distinct().Count());
    }
}
