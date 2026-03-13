using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMWeaponSpecificEnhance
{
    public int WeaponSpecificEnhanceId { get; set; }

    public int BaseEnhancementObtainedExp { get; set; }

    public int SellPriceNumericalFunctionId { get; set; }

    public int MaxLevelNumericalFunctionId { get; set; }

    public int MaxSkillLevelNumericalFunctionId { get; set; }

    public int MaxAbilityLevelNumericalFunctionId { get; set; }

    public int RequiredExpForLevelUpNumericalParameterMapId { get; set; }

    public int EnhancementCostByWeaponNumericalFunctionId { get; set; }

    public int EnhancementCostByMaterialNumericalFunctionId { get; set; }

    public int SkillEnhancementCostNumericalFunctionId { get; set; }

    public int AbilityEnhancementCostNumericalFunctionId { get; set; }

    public int LimitBreakCostByWeaponNumericalFunctionId { get; set; }

    public int LimitBreakCostByMaterialNumericalFunctionId { get; set; }

    public int EvolutionCostNumericalFunctionId { get; set; }
}
