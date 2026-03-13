using MariesWonderland.Proto.GamePlay;
using Grpc.Core;

namespace MariesWonderland.Services;

public class GameplayService : MariesWonderland.Proto.GamePlay.GameplayService.GameplayServiceBase
{
    public override Task<CheckBeforeGamePlayResponse> CheckBeforeGamePlay(CheckBeforeGamePlayRequest request, ServerCallContext context)
    {
        return Task.FromResult(new CheckBeforeGamePlayResponse());
    }
}
