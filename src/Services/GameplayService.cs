using Grpc.Core;
using MariesWonderland.Proto.GamePlay;

namespace MariesWonderland.Services;

public class GameplayService : MariesWonderland.Proto.GamePlay.GameplayService.GameplayServiceBase
{
    /// <summary>Performs pre-battle validation. Returns no popups and an empty gacha badge list.</summary>
    public override Task<CheckBeforeGamePlayResponse> CheckBeforeGamePlay(CheckBeforeGamePlayRequest request, ServerCallContext context)
    {
        return Task.FromResult(new CheckBeforeGamePlayResponse
        {
            IsExistUnreadPop = false
        });
    }
}
