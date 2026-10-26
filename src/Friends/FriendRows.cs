namespace MariesWonderland.Friends;

/// <summary>A friendship. One row per pair, the lower user id first.</summary>
public sealed class FriendLink
{
    public long UserId { get; set; }
    public long FriendUserId { get; set; }
    public long CreatedAt { get; set; }
}

/// <summary>
/// A friend request. A resolved one (accepted or declined) is kept until the next game day so it still counts
/// toward the sender's daily limit; a pending one drops off after <see cref="FriendStore.RequestLifetime"/>.
/// </summary>
public sealed class FriendRequest
{
    public long FromUserId { get; set; }
    public long ToUserId { get; set; }
    public long SentAt { get; set; }
    public bool Resolved { get; set; }
}

/// <summary>A cheer (support) from one friend to another. Only today's count; <c>ReceivedAt</c> is 0 until taken.</summary>
public sealed class FriendCheer
{
    public long FromUserId { get; set; }
    public long ToUserId { get; set; }
    public long SentAt { get; set; }
    public long ReceivedAt { get; set; }
}

/// <summary>Everything saved about friends, as loaded at startup.</summary>
public sealed record FriendState(IReadOnlyList<FriendLink> Links, IReadOnlyList<FriendRequest> Requests, IReadOnlyList<FriendCheer> Cheers);

/// <summary>The rows one friend operation adds or changes (saved) and removes, written in one transaction.</summary>
public sealed class FriendChanges
{
    public List<FriendLink> SavedLinks { get; } = [];
    public List<FriendLink> RemovedLinks { get; } = [];
    public List<FriendRequest> SavedRequests { get; } = [];
    public List<FriendRequest> RemovedRequests { get; } = [];
    public List<FriendCheer> SavedCheers { get; } = [];
    public List<FriendCheer> RemovedCheers { get; } = [];

    public bool IsEmpty => SavedLinks.Count == 0 && RemovedLinks.Count == 0 && SavedRequests.Count == 0
        && RemovedRequests.Count == 0 && SavedCheers.Count == 0 && RemovedCheers.Count == 0;
}
