using MariesWonderland.Proto.Character;
using Grpc.Core;

namespace MariesWonderland.Services;

public class CharacterService : MariesWonderland.Proto.Character.CharacterService.CharacterServiceBase
{
    public override Task<RebirthResponse> Rebirth(RebirthRequest request, ServerCallContext context)
    {
        return Task.FromResult(new RebirthResponse());
    }
}
