using MariesWonderland.Data;
using MariesWonderland.Helpers;
using MariesWonderland.Persistence;

namespace MariesWonderland.Friends;

/// <summary>Why a friend operation was refused, or <see cref="Ok"/>.</summary>
public enum FriendOutcome
{
    Ok,
    /// <summary>The request crossed one the other player had already sent, so they are friends now.</summary>
    BecameFriends,
    Myself,
    AlreadyFriend,
    MyFriendsFull,
    TheirFriendsFull,
    RequestDayLimit,
    TheirRequestsFull,
    RequestNotFound,
    NotFriend,
    CheerAlreadySent,
    CheerSendDayLimit,
    NoCheerToReceive,
    CheerReceiveDayLimit,
}

/// <summary>The friend limits, from master config (100 each); the daily request limit isn't in config.</summary>
public sealed record FriendLimits(int MaxFriends, int MaxPendingReceived, int MaxRequestsPerDay, int MaxCheersSent, int MaxCheersReceived)
{
    public const int DefaultLimit = 100;

    public static FriendLimits From(GameConfig config) => new(
        OrDefault(config.UserFriendMaxNumber),
        OrDefault(config.UserFriendReceiveRequestMaxNumber),
        DefaultLimit,
        OrDefault(config.UserFriendSendCheerMaxNumber),
        OrDefault(config.UserFriendReceiveCheerMaxNumber));

    private static int OrDefault(int value) => value > 0 ? value : DefaultLimit;
}

/// <summary>Today's cheers between me and one friend.</summary>
/// <param name="Sent">I cheered them today.</param>
/// <param name="Receivable">They cheered me today and I haven't taken it.</param>
/// <param name="Received">They cheered me today and I took it.</param>
public readonly record struct CheerStatus(bool Sent, bool Receivable, bool Received);

/// <summary>
/// Friendships, friend requests and cheers. They belong to two players at once, so they live here, server-wide,
/// rather than in either user's data; nothing here locks a user. Everything is kept in memory and written through
/// to the user database (when there is one). Safe to use from parallel requests: each operation runs under one lock,
/// so limits hold when two players act at the same moment.
/// Days are game days (<see cref="GameDayHelper"/>): cheers and the daily limits reset at 08:00 UTC.
/// </summary>
public sealed class FriendStore
{
    /// <summary>How long a request waits for an answer before it is withdrawn.</summary>
    public static readonly TimeSpan RequestLifetime = TimeSpan.FromDays(7);

    private readonly Lock _lock = new();
    private readonly IUserPersistence? _persistence;
    private readonly Dictionary<long, HashSet<long>> _friends = [];
    private readonly Dictionary<(long From, long To), FriendRequest> _requests = [];
    private readonly Dictionary<(long From, long To), FriendCheer> _cheers = [];
    private long _prunedDay;

    public FriendStore(IUserPersistence? persistence = null)
    {
        _persistence = persistence;
        FriendState? state = persistence?.LoadFriends();
        if (state == null)
        {
            return;
        }

        foreach (FriendLink link in state.Links)
        {
            Link(link.UserId, link.FriendUserId);
        }
        foreach (FriendRequest request in state.Requests)
        {
            _requests[(request.FromUserId, request.ToUserId)] = request;
        }
        foreach (FriendCheer cheer in state.Cheers)
        {
            _cheers[(cheer.FromUserId, cheer.ToUserId)] = cheer;
        }
    }

    // ---- Reads ----

    /// <summary>The user's friends.</summary>
    public IReadOnlyList<long> FriendsOf(long userId)
    {
        lock (_lock)
        {
            return _friends.TryGetValue(userId, out HashSet<long>? friends) ? [.. friends] : [];
        }
    }

    public bool AreFriends(long userId, long otherUserId)
    {
        lock (_lock)
        {
            return AreFriendsLocked(userId, otherUserId);
        }
    }

    /// <summary>Who has a pending request to the user, newest first.</summary>
    public IReadOnlyList<long> PendingRequestSenders(long userId, long nowMs)
    {
        lock (_lock)
        {
            return [.. _requests.Values
                .Where(r => r.ToUserId == userId && IsPending(r, nowMs))
                .OrderByDescending(r => r.SentAt)
                .Select(r => r.FromUserId)];
        }
    }

    /// <summary>How many pending requests the user has received (the header badge).</summary>
    public int PendingRequestCount(long userId, long nowMs)
    {
        lock (_lock)
        {
            return PendingCountLocked(userId, nowMs);
        }
    }

