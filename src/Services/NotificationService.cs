using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Proto.Notification;

namespace MariesWonderland.Services;

public class NotificationService(UserDataStore store, TimeProvider timeProvider) : MariesWonderland.Proto.Notification.NotificationService.NotificationServiceBase
{
    private readonly UserDataStore _store = store;
    private readonly TimeProvider _timeProvider = timeProvider;

    /// <summary>Returns notification badge counts for the header bar (unclaimed gifts, friend requests, unread info).</summary>
    public override Task<GetHeaderNotificationResponse> GetHeaderNotification(Empty request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        int giftNotReceiveCount = userDb.EntitySUserGift.Count(g => g.UserId == userId && g.ReceivedDatetime == 0);

        return Task.FromResult(new GetHeaderNotificationResponse
        {
            GiftNotReceiveCount = giftNotReceiveCount,
            FriendRequestReceiveCount = _store.Friends.PendingRequestCount(userId, _timeProvider.GetUtcNow().ToUnixTimeMilliseconds()),
            IsExistUnreadInformation = false
        });
    }
}
