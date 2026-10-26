using MariesWonderland.Friends;

namespace MariesWonderland.Tests.Friends;

/// <summary>The friend limits, checked with small limits so they're quick to reach.</summary>
public class FriendStoreTests
{
    // 2026-10-24 12:00 UTC, four hours into the game day.
    private static readonly long Now = new DateTimeOffset(2026, 10, 24, 12, 0, 0, TimeSpan.Zero).ToUnixTimeMilliseconds();
    private static readonly FriendLimits Small = new(MaxFriends: 2, MaxPendingReceived: 2, MaxRequestsPerDay: 2, MaxCheersSent: 1, MaxCheersReceived: 1);

    private static void Befriend(FriendStore store, long a, long b)
    {
        Assert.Equal(FriendOutcome.Ok, store.SendRequest(a, b, Now, Small));
        Assert.Equal(FriendOutcome.Ok, store.AcceptRequest(b, a, Now, Small));
    }

    [Fact]
    public void Requests_StopAtTheDailyLimit_EvenWhenAnswered()
    {
        FriendStore store = new();
        store.SendRequest(1, 2, Now, Small);
        store.SendRequest(1, 3, Now, Small);
        store.DeclineRequests(2, [1], Now);

        Assert.Equal(FriendOutcome.RequestDayLimit, store.SendRequest(1, 4, Now, Small));
        Assert.Equal(FriendOutcome.Ok, store.SendRequest(1, 4, Now + (long)TimeSpan.FromDays(1).TotalMilliseconds, Small));
    }

    [Fact]
    public void Requests_StopWhenTheReceiverHasTooManyPending()
    {
        FriendStore store = new();
        store.SendRequest(2, 1, Now, Small);
        store.SendRequest(3, 1, Now, Small);

        Assert.Equal(FriendOutcome.TheirRequestsFull, store.SendRequest(4, 1, Now, Small));
    }

    [Fact]
    public void FullFriendLists_BlockRequestsAndAccepts()
    {
        FriendStore store = new();
        Befriend(store, 1, 2);
        Befriend(store, 1, 3);
        store.SendRequest(4, 5, Now, Small);

        Assert.Equal(FriendOutcome.MyFriendsFull, store.SendRequest(1, 4, Now, Small));
        Assert.Equal(FriendOutcome.TheirFriendsFull, store.SendRequest(4, 1, Now, Small));

        Befriend(store, 5, 6);
        Befriend(store, 5, 7);
        Assert.Equal(FriendOutcome.MyFriendsFull, store.AcceptRequest(5, 4, Now, Small));
    }

    [Fact]
    public void Cheers_StopAtTheDailyLimits()
    {
        FriendStore store = new();
        Befriend(store, 1, 2);
        Befriend(store, 1, 3);
        Befriend(store, 4, 2);

        Assert.Equal(FriendOutcome.Ok, store.Cheer(1, 2, Now, Small));
        Assert.Equal(FriendOutcome.CheerSendDayLimit, store.Cheer(1, 3, Now, Small));
        Assert.Empty(store.CheerAll(1, Now, Small));

        Assert.Equal(FriendOutcome.Ok, store.Cheer(4, 2, Now, Small));
        Assert.Equal(FriendOutcome.Ok, store.ReceiveCheer(2, 1, Now, Small));
        Assert.Equal(FriendOutcome.CheerReceiveDayLimit, store.ReceiveCheer(2, 4, Now, Small));
    }

    [Fact]
    public void Cheering_SomeoneWhoIsNotAFriend_IsRefused()
    {
        FriendStore store = new();

        Assert.Equal(FriendOutcome.NotFriend, store.Cheer(1, 2, Now, Small));
        Assert.Equal(FriendOutcome.NotFriend, store.ReceiveCheer(1, 2, Now, Small));
    }
}
