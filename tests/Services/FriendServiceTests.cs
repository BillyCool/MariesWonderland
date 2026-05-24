using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MariesWonderland.Proto.Data;
using MariesWonderland.Proto.Friend;
using MariesWonderland.Tests.Infrastructure;
using FriendServiceImpl = MariesWonderland.Services.FriendService;

namespace MariesWonderland.Tests.Services;

public class FriendServiceTests
{
    private const long UserId = 1L;

    [Fact]
    public async Task GetUser_ReturnsFreshEmptyResponse()
    {
        await AssertFreshEmptyResponseAsync(
            (service, context) => service.GetUser(
                new GetUserRequest { PlayerId = 12345L },
                context),
            response =>
            {
                Assert.Null(response.User);
                Assert.Empty(response.DiffUserData);
            },
            response =>
            {
                response.User = new User { PlayerId = 98765L, UserName = "Patched" };
                response.DiffUserData.Add("patched", new DiffData());
            });
    }

    [Fact]
    public async Task SearchRecommendedUsers_ReturnsFreshEmptyResponse()
    {
        await AssertFreshEmptyResponseAsync(
            (service, context) => service.SearchRecommendedUsers(new Empty(), context),
            response =>
            {
                Assert.Empty(response.Users);
                Assert.Empty(response.DiffUserData);
            },
            response =>
            {
                response.Users.Add(new User { PlayerId = 10L, UserName = "Patched" });
                response.DiffUserData.Add("patched", new DiffData());
            });
    }

    [Fact]
    public async Task GetFriendList_ReturnsEmptyCountsAndFreshResponse()
    {
        await AssertFreshEmptyResponseAsync(
            (service, context) => service.GetFriendList(
                 new GetFriendListRequest
                 {
                     CageMeasurableValues = new Proto.Mission.CageMeasurableValues
                     {
                        RunningDistanceMeters = 99,
                        MamaTappedCount = 100
                     }
                 },
                 context),
            response =>
            {
                Assert.Empty(response.FriendUser);
                Assert.Equal(0, response.SendCheerCount);
                Assert.Equal(0, response.ReceivedCheerCount);
                Assert.Empty(response.DiffUserData);
            },
            response =>
            {
                response.FriendUser.Add(new FriendUser { PlayerId = 44L, UserName = "Patched" });
                response.SendCheerCount = 1;
                response.ReceivedCheerCount = 2;
                response.DiffUserData.Add("patched", new DiffData());
            });
    }

    [Fact]
    public async Task GetFriendRequestList_ReturnsFreshEmptyResponse()
    {
        await AssertFreshEmptyResponseAsync(
            (service, context) => service.GetFriendRequestList(new Empty(), context),
            response =>
            {
                Assert.Empty(response.User);
                Assert.Empty(response.DiffUserData);
            },
            response =>
            {
                response.User.Add(new User { PlayerId = 11L, UserName = "Patched" });
                response.DiffUserData.Add("patched", new DiffData());
            });
    }

    [Fact]
    public async Task FriendRequestAndCheerMutations_ReturnFreshEmptyResponses()
    {
        FriendServiceImpl service = new();

        await AssertDiffOnlyResponseAsync(
            () => service.SendFriendRequest(
                new SendFriendRequestRequest { PlayerId = 1001L },
                FakeServerCallContext.For(UserId)),
            () => service.SendFriendRequest(
                new SendFriendRequestRequest { PlayerId = 2002L },
                FakeServerCallContext.For(999L)));

        await AssertDiffOnlyResponseAsync(
            () => service.AcceptFriendRequest(
                new AcceptFriendRequestRequest { PlayerId = 1001L },
                FakeServerCallContext.For(UserId)),
            () => service.AcceptFriendRequest(
                new AcceptFriendRequestRequest { PlayerId = 2002L },
                FakeServerCallContext.For(999L)));

        await AssertDiffOnlyResponseAsync(
            () => service.DeclineFriendRequest(
                new DeclineFriendRequestRequest { PlayerIdOld = 1001L, PlayerId = { 2002L, 3003L } },
                FakeServerCallContext.For(UserId)),
            () => service.DeclineFriendRequest(
                new DeclineFriendRequestRequest { PlayerIdOld = 2002L, PlayerId = { 4004L } },
                FakeServerCallContext.For(999L)));

        await AssertDiffOnlyResponseAsync(
            () => service.DeleteFriend(
                new DeleteFriendRequest { PlayerId = 1001L },
                FakeServerCallContext.For(UserId)),
            () => service.DeleteFriend(
                new DeleteFriendRequest { PlayerId = 2002L },
                FakeServerCallContext.For(999L)));

        await AssertDiffOnlyResponseAsync(
            () => service.CheerFriend(
                new CheerFriendRequest { PlayerId = 1001L },
                FakeServerCallContext.For(UserId)),
            () => service.CheerFriend(
                new CheerFriendRequest { PlayerId = 2002L },
                FakeServerCallContext.For(999L)));

        await AssertDiffOnlyResponseAsync(
            () => service.ReceiveCheer(
                new ReceiveCheerRequest { PlayerId = 1001L },
                FakeServerCallContext.For(UserId)),
            () => service.ReceiveCheer(
                new ReceiveCheerRequest { PlayerId = 2002L },
                FakeServerCallContext.For(999L)));
    }

