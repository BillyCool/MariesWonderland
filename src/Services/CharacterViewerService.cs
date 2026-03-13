using MariesWonderland.Proto.CharacterViewer;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace MariesWonderland.Services;

public class CharacterViewerService : MariesWonderland.Proto.CharacterViewer.CharacterviewerService.CharacterviewerServiceBase
{
    public override Task<CharacterViewerTopResponse> CharacterViewerTop(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new CharacterViewerTopResponse());
    }
}
