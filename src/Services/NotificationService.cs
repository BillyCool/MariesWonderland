using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Proto.Notification;

namespace MariesWonderland.Services;

public class NotificationService(UserDataStore store) : MariesWonderland.Proto.Notification.NotificationService.NotificationServiceBase
{
    private readonly UserDataStore _store = store;

    /// <summary>Returns notification badge counts for the header bar (unclaimed gifts, friend requests, unread info).</summary>
    public override Task<GetHeaderNotificationResponse> GetHeaderNotification(Empty request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        int giftNotReceiveCount = userDb.EntitySUserGift.Count(g => g.ReceivedDatetime == 0);

        return Task.FromResult(new GetHeaderNotificationResponse
        {
            GiftNotReceiveCount = giftNotReceiveCount,
            FriendRequestReceiveCount = 0,
            IsExistUnreadInformation = false
        });
    }
}
