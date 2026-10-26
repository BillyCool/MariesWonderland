using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MariesWonderland.Constants;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Friends;
using MariesWonderland.Helpers;
using MariesWonderland.Missions;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Friend;

namespace MariesWonderland.Services;

/// <summary>
/// Friends: search, recommendations, requests, the friend list and cheers (support). Friendships, requests and cheers
/// live in <see cref="UserDataStore.Friends"/>; only the caller's own data changes here (stamina from cheers, missions).
/// Other players are shown from their <see cref="PlayerCard"/>, so no other player is loaded or locked.
/// </summary>
public class FriendService(UserDataStore store, DarkMasterMemoryDatabase masterDb, GameConfig gameConfig, MissionTracker missions, TimeProvider timeProvider)
    : MariesWonderland.Proto.Friend.FriendService.FriendServiceBase
{
    /// <summary>Stamina for sending one cheer (owner's call; not in the client or master data).</summary>
    public const int SendCheerStamina = 1;

    /// <summary>Stamina for taking one cheer (owner's call; not in the client or master data).</summary>
    public const int ReceiveCheerStamina = 1;

    /// <summary>How many recommendations to send; the client keeps 100 and shows a random 20.</summary>
    public const int RecommendedUserCount = 100;

    /// <summary>Players who logged in this recently are recommended before everyone else.</summary>
    public static readonly TimeSpan RecentLogin = TimeSpan.FromDays(7);

    private readonly UserDataStore _store = store;
    private readonly DarkMasterMemoryDatabase _masterDb = masterDb;
    private readonly GameConfig _gameConfig = gameConfig;
    private readonly MissionTracker _missions = missions;
    private readonly TimeProvider _timeProvider = timeProvider;

    private FriendStore Friends => _store.Friends;
    private FriendLimits Limits => FriendLimits.From(_gameConfig);
    private long NowMs => _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();

    /// <summary>ID search. An unknown ID returns no user, which the client shows as "No player with this ID."</summary>
    public override Task<GetUserResponse> GetUser(GetUserRequest request, ServerCallContext context)
    {
        long? userId = _store.FindUserIdByPlayerId(request.PlayerId);
        PlayerCard? card = userId == null ? null : _store.PlayerCards([userId.Value]).FirstOrDefault();
        return Task.FromResult(new GetUserResponse { User = card == null ? null : ToUser(card) });
    }

    /// <summary>
    /// Up to 100 players to befriend: not me, not friends, no pending request either way. Recent logins come first,
    /// then whoever is closest to my level, so a small server still fills the list with everyone there is.
    /// </summary>
    public override Task<SearchRecommendedUsersResponse> SearchRecommendedUsers(Empty request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        long nowMs = NowMs;
        HashSet<long> excluded = [userId, .. Friends.FriendsOf(userId), .. Friends.RequestPartners(userId, nowMs)];
        int myLevel = _store.PlayerCards([userId]).FirstOrDefault()?.Level ?? 1;
        long recentSince = nowMs - (long)RecentLogin.TotalMilliseconds;

        IEnumerable<PlayerCard> recommended = _store.PlayerCards()
            .Where(c => !excluded.Contains(c.UserId) && c.PlayerId != 0 && !string.IsNullOrEmpty(c.Name))
            .OrderByDescending(c => c.LastLoginDatetime >= recentSince)
            .ThenBy(c => Math.Abs(c.Level - myLevel))
            .ThenByDescending(c => c.LastLoginDatetime)
            .Take(RecommendedUserCount);

        SearchRecommendedUsersResponse response = new();
        response.Users.AddRange(recommended.Select(ToUser));
        return Task.FromResult(response);
    }

    /// <summary>The friend list with today's cheers, and takes the Cage values measured since the last report.</summary>
    public override Task<GetFriendListResponse> GetFriendList(GetFriendListRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        long nowMs = NowMs;
        if (request.CageMeasurableValues is { } cage)
        {
            DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);
            _missions.RecordCageValues(userDb, cage.RunningDistanceMeters, cage.MamaTappedCount, nowMs);
        }

        GetFriendListResponse response = new();
        foreach (PlayerCard card in _store.PlayerCards(Friends.FriendsOf(userId)))
        {
            CheerStatus cheers = Friends.Cheers(userId, card.UserId, nowMs);
            response.FriendUser.Add(new FriendUser
            {
                PlayerId = card.PlayerId,
                UserName = card.Name,
                LastLoginDatetime = ToTimestamp(card.LastLoginDatetime),
                MaxDeckPower = card.MaxDeckPower,
                FavoriteCostumeId = card.FavoriteCostumeId,
                Level = card.Level,
                CheerSent = cheers.Sent,
                CheerReceived = cheers.Receivable,
                StaminaReceived = cheers.Received,
            });
        }

        (int sent, int received) = Friends.CheerCounts(userId, nowMs);
        response.SendCheerCount = sent;
        response.ReceivedCheerCount = received;
        return Task.FromResult(response);
    }

    /// <summary>Pending requests sent to me, newest first.</summary>
    public override Task<GetFriendRequestListResponse> GetFriendRequestList(Empty request, ServerCallContext context)
    {
        IReadOnlyList<long> senders = Friends.PendingRequestSenders(context.GetUserId(), NowMs);
        Dictionary<long, PlayerCard> cards = _store.PlayerCards(senders).ToDictionary(c => c.UserId);

        GetFriendRequestListResponse response = new();
        response.User.AddRange(senders.Where(cards.ContainsKey).Select(id => ToUser(cards[id])));
        return Task.FromResult(response);
    }

    /// <summary>Sends a friend request; if they had already sent me one, we become friends.</summary>
    public override Task<SendFriendRequestResponse> SendFriendRequest(SendFriendRequestRequest request, ServerCallContext context)
    {
        long targetId = RequireUser(request.PlayerId);
        ThrowIfRefused(Friends.SendRequest(context.GetUserId(), targetId, NowMs, Limits));
        return Task.FromResult(new SendFriendRequestResponse());
    }

    public override Task<AcceptFriendRequestResponse> AcceptFriendRequest(AcceptFriendRequestRequest request, ServerCallContext context)
    {
        long fromId = RequireUser(request.PlayerId);
        ThrowIfRefused(Friends.AcceptRequest(context.GetUserId(), fromId, NowMs, Limits));
        return Task.FromResult(new AcceptFriendRequestResponse());
    }

    /// <summary>Declines one request, or all of them ("Reject All" sends every pending id). Unknown ids are skipped.</summary>
    public override Task<DeclineFriendRequestResponse> DeclineFriendRequest(DeclineFriendRequestRequest request, ServerCallContext context)
    {
        IEnumerable<long> playerIds = request.PlayerIdOld != 0 ? request.PlayerId.Append(request.PlayerIdOld) : request.PlayerId;
        List<long> fromIds = [.. playerIds.Select(_store.FindUserIdByPlayerId).OfType<long>()];
        Friends.DeclineRequests(context.GetUserId(), fromIds, NowMs);
        return Task.FromResult(new DeclineFriendRequestResponse());
    }

    /// <summary>Removes a friend (from their profile). Removing someone who isn't a friend does nothing.</summary>
    public override Task<DeleteFriendResponse> DeleteFriend(DeleteFriendRequest request, ServerCallContext context)
    {
        long? friendId = _store.FindUserIdByPlayerId(request.PlayerId);
        if (friendId != null)
        {
            Friends.RemoveFriend(context.GetUserId(), friendId.Value, NowMs);
        }
        return Task.FromResult(new DeleteFriendResponse());
    }

    /// <summary>Cheers one friend: stamina for me, and it counts for the friend-support missions.</summary>
    public override Task<CheerFriendResponse> CheerFriend(CheerFriendRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        long friendId = RequireUser(request.PlayerId);
        long nowMs = NowMs;
        ThrowIfRefused(Friends.Cheer(userId, friendId, nowMs, Limits));

        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);
        AddStamina(userDb, SendCheerStamina, nowMs);
        _missions.Record(userDb, MissionClearConditionType.CHEER_FRIEND_BY_COUNT, 1, nowMs);
        return Task.FromResult(new CheerFriendResponse());
    }

    /// <summary>"Send All": cheers every friend not cheered today, up to the daily limit.</summary>
    public override Task<BulkCheerFriendResponse> BulkCheerFriend(Empty request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        long nowMs = NowMs;
        IReadOnlyList<long> cheered = Friends.CheerAll(userId, nowMs, Limits);

        BulkCheerFriendResponse response = new();
        if (cheered.Count > 0)
        {
            DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);
            AddStamina(userDb, SendCheerStamina * cheered.Count, nowMs);
            _missions.Record(userDb, MissionClearConditionType.CHEER_FRIEND_BY_COUNT, cheered.Count, nowMs);
            response.PlayerId.AddRange(PlayerIds(cheered));
        }
        return Task.FromResult(response);
    }

    /// <summary>Takes one friend's cheer from today: stamina for me.</summary>
    public override Task<ReceiveCheerResponse> ReceiveCheer(ReceiveCheerRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        long friendId = RequireUser(request.PlayerId);
        long nowMs = NowMs;
        ThrowIfRefused(Friends.ReceiveCheer(userId, friendId, nowMs, Limits));

        AddStamina(_store.GetOrCreate(userId), ReceiveCheerStamina, nowMs);
        return Task.FromResult(new ReceiveCheerResponse());
    }

    /// <summary>"Take All": takes every friend's cheer from today, up to the daily limit.</summary>
    public override Task<BulkReceiveCheerResponse> BulkReceiveCheer(Empty request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        long nowMs = NowMs;
        IReadOnlyList<long> received = Friends.ReceiveAll(userId, nowMs, Limits);

        BulkReceiveCheerResponse response = new();
        if (received.Count > 0)
        {
            AddStamina(_store.GetOrCreate(userId), ReceiveCheerStamina * received.Count, nowMs);
            response.PlayerId.AddRange(PlayerIds(received));
        }
        return Task.FromResult(response);
    }

    /// <summary>Adds stamina after settling regeneration, so the client's before → after popup shows the gain.</summary>
    private void AddStamina(DarkUserMemoryDatabase userDb, int amount, long nowMs)
    {
        EntityIUserStatus? status = userDb.EntityIUserStatus.FirstOrDefault();
        if (status == null)
        {
            return;
        }

        StaminaHelper.Settle(status, _masterDb, _gameConfig, nowMs);
        StaminaHelper.AddStamina(status, amount, _gameConfig, nowMs);
    }

    private long RequireUser(long playerId)
        => _store.FindUserIdByPlayerId(playerId)
            ?? throw new RpcException(new Status(StatusCode.NotFound, FriendErrors.PlayerNotFound));

    private IEnumerable<long> PlayerIds(IReadOnlyList<long> userIds)
        => _store.PlayerCards(userIds).Select(c => c.PlayerId);

    private static void ThrowIfRefused(FriendOutcome outcome)
    {
        string? message = outcome switch
        {
            FriendOutcome.Ok or FriendOutcome.BecameFriends => null,
            FriendOutcome.Myself => FriendErrors.Myself,
            FriendOutcome.AlreadyFriend => FriendErrors.AlreadyFriend,
            FriendOutcome.MyFriendsFull => FriendErrors.MyFriendsFull,
            FriendOutcome.TheirFriendsFull => FriendErrors.TheirFriendsFull,
            FriendOutcome.RequestDayLimit => FriendErrors.RequestDayLimit,
            FriendOutcome.TheirRequestsFull => FriendErrors.TheirRequestsFull,
            FriendOutcome.RequestNotFound => FriendErrors.RequestNotFound,
            FriendOutcome.NotFriend => FriendErrors.NotFriend,
            FriendOutcome.CheerAlreadySent => FriendErrors.CheerAlreadySent,
            FriendOutcome.CheerSendDayLimit => FriendErrors.CheerSendDayLimit,
            FriendOutcome.NoCheerToReceive => FriendErrors.NoCheerToReceive,
            FriendOutcome.CheerReceiveDayLimit => FriendErrors.CheerReceiveDayLimit,
            _ => outcome.ToString(),
        };

        if (message != null)
        {
            throw new RpcException(new Status(StatusCode.FailedPrecondition, message));
        }
    }

    private static User ToUser(PlayerCard card) => new()
    {
        PlayerId = card.PlayerId,
        UserName = card.Name,
        LastLoginDatetime = ToTimestamp(card.LastLoginDatetime),
        MaxDeckPower = card.MaxDeckPower,
        FavoriteCostumeId = card.FavoriteCostumeId,
        Level = card.Level,
    };

    private static Timestamp ToTimestamp(long unixMs) => Timestamp.FromDateTimeOffset(DateTimeOffset.FromUnixTimeMilliseconds(unixMs));
}
