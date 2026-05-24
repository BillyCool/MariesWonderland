using Google.Protobuf.WellKnownTypes;
using MariesWonderland.Proto.Data;
using MariesWonderland.Proto.Pvp;
using MariesWonderland.Tests.Infrastructure;
using PvpServiceImpl = MariesWonderland.Services.PvpService;

namespace MariesWonderland.Tests.Services;

public class PvpServiceTests
{
    private const long UserId = 1L;

    [Fact]
    public async Task GetTopData_ReturnsFreshEmptyResponse()
    {
        await AssertFreshEmptyResponseAsync(
            (service, context) => service.GetTopData(new Empty(), context),
            response =>
            {
                Assert.Equal(0, response.CurrentSeasonId);
                Assert.Equal(0, response.PvpPoint);
                Assert.Equal(0, response.Rank);
                Assert.Null(response.WeeklyGradeResult);
                Assert.Null(response.SeasonRankResult);
                Assert.Null(response.WeeklyRankResult);
                Assert.Empty(response.DiffUserData);
            },
            response =>
            {
                response.CurrentSeasonId = 1;
                response.PvpPoint = 2;
                response.Rank = 3;
                response.WeeklyGradeResult = new WeeklyGradeResult { TargetSeasonId = 4 };
                response.SeasonRankResult = new SeasonRankResult { TargetSeasonId = 5 };
                response.WeeklyRankResult = new WeeklyRankResult { TargetSeasonId = 6 };
                response.DiffUserData.Add("patched", new DiffData());
            });
    }

    [Fact]
    public async Task MatchingListRequests_ReturnFreshEmptyResponses()
    {
        await AssertFreshEmptyResponseAsync(
            (service, context) => service.GetMatchingList(new Empty(), context),
            response =>
            {
                Assert.Empty(response.Matching);
                Assert.Empty(response.DiffUserData);
            },
            response =>
            {
                response.Matching.Add(new MatchingOpponent { PlayerId = 99L, Name = "patched" });
                response.DiffUserData.Add("patched", new DiffData());
            });

        await AssertFreshEmptyResponseAsync(
            (service, context) => service.UpdateMatchingList(new Empty(), context),
            response =>
            {
                Assert.Empty(response.Matching);
                Assert.Empty(response.DiffUserData);
            },
            response =>
            {
                response.Matching.Add(new MatchingOpponent { PlayerId = 100L, Name = "patched" });
                response.DiffUserData.Add("patched", new DiffData());
            });
    }

    [Fact]
    public async Task StartBattle_ReturnsFreshEmptyOpponentDeck()
    {
        await AssertFreshEmptyResponseAsync(
            (service, context) => service.StartBattle(
                new StartBattleRequest
                {
                    OpponentPlayerId = 12345L,
                    UseDeckNumber = 3
                },
                context),
            response =>
            {
                Assert.Empty(response.OpponentDeckCharacter);
                Assert.Empty(response.DiffUserData);
            },
            response =>
            {
                response.OpponentDeckCharacter.Add(new DeckCharacter
                {
                    Costume = new CostumeInfo { CostumeId = 101 }
                });
                response.DiffUserData.Add("patched", new DiffData());
            });
    }

    [Fact]
    public async Task FinishBattle_ReturnsFreshZeroValuedSummary()
    {
        await AssertFreshEmptyResponseAsync(
            (service, context) => service.FinishBattle(
                new FinishBattleRequest
                {
                    OpponentPlayerId = 98765L,
                    IsVictory = true
                },
                context),
            response =>
            {
                Assert.Equal(0, response.BeforePvpPoint);
                Assert.Equal(0, response.BeforeRank);
                Assert.Equal(0, response.AfterPvpPoint);
                Assert.Equal(0, response.AfterRank);
                Assert.Equal(0, response.PvpGradeOneMatchRewardId);
                Assert.Equal(0, response.PvpGradeGroupId);
                Assert.Empty(response.DiffUserData);
            },
            response =>
            {
                response.BeforePvpPoint = 1;
                response.BeforeRank = 2;
                response.AfterPvpPoint = 3;
                response.AfterRank = 4;
                response.PvpGradeOneMatchRewardId = 5;
                response.PvpGradeGroupId = 6;
                response.DiffUserData.Add("patched", new DiffData());
            });
    }

    [Fact]
    public async Task GetRanking_ReturnsFreshEmptyRankingResponse()
    {
        await AssertFreshEmptyResponseAsync(
            (service, context) => service.GetRanking(new GetRankingRequest { RankFrom = 10 }, context),
            response =>
            {
                Assert.Empty(response.RankingUser);
                Assert.Equal(0, response.UserCount);
                Assert.Equal(0, response.RankingPosition);
                Assert.Empty(response.DiffUserData);
            },
            response =>
            {
                response.RankingUser.Add(new RankingUser { Rank = 1, PlayerId = 99L, Name = "patched" });
                response.UserCount = 1;
                response.RankingPosition = 1;
                response.DiffUserData.Add("patched", new DiffData());
            });
    }

    [Fact]
    public async Task GetSeasonResult_ReturnsFreshZeroValuedSeasonSummary()
    {
        await AssertFreshEmptyResponseAsync(
            (service, context) => service.GetSeasonResult(new Empty(), context),
            response =>
            {
                Assert.Equal(0, response.AttackWinCount);
                Assert.Equal(0, response.AttackLoseCount);
                Assert.Equal(0, response.AttackPvpPoint);
                Assert.Equal(0, response.DefenseWinRatePermil);
                Assert.Equal(0, response.DefensePvpPoint);
                Assert.Empty(response.DiffUserData);
            },
            response =>
            {
                response.AttackWinCount = 1;
                response.AttackLoseCount = 2;
                response.AttackPvpPoint = 3;
                response.DefenseWinRatePermil = 4;
                response.DefensePvpPoint = 5;
                response.DiffUserData.Add("patched", new DiffData());
            });
    }

    [Fact]
    public async Task BattleLogQueries_ReturnFreshEmptyResponses()
    {
        await AssertFreshEmptyResponseAsync(
            (service, context) => service.GetAttackLogList(new Empty(), context),
            response =>
            {
                Assert.Empty(response.AttackLog);
                Assert.Empty(response.DiffUserData);
            },
            response =>
            {
                response.AttackLog.Add(new BattleLog { PlayerId = 10L, Name = "patched" });
                response.DiffUserData.Add("patched", new DiffData());
            });

        await AssertFreshEmptyResponseAsync(
            (service, context) => service.GetDefenseLogList(new Empty(), context),
            response =>
            {
                Assert.Empty(response.DefenseLog);
                Assert.Empty(response.DiffUserData);
            },
            response =>
            {
                response.DefenseLog.Add(new BattleLog { PlayerId = 11L, Name = "patched" });
                response.DiffUserData.Add("patched", new DiffData());
            });
    }

    private static async Task AssertFreshEmptyResponseAsync<TResponse>(
        Func<PvpServiceImpl, Grpc.Core.ServerCallContext, Task<TResponse>> invoke,
        Action<TResponse> assertEmpty,
        Action<TResponse> mutate)
        where TResponse : class
    {
        PvpServiceImpl service = new();

        TResponse firstResponse = await invoke(service, FakeServerCallContext.For(UserId));
        Assert.NotNull(firstResponse);
        assertEmpty(firstResponse);
        mutate(firstResponse);

        TResponse secondResponse = await invoke(service, FakeServerCallContext.For(userId: 999));
        Assert.NotSame(firstResponse, secondResponse);
        assertEmpty(secondResponse);
    }
}