    /// <summary>Everyone the user has a pending request with, sent or received.</summary>
    public IReadOnlySet<long> RequestPartners(long userId, long nowMs)
    {
        lock (_lock)
        {
            HashSet<long> partners = [];
            foreach (FriendRequest request in _requests.Values)
            {
                if (!IsPending(request, nowMs))
                {
                    continue;
                }

                if (request.FromUserId == userId)
                {
                    partners.Add(request.ToUserId);
                }
                else if (request.ToUserId == userId)
                {
                    partners.Add(request.FromUserId);
                }
            }
            return partners;
        }
    }

    /// <summary>Today's cheers between the user and one friend.</summary>
    public CheerStatus Cheers(long userId, long friendUserId, long nowMs)
    {
        lock (_lock)
        {
            long today = GameDayHelper.StartOfDay(nowMs);
            bool sent = _cheers.TryGetValue((userId, friendUserId), out FriendCheer? mine) && mine.SentAt >= today;
            bool theirs = _cheers.TryGetValue((friendUserId, userId), out FriendCheer? cheer) && cheer.SentAt >= today;
            return new CheerStatus(sent, theirs && cheer!.ReceivedAt == 0, theirs && cheer!.ReceivedAt != 0);
        }
    }

    /// <summary>How many cheers the user sent and took today.</summary>
    public (int Sent, int Received) CheerCounts(long userId, long nowMs)
    {
        lock (_lock)
        {
            long today = GameDayHelper.StartOfDay(nowMs);
            return (SentCountLocked(userId, today), ReceivedCountLocked(userId, today));
        }
    }

    // ---- Requests and friendships ----

    /// <summary>
    /// Sends a request. A request crossing a pending one from the other player makes them friends at once; sending
    /// again while one is pending does nothing.
    /// </summary>
    public FriendOutcome SendRequest(long fromUserId, long toUserId, long nowMs, FriendLimits limits)
        => Write(nowMs, changes =>
        {
            if (fromUserId == toUserId)
            {
                return FriendOutcome.Myself;
            }
            if (AreFriendsLocked(fromUserId, toUserId))
            {
                return FriendOutcome.AlreadyFriend;
            }
            if (FriendCountLocked(fromUserId) >= limits.MaxFriends)
            {
                return FriendOutcome.MyFriendsFull;
            }
            if (FriendCountLocked(toUserId) >= limits.MaxFriends)
            {
                return FriendOutcome.TheirFriendsFull;
            }
            if (_requests.TryGetValue((toUserId, fromUserId), out FriendRequest? theirs) && IsPending(theirs, nowMs))
            {
                MakeFriends(fromUserId, toUserId, nowMs, changes);
                return FriendOutcome.BecameFriends;
            }
            if (_requests.TryGetValue((fromUserId, toUserId), out FriendRequest? mine) && IsPending(mine, nowMs))
            {
                return FriendOutcome.Ok;
            }

            long today = GameDayHelper.StartOfDay(nowMs);
            if (_requests.Values.Count(r => r.FromUserId == fromUserId && r.SentAt >= today) >= limits.MaxRequestsPerDay)
            {
                return FriendOutcome.RequestDayLimit;
            }
            if (PendingCountLocked(toUserId, nowMs) >= limits.MaxPendingReceived)
            {
                return FriendOutcome.TheirRequestsFull;
            }

            FriendRequest request = mine ?? new FriendRequest { FromUserId = fromUserId, ToUserId = toUserId };
            request.SentAt = nowMs;
            request.Resolved = false;
            _requests[(fromUserId, toUserId)] = request;
            changes.SavedRequests.Add(request);
            return FriendOutcome.Ok;
        });

    /// <summary>Accepts a pending request from <paramref name="fromUserId"/>.</summary>
    public FriendOutcome AcceptRequest(long userId, long fromUserId, long nowMs, FriendLimits limits)
        => Write(nowMs, changes =>
        {
            if (!_requests.TryGetValue((fromUserId, userId), out FriendRequest? request) || !IsPending(request, nowMs))
            {
                return FriendOutcome.RequestNotFound;
            }
            if (AreFriendsLocked(userId, fromUserId))
            {
                Resolve(request, changes);
                return FriendOutcome.Ok;
            }
            if (FriendCountLocked(userId) >= limits.MaxFriends)
            {
                return FriendOutcome.MyFriendsFull;
            }
            if (FriendCountLocked(fromUserId) >= limits.MaxFriends)
            {
                return FriendOutcome.TheirFriendsFull;
            }

            MakeFriends(userId, fromUserId, nowMs, changes);
            return FriendOutcome.Ok;
        });

