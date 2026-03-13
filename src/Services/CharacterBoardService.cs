using MariesWonderland.Proto.CharacterBoard;
using Grpc.Core;

namespace MariesWonderland.Services;

public class CharacterBoardService : MariesWonderland.Proto.CharacterBoard.CharacterboardService.CharacterboardServiceBase
{
    public override Task<ReleasePanelResponse> ReleasePanel(ReleasePanelRequest request, ServerCallContext context)
    {
        return Task.FromResult(new ReleasePanelResponse());
    }
}
