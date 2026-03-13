using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMCostumeRarity
{
    public RarityType RarityType { get; set; }

    public int CostumeLimitBreakMaterialRarityGroupId { get; set; }

    public int EnhancementCostByMaterialNumericalFunctionId { get; set; }

    public int LimitBreakCostNumericalFunctionId { get; set; }

    public int MaxLevelNumericalFunctionId { get; set; }

    public int RequiredExpForLevelUpNumericalParameterMapId { get; set; }

    public int ActiveSkillMaxLevelNumericalFunctionId { get; set; }

    public int ActiveSkillEnhancementCostNumericalFunctionId { get; set; }
}