    [Fact]
    public async Task BulkCheerAndReceive_ReturnFreshEmptyResponses()
    {
        FriendServiceImpl service = new();

        BulkCheerFriendResponse firstCheer = await service.BulkCheerFriend(new Empty(), FakeServerCallContext.For(UserId));
        Assert.Empty(firstCheer.PlayerId);
        Assert.Empty(firstCheer.DiffUserData);
        firstCheer.PlayerId.Add(123L);
        firstCheer.DiffUserData.Add("patched", new DiffData());

        BulkCheerFriendResponse secondCheer = await service.BulkCheerFriend(new Empty(), FakeServerCallContext.For(999L));
        Assert.NotSame(firstCheer, secondCheer);
        Assert.Empty(secondCheer.PlayerId);
        Assert.Empty(secondCheer.DiffUserData);

        BulkReceiveCheerResponse firstReceive = await service.BulkReceiveCheer(new Empty(), FakeServerCallContext.For(UserId));
        Assert.Empty(firstReceive.PlayerId);
        Assert.Empty(firstReceive.DiffUserData);
        firstReceive.PlayerId.Add(456L);
        firstReceive.DiffUserData.Add("patched", new DiffData());

        BulkReceiveCheerResponse secondReceive = await service.BulkReceiveCheer(new Empty(), FakeServerCallContext.For(999L));
        Assert.NotSame(firstReceive, secondReceive);
        Assert.Empty(secondReceive.PlayerId);
        Assert.Empty(secondReceive.DiffUserData);
    }

    private static async Task AssertFreshEmptyResponseAsync<TResponse>(
        Func<FriendServiceImpl, ServerCallContext, Task<TResponse>> invoke,
        Action<TResponse> assertEmpty,
        Action<TResponse> mutate)
        where TResponse : class
    {
        FriendServiceImpl service = new();

        TResponse firstResponse = await invoke(service, FakeServerCallContext.For(UserId));
        Assert.NotNull(firstResponse);
        assertEmpty(firstResponse);
        mutate(firstResponse);

        TResponse secondResponse = await invoke(service, FakeServerCallContext.For(999L));
        Assert.NotSame(firstResponse, secondResponse);
        assertEmpty(secondResponse);
    }

    private static async Task AssertDiffOnlyResponseAsync<TResponse>(
        Func<Task<TResponse>> firstCall,
        Func<Task<TResponse>> secondCall)
        where TResponse : class
    {
        TResponse firstResponse = await firstCall();
        Assert.NotNull(firstResponse);

        switch (firstResponse)
        {
            case SendFriendRequestResponse send:
                Assert.Empty(send.DiffUserData);
                send.DiffUserData.Add("patched", new DiffData());
                break;
            case AcceptFriendRequestResponse accept:
                Assert.Empty(accept.DiffUserData);
                accept.DiffUserData.Add("patched", new DiffData());
                break;
            case DeclineFriendRequestResponse decline:
                Assert.Empty(decline.DiffUserData);
                decline.DiffUserData.Add("patched", new DiffData());
                break;
            case DeleteFriendResponse delete:
                Assert.Empty(delete.DiffUserData);
                delete.DiffUserData.Add("patched", new DiffData());
                break;
            case CheerFriendResponse cheer:
                Assert.Empty(cheer.DiffUserData);
                cheer.DiffUserData.Add("patched", new DiffData());
                break;
            case ReceiveCheerResponse receive:
                Assert.Empty(receive.DiffUserData);
                receive.DiffUserData.Add("patched", new DiffData());
                break;
            default:
                throw new InvalidOperationException($"Unexpected response type: {typeof(TResponse).Name}");
        }

        TResponse secondResponse = await secondCall();
        Assert.NotSame(firstResponse, secondResponse);

        switch (secondResponse)
        {
            case SendFriendRequestResponse send:
                Assert.Empty(send.DiffUserData);
                break;
            case AcceptFriendRequestResponse accept:
                Assert.Empty(accept.DiffUserData);
                break;
            case DeclineFriendRequestResponse decline:
                Assert.Empty(decline.DiffUserData);
                break;
            case DeleteFriendResponse delete:
                Assert.Empty(delete.DiffUserData);
                break;
            case CheerFriendResponse cheer:
                Assert.Empty(cheer.DiffUserData);
                break;
            case ReceiveCheerResponse receive:
                Assert.Empty(receive.DiffUserData);
                break;
            default:
                throw new InvalidOperationException($"Unexpected response type: {typeof(TResponse).Name}");
        }
    }
}
