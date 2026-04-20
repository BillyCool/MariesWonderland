using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCostumeRarity))]
public class EntityMCostumeRarity
{
    [Key(0)] public RarityType RarityType { get; set; }

    [Key(1)] public int CostumeLimitBreakMaterialRarityGroupId { get; set; }

    [Key(2)] public int EnhancementCostByMaterialNumericalFunctionId { get; set; }

    [Key(3)] public int LimitBreakCostNumericalFunctionId { get; set; }

    [Key(4)] public int MaxLevelNumericalFunctionId { get; set; }

    [Key(5)] public int RequiredExpForLevelUpNumericalParameterMapId { get; set; }

    [Key(6)] public int ActiveSkillMaxLevelNumericalFunctionId { get; set; }

    [Key(7)] public int ActiveSkillEnhancementCostNumericalFunctionId { get; set; }
}