    /// <summary>Declines the pending requests from these users; ones that aren't pending are skipped.</summary>
    public void DeclineRequests(long userId, IEnumerable<long> fromUserIds, long nowMs)
        => Write(nowMs, changes =>
        {
            foreach (long fromUserId in fromUserIds)
            {
                if (_requests.TryGetValue((fromUserId, userId), out FriendRequest? request) && IsPending(request, nowMs))
                {
                    Resolve(request, changes);
                }
            }
            return FriendOutcome.Ok;
        });

    /// <summary>Ends a friendship. Today's cheers stay, so removing and re-adding a friend doesn't allow a second one.</summary>
    public FriendOutcome RemoveFriend(long userId, long friendUserId, long nowMs)
        => Write(nowMs, changes =>
        {
            if (!AreFriendsLocked(userId, friendUserId))
            {
                return FriendOutcome.NotFriend;
            }

            _friends[userId].Remove(friendUserId);
            _friends[friendUserId].Remove(userId);
            (long low, long high) = Order(userId, friendUserId);
            changes.RemovedLinks.Add(new FriendLink { UserId = low, FriendUserId = high });
            return FriendOutcome.Ok;
        });

    // ---- Cheers ----

    /// <summary>Cheers one friend, once per friend per day.</summary>
    public FriendOutcome Cheer(long fromUserId, long toUserId, long nowMs, FriendLimits limits)
        => Write(nowMs, changes =>
        {
            if (!AreFriendsLocked(fromUserId, toUserId))
            {
                return FriendOutcome.NotFriend;
            }

            long today = GameDayHelper.StartOfDay(nowMs);
            if (_cheers.TryGetValue((fromUserId, toUserId), out FriendCheer? cheer) && cheer.SentAt >= today)
            {
                return FriendOutcome.CheerAlreadySent;
            }
            if (SentCountLocked(fromUserId, today) >= limits.MaxCheersSent)
            {
                return FriendOutcome.CheerSendDayLimit;
            }

            SendCheer(fromUserId, toUserId, nowMs, changes);
            return FriendOutcome.Ok;
        });

    /// <summary>Cheers every friend not cheered today, up to the daily limit. Returns who was cheered.</summary>
    public IReadOnlyList<long> CheerAll(long fromUserId, long nowMs, FriendLimits limits)
    {
        List<long> cheered = [];
        Write(nowMs, changes =>
        {
            long today = GameDayHelper.StartOfDay(nowMs);
            int room = limits.MaxCheersSent - SentCountLocked(fromUserId, today);
            foreach (long friend in _friends.TryGetValue(fromUserId, out HashSet<long>? friends) ? friends : [])
            {
                if (cheered.Count >= room)
                {
                    break;
                }
                if (_cheers.TryGetValue((fromUserId, friend), out FriendCheer? cheer) && cheer.SentAt >= today)
                {
                    continue;
                }

                SendCheer(fromUserId, friend, nowMs, changes);
                cheered.Add(friend);
            }
            return FriendOutcome.Ok;
        });
        return cheered;
    }

    /// <summary>Takes a friend's cheer from today.</summary>
    public FriendOutcome ReceiveCheer(long userId, long fromUserId, long nowMs, FriendLimits limits)
        => Write(nowMs, changes =>
        {
            if (!AreFriendsLocked(userId, fromUserId))
            {
                return FriendOutcome.NotFriend;
            }

            long today = GameDayHelper.StartOfDay(nowMs);
            if (!_cheers.TryGetValue((fromUserId, userId), out FriendCheer? cheer) || cheer.SentAt < today || cheer.ReceivedAt != 0)
            {
                return FriendOutcome.NoCheerToReceive;
            }
            if (ReceivedCountLocked(userId, today) >= limits.MaxCheersReceived)
            {
                return FriendOutcome.CheerReceiveDayLimit;
            }

            cheer.ReceivedAt = nowMs;
            changes.SavedCheers.Add(cheer);
            return FriendOutcome.Ok;
        });

