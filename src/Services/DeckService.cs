using MariesWonderland.Proto.Deck;
using Grpc.Core;

namespace MariesWonderland.Services;

public class DeckService : MariesWonderland.Proto.Deck.DeckService.DeckServiceBase
{
    public override Task<UpdateNameResponse> UpdateName(UpdateNameRequest request, ServerCallContext context)
    {
        return Task.FromResult(new UpdateNameResponse());
    }

    public override Task<ReplaceDeckResponse> ReplaceDeck(ReplaceDeckRequest request, ServerCallContext context)
    {
        return Task.FromResult(new ReplaceDeckResponse());
    }

    public override Task<SetPvpDefenseDeckResponse> SetPvpDefenseDeck(SetPvpDefenseDeckRequest request, ServerCallContext context)
    {
        return Task.FromResult(new SetPvpDefenseDeckResponse());
    }

    public override Task<CopyDeckResponse> CopyDeck(CopyDeckRequest request, ServerCallContext context)
    {
        return Task.FromResult(new CopyDeckResponse());
    }

    public override Task<RemoveDeckResponse> RemoveDeck(RemoveDeckRequest request, ServerCallContext context)
    {
        return Task.FromResult(new RemoveDeckResponse());
    }

    public override Task<RefreshDeckPowerResponse> RefreshDeckPower(RefreshDeckPowerRequest request, ServerCallContext context)
    {
        return Task.FromResult(new RefreshDeckPowerResponse());
    }

    public override Task<UpdateTripleDeckNameResponse> UpdateTripleDeckName(UpdateTripleDeckNameRequest request, ServerCallContext context)
    {
        return Task.FromResult(new UpdateTripleDeckNameResponse());
    }

    public override Task<ReplaceTripleDeckResponse> ReplaceTripleDeck(ReplaceTripleDeckRequest request, ServerCallContext context)
    {
        return Task.FromResult(new ReplaceTripleDeckResponse());
    }

    public override Task<ReplaceMultiDeckResponse> ReplaceMultiDeck(ReplaceMultiDeckRequest request, ServerCallContext context)
    {
        return Task.FromResult(new ReplaceMultiDeckResponse());
    }

    public override Task<RefreshMultiDeckPowerResponse> RefreshMultiDeckPower(RefreshMultiDeckPowerRequest request, ServerCallContext context)
    {
        return Task.FromResult(new RefreshMultiDeckPowerResponse());
    }
}
