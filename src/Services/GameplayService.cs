using Art.Framework.ApiNetwork.Grpc.Api.GamePlay;
using Grpc.Core;

namespace MariesWonderland.Services;

public class GameplayService : Art.Framework.ApiNetwork.Grpc.Api.GamePlay.GameplayService.GameplayServiceBase
{
    public override Task<CheckBeforeGamePlayResponse> CheckBeforeGamePlay(CheckBeforeGamePlayRequest request, ServerCallContext context)
    {
        return Task.FromResult(new CheckBeforeGamePlayResponse());
    }
}
