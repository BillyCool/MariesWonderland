using MariesWonderland.Proto.Parts;
using Grpc.Core;

namespace MariesWonderland.Services;

public class PartsService : MariesWonderland.Proto.Parts.PartsService.PartsServiceBase
{
    public override Task<SellResponse> Sell(SellRequest request, ServerCallContext context)
    {
        return Task.FromResult(new SellResponse());
    }

    public override Task<ProtectResponse> Protect(ProtectRequest request, ServerCallContext context)
    {
        return Task.FromResult(new ProtectResponse());
    }

    public override Task<UnprotectResponse> Unprotect(UnprotectRequest request, ServerCallContext context)
    {
        return Task.FromResult(new UnprotectResponse());
    }

    public override Task<EnhanceResponse> Enhance(EnhanceRequest request, ServerCallContext context)
    {
        return Task.FromResult(new EnhanceResponse());
    }

    public override Task<UpdatePresetNameResponse> UpdatePresetName(UpdatePresetNameRequest request, ServerCallContext context)
    {
        return Task.FromResult(new UpdatePresetNameResponse());
    }

    public override Task<UpdatePresetTagNumberResponse> UpdatePresetTagNumber(UpdatePresetTagNumberRequest request, ServerCallContext context)
    {
        return Task.FromResult(new UpdatePresetTagNumberResponse());
    }

    public override Task<UpdatePresetTagNameResponse> UpdatePresetTagName(UpdatePresetTagNameRequest request, ServerCallContext context)
    {
        return Task.FromResult(new UpdatePresetTagNameResponse());
    }

    public override Task<ReplacePresetResponse> ReplacePreset(ReplacePresetRequest request, ServerCallContext context)
    {
        return Task.FromResult(new ReplacePresetResponse());
    }

    public override Task<CopyPresetResponse> CopyPreset(CopyPresetRequest request, ServerCallContext context)
    {
        return Task.FromResult(new CopyPresetResponse());
    }

    public override Task<RemovePresetResponse> RemovePreset(RemovePresetRequest request, ServerCallContext context)
    {
        return Task.FromResult(new RemovePresetResponse());
    }
}
