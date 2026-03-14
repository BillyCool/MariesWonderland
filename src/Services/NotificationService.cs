using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MariesWonderland.Proto.Notification;

namespace MariesWonderland.Services;

public class NotificationService : MariesWonderland.Proto.Notification.NotificationService.NotificationServiceBase
{
    public override Task<GetHeaderNotificationResponse> GetHeaderNotification(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new GetHeaderNotificationResponse()
        {
            FriendRequestReceiveCount = 0,
            GiftNotReceiveCount = 0,
            IsExistUnreadInformation = false
        });
    }
}
