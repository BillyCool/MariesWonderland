using MariesWonderland.Proto.LoginBonus;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace MariesWonderland.Services;

public class LoginBonusService : MariesWonderland.Proto.LoginBonus.LoginbonusService.LoginbonusServiceBase
{
    public override Task<ReceiveStampResponse> ReceiveStamp(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new ReceiveStampResponse());
    }
}
