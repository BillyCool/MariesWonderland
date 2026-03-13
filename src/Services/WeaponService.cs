using MariesWonderland.Proto.Weapon;
using Grpc.Core;

namespace MariesWonderland.Services;

public class WeaponService : MariesWonderland.Proto.Weapon.WeaponService.WeaponServiceBase
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

    public override Task<EnhanceByWeaponResponse> EnhanceByWeapon(EnhanceByWeaponRequest request, ServerCallContext context)
    {
        return Task.FromResult(new EnhanceByWeaponResponse());
    }

    public override Task<EnhanceByMaterialResponse> EnhanceByMaterial(EnhanceByMaterialRequest request, ServerCallContext context)
    {
        return Task.FromResult(new EnhanceByMaterialResponse());
    }

    public override Task<EnhanceSkillResponse> EnhanceSkill(EnhanceSkillRequest request, ServerCallContext context)
    {
        return Task.FromResult(new EnhanceSkillResponse());
    }

    public override Task<EnhanceAbilityResponse> EnhanceAbility(EnhanceAbilityRequest request, ServerCallContext context)
    {
        return Task.FromResult(new EnhanceAbilityResponse());
    }

    public override Task<LimitBreakByWeaponResponse> LimitBreakByWeapon(LimitBreakByWeaponRequest request, ServerCallContext context)
    {
        return Task.FromResult(new LimitBreakByWeaponResponse());
    }

    public override Task<LimitBreakByMaterialResponse> LimitBreakByMaterial(LimitBreakByMaterialRequest request, ServerCallContext context)
    {
        return Task.FromResult(new LimitBreakByMaterialResponse());
    }

    public override Task<EvolveResponse> Evolve(EvolveRequest request, ServerCallContext context)
    {
        return Task.FromResult(new EvolveResponse());
    }

    public override Task<AwakenResponse> Awaken(AwakenRequest request, ServerCallContext context)
    {
        return Task.FromResult(new AwakenResponse());
    }
}
