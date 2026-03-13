using MariesWonderland.Proto.Notification;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace MariesWonderland.Services;

public class NotificationService : MariesWonderland.Proto.Notification.NotificationService.NotificationServiceBase
{
    public override Task<GetHeaderNotificationResponse> GetHeaderNotification(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new GetHeaderNotificationResponse());
    }
}
