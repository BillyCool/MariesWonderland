using MariesWonderland.Proto.Costume;
using Grpc.Core;

namespace MariesWonderland.Services;

public class CostumeService : MariesWonderland.Proto.Costume.CostumeService.CostumeServiceBase
{
    public override Task<EnhanceResponse> Enhance(EnhanceRequest request, ServerCallContext context)
    {
        return Task.FromResult(new EnhanceResponse());
    }

    public override Task<LimitBreakResponse> LimitBreak(LimitBreakRequest request, ServerCallContext context)
    {
        return Task.FromResult(new LimitBreakResponse());
    }

    public override Task<AwakenResponse> Awaken(AwakenRequest request, ServerCallContext context)
    {
        return Task.FromResult(new AwakenResponse());
    }

    public override Task<EnhanceActiveSkillResponse> EnhanceActiveSkill(EnhanceActiveSkillRequest request, ServerCallContext context)
    {
        return Task.FromResult(new EnhanceActiveSkillResponse());
    }

    public override Task<RegisterLevelBonusConfirmedResponse> RegisterLevelBonusConfirmed(RegisterLevelBonusConfirmedRequest request, ServerCallContext context)
    {
        return Task.FromResult(new RegisterLevelBonusConfirmedResponse());
    }

    public override Task<UnlockLotteryEffectSlotResponse> UnlockLotteryEffectSlot(UnlockLotteryEffectSlotRequest request, ServerCallContext context)
    {
        return Task.FromResult(new UnlockLotteryEffectSlotResponse());
    }

    public override Task<DrawLotteryEffectResponse> DrawLotteryEffect(DrawLotteryEffectRequest request, ServerCallContext context)
    {
        return Task.FromResult(new DrawLotteryEffectResponse());
    }

    public override Task<ConfirmLotteryEffectResponse> ConfirmLotteryEffect(ConfirmLotteryEffectRequest request, ServerCallContext context)
    {
        return Task.FromResult(new ConfirmLotteryEffectResponse());
    }
}