    /// <summary>Takes every friend's cheer from today, up to the daily limit. Returns whose cheers were taken.</summary>
    public IReadOnlyList<long> ReceiveAll(long userId, long nowMs, FriendLimits limits)
    {
        List<long> received = [];
        Write(nowMs, changes =>
        {
            long today = GameDayHelper.StartOfDay(nowMs);
            int room = limits.MaxCheersReceived - ReceivedCountLocked(userId, today);
            foreach (long friend in _friends.TryGetValue(userId, out HashSet<long>? friends) ? friends : [])
            {
                if (received.Count >= room)
                {
                    break;
                }
                if (!_cheers.TryGetValue((friend, userId), out FriendCheer? cheer) || cheer.SentAt < today || cheer.ReceivedAt != 0)
                {
                    continue;
                }

                cheer.ReceivedAt = nowMs;
                changes.SavedCheers.Add(cheer);
                received.Add(friend);
            }
            return FriendOutcome.Ok;
        });
        return received;
    }

    // ---- Internals (callers hold the lock) ----

    /// <summary>Runs one change under the lock, after dropping rows that ran out, and saves what it changed.</summary>
    private FriendOutcome Write(long nowMs, Func<FriendChanges, FriendOutcome> change)
    {
        lock (_lock)
        {
            FriendChanges changes = new();
            Prune(nowMs, changes);
            FriendOutcome outcome = change(changes);
            if (!changes.IsEmpty)
            {
                _persistence?.SaveFriendChanges(changes);
            }
            return outcome;
        }
    }

    /// <summary>Once per game day: drops older cheers, and older requests that are answered or expired.</summary>
    private void Prune(long nowMs, FriendChanges changes)
    {
        long today = GameDayHelper.StartOfDay(nowMs);
        if (_prunedDay == today)
        {
            return;
        }
        _prunedDay = today;

        foreach (((long From, long To) key, FriendCheer cheer) in _cheers.Where(c => c.Value.SentAt < today).ToList())
        {
            _cheers.Remove(key);
            changes.RemovedCheers.Add(cheer);
        }
        foreach (((long From, long To) key, FriendRequest request) in _requests.Where(r => r.Value.SentAt < today && !IsPending(r.Value, nowMs)).ToList())
        {
            _requests.Remove(key);
            changes.RemovedRequests.Add(request);
        }
    }

    private void MakeFriends(long userId, long otherUserId, long nowMs, FriendChanges changes)
    {
        Link(userId, otherUserId);
        (long low, long high) = Order(userId, otherUserId);
        changes.SavedLinks.Add(new FriendLink { UserId = low, FriendUserId = high, CreatedAt = nowMs });

        foreach ((long From, long To) key in new[] { (userId, otherUserId), (otherUserId, userId) })
        {
            if (_requests.TryGetValue(key, out FriendRequest? request) && !request.Resolved)
            {
                Resolve(request, changes);
            }
        }
    }

    private void SendCheer(long fromUserId, long toUserId, long nowMs, FriendChanges changes)
    {
        FriendCheer cheer = _cheers.TryGetValue((fromUserId, toUserId), out FriendCheer? existing)
            ? existing
            : new FriendCheer { FromUserId = fromUserId, ToUserId = toUserId };
        cheer.SentAt = nowMs;
        cheer.ReceivedAt = 0;
        _cheers[(fromUserId, toUserId)] = cheer;
        changes.SavedCheers.Add(cheer);
    }

    private static void Resolve(FriendRequest request, FriendChanges changes)
    {
        request.Resolved = true;
        changes.SavedRequests.Add(request);
    }

    private void Link(long userId, long otherUserId)
    {
        Friends(userId).Add(otherUserId);
        Friends(otherUserId).Add(userId);
    }

    private HashSet<long> Friends(long userId)
    {
        if (!_friends.TryGetValue(userId, out HashSet<long>? friends))
        {
            friends = [];
            _friends[userId] = friends;
        }
        return friends;
    }

    private bool AreFriendsLocked(long userId, long otherUserId)
        => _friends.TryGetValue(userId, out HashSet<long>? friends) && friends.Contains(otherUserId);

    private int FriendCountLocked(long userId)
        => _friends.TryGetValue(userId, out HashSet<long>? friends) ? friends.Count : 0;

    private int PendingCountLocked(long userId, long nowMs)
        => _requests.Values.Count(r => r.ToUserId == userId && IsPending(r, nowMs));

    private int SentCountLocked(long userId, long today)
        => _cheers.Values.Count(c => c.FromUserId == userId && c.SentAt >= today);

    private int ReceivedCountLocked(long userId, long today)
        => _cheers.Values.Count(c => c.ToUserId == userId && c.ReceivedAt >= today);

    private static bool IsPending(FriendRequest request, long nowMs)
        => !request.Resolved && request.SentAt > nowMs - (long)RequestLifetime.TotalMilliseconds;

    private static (long Low, long High) Order(long a, long b) => a < b ? (a, b) : (b, a);
}
