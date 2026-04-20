using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMWeaponSpecificEnhance))]
public class EntityMWeaponSpecificEnhance
{
    [Key(0)] public int WeaponSpecificEnhanceId { get; set; }

    [Key(1)] public int BaseEnhancementObtainedExp { get; set; }

    [Key(2)] public int SellPriceNumericalFunctionId { get; set; }

    [Key(3)] public int MaxLevelNumericalFunctionId { get; set; }

    [Key(4)] public int MaxSkillLevelNumericalFunctionId { get; set; }

    [Key(5)] public int MaxAbilityLevelNumericalFunctionId { get; set; }

    [Key(6)] public int RequiredExpForLevelUpNumericalParameterMapId { get; set; }

    [Key(7)] public int EnhancementCostByWeaponNumericalFunctionId { get; set; }

    [Key(8)] public int EnhancementCostByMaterialNumericalFunctionId { get; set; }

    [Key(9)] public int SkillEnhancementCostNumericalFunctionId { get; set; }

    [Key(10)] public int AbilityEnhancementCostNumericalFunctionId { get; set; }

    [Key(11)] public int LimitBreakCostByWeaponNumericalFunctionId { get; set; }

    [Key(12)] public int LimitBreakCostByMaterialNumericalFunctionId { get; set; }

    [Key(13)] public int EvolutionCostNumericalFunctionId { get; set; }
}
