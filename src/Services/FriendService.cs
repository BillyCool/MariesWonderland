using MariesWonderland.Proto.Friend;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace MariesWonderland.Services;

public class FriendService : MariesWonderland.Proto.Friend.FriendService.FriendServiceBase
{
    public override Task<GetUserResponse> GetUser(GetUserRequest request, ServerCallContext context)
    {
        return Task.FromResult(new GetUserResponse());
    }

    public override Task<SearchRecommendedUsersResponse> SearchRecommendedUsers(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new SearchRecommendedUsersResponse());
    }

    public override Task<GetFriendListResponse> GetFriendList(GetFriendListRequest request, ServerCallContext context)
    {
        return Task.FromResult(new GetFriendListResponse());
    }

    public override Task<GetFriendRequestListResponse> GetFriendRequestList(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new GetFriendRequestListResponse());
    }

    public override Task<SendFriendRequestResponse> SendFriendRequest(SendFriendRequestRequest request, ServerCallContext context)
    {
        return Task.FromResult(new SendFriendRequestResponse());
    }

    public override Task<AcceptFriendRequestResponse> AcceptFriendRequest(AcceptFriendRequestRequest request, ServerCallContext context)
    {
        return Task.FromResult(new AcceptFriendRequestResponse());
    }

    public override Task<DeclineFriendRequestResponse> DeclineFriendRequest(DeclineFriendRequestRequest request, ServerCallContext context)
    {
        return Task.FromResult(new DeclineFriendRequestResponse());
    }

    public override Task<DeleteFriendResponse> DeleteFriend(DeleteFriendRequest request, ServerCallContext context)
    {
        return Task.FromResult(new DeleteFriendResponse());
    }

    public override Task<CheerFriendResponse> CheerFriend(CheerFriendRequest request, ServerCallContext context)
    {
        return Task.FromResult(new CheerFriendResponse());
    }

    public override Task<BulkCheerFriendResponse> BulkCheerFriend(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new BulkCheerFriendResponse());
    }

    public override Task<ReceiveCheerResponse> ReceiveCheer(ReceiveCheerRequest request, ServerCallContext context)
    {
        return Task.FromResult(new ReceiveCheerResponse());
    }

    public override Task<BulkReceiveCheerResponse> BulkReceiveCheer(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new BulkReceiveCheerResponse());
    }
}
