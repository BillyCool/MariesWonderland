using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MariesWonderland.Arena;
using MariesWonderland.Constants;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Helpers;
using MariesWonderland.Missions;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Pvp;

namespace MariesWonderland.Services;

/// <summary>
/// The Arena: asynchronous PvP against other players' defense decks and the season's bots, which the client's own
/// battle AI plays. The server picks opponents, hands over the opponent's deck, moves points when the battle ends and
/// keeps rankings, histories and weekly results (<see cref="ArenaStore"/>, server-wide). Only the caller's own data
/// changes here: BP, the win streak, rewards, missions and their weekly rows.
/// </summary>
public class PvpService(UserDataStore store, DarkMasterMemoryDatabase masterDb, GameConfig gameConfig, MissionTracker missions, TimeProvider timeProvider)
    : MariesWonderland.Proto.Pvp.PvpService.PvpServiceBase
{
    private readonly UserDataStore _store = store;
    private readonly DarkMasterMemoryDatabase _masterDb = masterDb;
    private readonly GameConfig _gameConfig = gameConfig;
    private readonly MissionTracker _missions = missions;
    private readonly TimeProvider _timeProvider = timeProvider;

    private long NowMs => _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();

    /// <summary>
    /// The top screen: the running season (0 = closed), the player's points and place. Opening it enters the player
    /// into the season. The three result popups stay empty: since 2023 they come from <c>ReceivePvpReward</c> at login.
    /// </summary>
    public override Task<GetTopDataResponse> GetTopData(Empty request, ServerCallContext context)
    {
        long nowMs = NowMs;
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(context.GetUserId());
        EntityMPvpSeason? season = ArenaHelper.CurrentSeason(_masterDb, nowMs);
        if (season == null)
        {
            ArenaHelper.SyncWeeks(_store, userDb, nowMs);
            return Task.FromResult(new GetTopDataResponse());
        }

        ArenaStanding standing = ArenaHelper.Enter(_store, _masterDb, userDb, season, nowMs);
        StaminaHelper.SettleBattlePoints(userDb.EntityIUserPvpStatus.GetOrCreate(userDb.UserId), _gameConfig, nowMs);
        return Task.FromResult(new GetTopDataResponse
        {
            CurrentSeasonId = season.PvpSeasonId,
            PvpPoint = standing.Points,
            Rank = standing.Rank,
        });
    }

    /// <summary>The opponents on offer; a first list is drawn for free.</summary>
    public override Task<GetMatchingListResponse> GetMatchingList(Empty request, ServerCallContext context)
    {
        (DarkUserMemoryDatabase userDb, EntityMPvpSeason season, long nowMs) = EnterRunningSeason(context);
        IReadOnlyList<ArenaStanding> matching = _store.Arena.Matching(season.PvpSeasonId, userDb.UserId, nowMs);
        if (matching.Count == 0)
        {
            matching = _store.Arena.NewMatching(season.PvpSeasonId, userDb.UserId, Random.Shared, nowMs);
        }

        GetMatchingListResponse response = new();
        response.Matching.AddRange(MatchingCards(matching));
        return Task.FromResult(response);
    }

    /// <summary>A new opponent list, for <see cref="GameConfig.PvpUpdateMatchingConsumeBattlePoint"/> BP.</summary>
    public override Task<UpdateMatchingListResponse> UpdateMatchingList(Empty request, ServerCallContext context)
    {
        (DarkUserMemoryDatabase userDb, EntityMPvpSeason season, long nowMs) = EnterRunningSeason(context);
        SpendBattlePoints(userDb, _gameConfig.PvpUpdateMatchingConsumeBattlePoint, nowMs);

        UpdateMatchingListResponse response = new();
        response.Matching.AddRange(MatchingCards(_store.Arena.NewMatching(season.PvpSeasonId, userDb.UserId, Random.Shared, nowMs)));
        return Task.FromResult(response);
    }

    /// <summary>
    /// Starts a battle against an opponent from the list, for <see cref="GameConfig.PvpBattleConsumeBattlePoint"/> BP,
    /// and hands over their defense deck in full (the client looks nothing up itself).
    /// </summary>
    public override Task<StartBattleResponse> StartBattle(StartBattleRequest request, ServerCallContext context)
    {
        (DarkUserMemoryDatabase userDb, EntityMPvpSeason season, long nowMs) = EnterRunningSeason(context);
        ArenaStanding opponent = Opponent(season, request.OpponentPlayerId, nowMs);
        ThrowIfRefused(_store.Arena.CanStart(season.PvpSeasonId, userDb.UserId, opponent.UserId, nowMs));

        ArenaDefense attackerDeck = ArenaDecks.Summary(userDb, _masterDb, DeckType.PVP, request.UseDeckNumber);
        if (attackerDeck.CostumeIds.Count == 0)
        {
            throw Refused(ArenaErrors.NoDeck);
        }

        (List<DeckCharacter> characters, ArenaDefense defenderDeck) = OpponentDeck(context, season, opponent, nowMs);
        SpendBattlePoints(userDb, _gameConfig.PvpBattleConsumeBattlePoint, nowMs);
        ThrowIfRefused(_store.Arena.StartBattle(season.PvpSeasonId, userDb.UserId, opponent.UserId, attackerDeck, defenderDeck, nowMs));

        StartBattleResponse response = new();
        response.OpponentDeckCharacter.AddRange(characters);
        return Task.FromResult(response);
    }

    /// <summary>
    /// Ends the battle: moves points (the defender's too), counts the win streak, pays this grade's one-match reward,
    /// records missions and the week's result, and draws a new opponent list.
    /// </summary>
    public override Task<FinishBattleResponse> FinishBattle(FinishBattleRequest request, ServerCallContext context)
    {
        (DarkUserMemoryDatabase userDb, EntityMPvpSeason season, long nowMs) = EnterRunningSeason(context);
        ArenaStanding opponent = Opponent(season, request.OpponentPlayerId, nowMs);
        ArenaFinish finish = _store.Arena.FinishBattle(season.PvpSeasonId, userDb.UserId, opponent.UserId, request.IsVictory, nowMs)
            ?? throw Refused(ArenaErrors.NoBattleInProgress);

        EntityIUserPvpStatus status = userDb.EntityIUserPvpStatus.GetOrCreate(userDb.UserId);
        status.WinStreakCount = request.IsVictory ? status.WinStreakCount + 1 : 0;
        status.WinStreakCountUpdateDatetime = nowMs;

        int rewardId = PayOneMatchReward(userDb, season, finish.BeforePoints, nowMs);

        _missions.Record(userDb, MissionClearConditionType.PVP_FINISH_BY_COUNT, 1, nowMs);
        if (request.IsVictory)
        {
            _missions.Record(userDb, MissionClearConditionType.PVP_FINISH_BY_WIN_COUNT, 1, nowMs);
            _missions.RecordBest(userDb, MissionClearConditionType.PVP_FINISH_BY_WIN_STREAK_COUNT, status.WinStreakCount, _ => true, nowMs);
            _missions.RecordBest(userDb, MissionClearConditionType.PVP_FINISH_BY_WIN_STREAK_COUNT_FROM_UNLOCK, status.WinStreakCount, _ => true, nowMs);
        }
        _missions.RecordPvpRank(userDb, finish.AfterRank, nowMs);

        ArenaHelper.SyncWeeks(_store, userDb, nowMs);
        _store.Arena.NewMatching(season.PvpSeasonId, userDb.UserId, Random.Shared, nowMs);

        return Task.FromResult(new FinishBattleResponse
        {
            BeforePvpPoint = finish.BeforePoints,
            BeforeRank = finish.BeforeRank,
            AfterPvpPoint = finish.AfterPoints,
            AfterRank = finish.AfterRank,
            PvpGradeOneMatchRewardId = rewardId,
            PvpGradeGroupId = season.PvpGradeGroupId,
        });
    }

    /// <summary>A page of 100 from <c>rankFrom</c>, how many are ranked, and the player's own place.</summary>
    public override Task<GetRankingResponse> GetRanking(GetRankingRequest request, ServerCallContext context)
    {
        long nowMs = NowMs;
        long userId = context.GetUserId();
        EntityMPvpSeason? season = ArenaHelper.CurrentSeason(_masterDb, nowMs);
        if (season == null)
        {
            return Task.FromResult(new GetRankingResponse());
        }

        (IReadOnlyList<ArenaStanding> page, int count) = _store.Arena.Ranking(season.PvpSeasonId, request.RankFrom, ArenaStore.RankingPageSize, nowMs);
        Dictionary<long, PlayerCard> cards = PlayerCardsOf(page);
        GetRankingResponse response = new()
        {
            UserCount = count,
            RankingPosition = _store.Arena.Standing(season.PvpSeasonId, userId, nowMs)?.Rank ?? 0,
        };
        response.RankingUser.AddRange(page.Select(s => new RankingUser
        {
            Rank = s.Rank,
            PlayerId = s.PlayerId,
            Name = NameOf(s, cards),
            PvpPoint = s.Points,
            DeckPower = s.Defense.DeckPower,
            FavoriteCostumeId = s.IsBot ? s.BotFavoriteCostumeId : cards.GetValueOrDefault(s.UserId)?.FavoriteCostumeId ?? 0,
        }));
        return Task.FromResult(response);
    }

    /// <summary>The season so far: attack wins and losses, points from attacks, defense win rate and points.</summary>
    public override Task<GetSeasonResultResponse> GetSeasonResult(Empty request, ServerCallContext context)
    {
        long nowMs = NowMs;
        EntityMPvpSeason? season = ArenaHelper.CurrentSeason(_masterDb, nowMs);
        ArenaStanding? standing = season == null ? null : _store.Arena.Standing(season.PvpSeasonId, context.GetUserId(), nowMs);
        if (standing == null)
        {
            return Task.FromResult(new GetSeasonResultResponse());
        }

        int defenses = standing.DefenseWins + standing.DefenseLosses;
        return Task.FromResult(new GetSeasonResultResponse
        {
            AttackWinCount = standing.AttackWins,
            AttackLoseCount = standing.AttackLosses,
            AttackPvpPoint = standing.AttackPointsChange,
            DefenseWinRatePermil = defenses == 0 ? 0 : standing.DefenseWins * 1000 / defenses,
            DefensePvpPoint = standing.DefensePointsChange,
        });
    }

    /// <summary>The player's attacks this season, newest first.</summary>
    public override Task<GetAttackLogListResponse> GetAttackLogList(Empty request, ServerCallContext context)
    {
        GetAttackLogListResponse response = new();
        response.AttackLog.AddRange(Log(context, asAttacker: true));
        return Task.FromResult(response);
    }

    /// <summary>The attacks on the player's defense deck this season, newest first.</summary>
    public override Task<GetDefenseLogListResponse> GetDefenseLogList(Empty request, ServerCallContext context)
    {
        GetDefenseLogListResponse response = new();
        response.DefenseLog.AddRange(Log(context, asAttacker: false));
        return Task.FromResult(response);
    }

    // ---- Helpers ----

    /// <summary>The caller, entered into the running season; refuses when no season is running.</summary>
    private (DarkUserMemoryDatabase UserDb, EntityMPvpSeason Season, long NowMs) EnterRunningSeason(ServerCallContext context)
    {
        long nowMs = NowMs;
        EntityMPvpSeason season = ArenaHelper.CurrentSeason(_masterDb, nowMs) ?? throw Refused(ArenaErrors.NotOpen);
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(context.GetUserId());
        ArenaHelper.Enter(_store, _masterDb, userDb, season, nowMs);
        return (userDb, season, nowMs);
    }

    private ArenaStanding Opponent(EntityMPvpSeason season, long playerId, long nowMs)
        => _store.Arena.FindByPlayerId(season.PvpSeasonId, playerId, nowMs)
            ?? throw new RpcException(new Status(StatusCode.NotFound, ArenaErrors.OpponentNotFound));

    /// <summary>
    /// The opponent's defense deck as the client fields it, and as the logs show it. A bot's is rebuilt from its seed;
    /// a player's is read under their lock, and what others see of it is refreshed while at it.
    /// </summary>
    private (List<DeckCharacter> Characters, ArenaDefense Summary) OpponentDeck(ServerCallContext context, EntityMPvpSeason season, ArenaStanding opponent, long nowMs)
    {
        if (opponent.IsBot)
        {
            ArenaBotDeck deck = ArenaBots.Deck(_masterDb, opponent.PlayerId, opponent.BotSeed);
            return (ArenaBots.Build(_masterDb, deck), opponent.Defense);
        }

        using IDisposable? opponentLock = context.TryLockUser(_store, opponent.UserId);
        if (opponentLock == null || !_store.TryGet(opponent.UserId, out DarkUserMemoryDatabase opponentDb))
        {
            throw Refused(ArenaErrors.OpponentHasNoDeck);
        }

        int deckNumber = ArenaDecks.DefenseDeckNumber(opponentDb);
        List<DeckCharacter> characters = ArenaDecks.Build(opponentDb, _masterDb, DeckType.PVP, deckNumber);
        ArenaDefense summary = ArenaDecks.Summary(opponentDb, _masterDb, DeckType.PVP, deckNumber);
        _store.Arena.UpdateDefense(season.PvpSeasonId, opponent.UserId, summary, nowMs);
        if (characters.Count == 0)
        {
            throw Refused(ArenaErrors.OpponentHasNoDeck);
        }
        return (characters, summary);
    }

    private void SpendBattlePoints(DarkUserMemoryDatabase userDb, int cost, long nowMs)
    {
        EntityIUserPvpStatus status = userDb.EntityIUserPvpStatus.GetOrCreate(userDb.UserId);
        if (!StaminaHelper.TryConsumeBattlePoints(status, cost, _gameConfig, nowMs))
        {
            throw Refused(ArenaErrors.NotEnoughBattlePoints);
        }
    }

    /// <summary>
    /// Pays the one-match reward of the grade the battle was fought at (every battle, won or lost), drawn by weight
    /// from the grade's group. Returns the reward id the result screen lists; 0 when the grade has none.
    /// </summary>
    private int PayOneMatchReward(DarkUserMemoryDatabase userDb, EntityMPvpSeason season, int points, long nowMs)
    {
        EntityMPvpGradeGroup? grade = ArenaHelper.Grade(_masterDb, season.PvpGradeGroupId, points);
        List<EntityMPvpGradeOneMatchRewardGroup> options = grade == null
            ? []
            : [.. _masterDb.EntityMPvpGradeOneMatchRewardGroup.Where(g => g.PvpGradeOneMatchRewardGroupId == grade.PvpGradeOneMatchRewardGroupId && g.Weight > 0)];
        if (options.Count == 0)
        {
            return 0;
        }

        int roll = Random.Shared.Next(options.Sum(o => o.Weight));
        EntityMPvpGradeOneMatchRewardGroup picked = options[^1];
        foreach (EntityMPvpGradeOneMatchRewardGroup option in options)
        {
            if (roll < option.Weight)
            {
                picked = option;
                break;
            }
            roll -= option.Weight;
        }

        foreach (EntityMPvpGradeOneMatchReward reward in _masterDb.EntityMPvpGradeOneMatchReward
            .Where(r => r.PvpGradeOneMatchRewardId == picked.PvpGradeOneMatchRewardId)
            .OrderBy(r => r.SortOrder))
        {
            EntityMPvpReward? item = _masterDb.EntityMPvpReward.FirstOrDefault(r => r.PvpRewardId == reward.PvpRewardId);
            if (item != null)
            {
                PossessionHelper.Apply(userDb, item.PossessionType, item.PossessionId, item.Count, _masterDb, nowMs);
            }
        }
        return picked.PvpGradeOneMatchRewardId;
    }

    private IEnumerable<MatchingOpponent> MatchingCards(IReadOnlyList<ArenaStanding> opponents)
    {
        Dictionary<long, PlayerCard> cards = PlayerCardsOf(opponents);
        return opponents.Select(o =>
        {
            MatchingOpponent card = new()
            {
                PlayerId = o.PlayerId,
                Name = NameOf(o, cards),
                PvpPoint = o.Points,
                Rank = o.Rank,
                DeckPower = o.Defense.DeckPower,
                MostPowerfulCostumeId = o.Defense.MostPowerfulCostumeId,
            };
            card.DeckMainWeaponAttributeType.AddRange(o.Defense.AttributeTypes);
            return card;
        });
    }

    private IEnumerable<BattleLog> Log(ServerCallContext context, bool asAttacker)
    {
        long nowMs = NowMs;
        EntityMPvpSeason? season = ArenaHelper.CurrentSeason(_masterDb, nowMs);
        if (season == null)
        {
            return [];
        }

        IReadOnlyList<ArenaLogLine> lines = _store.Arena.Log(season.PvpSeasonId, context.GetUserId(), asAttacker, nowMs);
        Dictionary<long, PlayerCard> cards = PlayerCardsOf(lines.Select(l => l.Opponent));
        List<BattleLog> logs = [];
        foreach (ArenaLogLine line in lines)
        {
            // The client takes the first costume for the row's icon, so a row needs at least one.
            if (line.OpponentCostumeIds.Count == 0)
            {
                continue;
            }

            BattleLog log = new()
            {
                PlayerId = line.Opponent.PlayerId,
                Name = NameOf(line.Opponent, cards),
                PvpPoint = line.Opponent.Points,
                DeckPower = line.OpponentDeckPower,
                IsVictory = line.IsVictory,
                BattleDatetime = Timestamp.FromDateTimeOffset(DateTimeOffset.FromUnixTimeMilliseconds(line.BattleAt)),
                FluctuatedPvpPoint = line.PointsChange,
                Rank = line.Opponent.Rank,
            };
            log.DeckCostumeId.AddRange(line.OpponentCostumeIds);
            logs.Add(log);
        }
        return logs;
    }

    /// <summary>The cards (names, favorite costumes) of the real players among these, by user id.</summary>
    private Dictionary<long, PlayerCard> PlayerCardsOf(IEnumerable<ArenaStanding> standings)
    {
        long[] userIds = [.. standings.Where(s => !s.IsBot).Select(s => s.UserId).Distinct()];
        return userIds.Length == 0 ? [] : _store.PlayerCards(userIds).ToDictionary(c => c.UserId);
    }

    private static string NameOf(ArenaStanding standing, Dictionary<long, PlayerCard> cards)
        => standing.IsBot ? ArenaBots.Name(standing.BotSeed) : cards.GetValueOrDefault(standing.UserId)?.Name ?? string.Empty;

    private static void ThrowIfRefused(ArenaOutcome outcome)
    {
        string? message = outcome switch
        {
            ArenaOutcome.Ok => null,
            ArenaOutcome.NotEntered => ArenaErrors.NotEntered,
            ArenaOutcome.NotInMatchingList => ArenaErrors.NotInMatchingList,
            ArenaOutcome.NoBattleInProgress => ArenaErrors.NoBattleInProgress,
            _ => outcome.ToString(),
        };
        if (message != null)
        {
            throw Refused(message);
        }
    }

    private static RpcException Refused(string message) => new(new Status(StatusCode.FailedPrecondition, message));
}
