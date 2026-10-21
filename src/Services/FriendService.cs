using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Missions;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Friend;

namespace MariesWonderland.Services;

public class FriendService(UserDataStore store, MissionTracker missions, TimeProvider timeProvider) : MariesWonderland.Proto.Friend.FriendService.FriendServiceBase
{
    private readonly UserDataStore _store = store;
    private readonly MissionTracker _missions = missions;
    private readonly TimeProvider _timeProvider = timeProvider;

    /// <summary>Returns an empty response. Friend lookup not yet implemented.</summary>
    public override Task<GetUserResponse> GetUser(GetUserRequest request, ServerCallContext context)
    {
        return Task.FromResult(new GetUserResponse());
    }

    /// <summary>Returns an empty response. Friend recommendations not yet implemented.</summary>
    public override Task<SearchRecommendedUsersResponse> SearchRecommendedUsers(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new SearchRecommendedUsersResponse());
    }

    /// <summary>Returns an empty response (friend list not yet implemented), and takes the Cage values measured since the last report.</summary>
    public override Task<GetFriendListResponse> GetFriendList(GetFriendListRequest request, ServerCallContext context)
    {
        if (request.CageMeasurableValues is { } cage)
        {
            DarkUserMemoryDatabase userDb = _store.GetOrCreate(context.GetUserId());
            _missions.RecordCageValues(userDb, cage.RunningDistanceMeters, cage.MamaTappedCount, _timeProvider.GetUtcNow().ToUnixTimeMilliseconds());
        }

        return Task.FromResult(new GetFriendListResponse());
    }

    /// <summary>Returns an empty response. Friend requests not yet implemented.</summary>
    public override Task<GetFriendRequestListResponse> GetFriendRequestList(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new GetFriendRequestListResponse());
    }

    /// <summary>Returns an empty response. Sending friend requests not yet implemented.</summary>
    public override Task<SendFriendRequestResponse> SendFriendRequest(SendFriendRequestRequest request, ServerCallContext context)
    {
        return Task.FromResult(new SendFriendRequestResponse());
    }

    /// <summary>Returns an empty response. Accepting friend requests not yet implemented.</summary>
    public override Task<AcceptFriendRequestResponse> AcceptFriendRequest(AcceptFriendRequestRequest request, ServerCallContext context)
    {
        return Task.FromResult(new AcceptFriendRequestResponse());
    }

    /// <summary>Returns an empty response. Declining friend requests not yet implemented.</summary>
    public override Task<DeclineFriendRequestResponse> DeclineFriendRequest(DeclineFriendRequestRequest request, ServerCallContext context)
    {
        return Task.FromResult(new DeclineFriendRequestResponse());
    }

    /// <summary>Returns an empty response. Friend removal not yet implemented.</summary>
    public override Task<DeleteFriendResponse> DeleteFriend(DeleteFriendRequest request, ServerCallContext context)
    {
        return Task.FromResult(new DeleteFriendResponse());
    }

    /// <summary>Counts the cheer for the friend-support missions; nothing else (friends aren't implemented yet).</summary>
    public override Task<CheerFriendResponse> CheerFriend(CheerFriendRequest request, ServerCallContext context)
    {
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(context.GetUserId());
        _missions.Record(userDb, MissionClearConditionType.CHEER_FRIEND_BY_COUNT, 1, _timeProvider.GetUtcNow().ToUnixTimeMilliseconds());
        return Task.FromResult(new CheerFriendResponse());
    }

    /// <summary>Returns an empty response. Bulk cheering not yet implemented.</summary>
    public override Task<BulkCheerFriendResponse> BulkCheerFriend(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new BulkCheerFriendResponse());
    }

    /// <summary>Returns an empty response. Receiving cheers not yet implemented.</summary>
    public override Task<ReceiveCheerResponse> ReceiveCheer(ReceiveCheerRequest request, ServerCallContext context)
    {
        return Task.FromResult(new ReceiveCheerResponse());
    }

    /// <summary>Returns an empty response. Bulk receiving cheers not yet implemented.</summary>
    public override Task<BulkReceiveCheerResponse> BulkReceiveCheer(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new BulkReceiveCheerResponse());
    }
}
