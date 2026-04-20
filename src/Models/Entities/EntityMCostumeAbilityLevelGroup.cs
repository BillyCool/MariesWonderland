using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCostumeAbilityLevelGroup))]
public class EntityMCostumeAbilityLevelGroup
{
    [Key(0)] public int CostumeAbilityLevelGroupId { get; set; }

    [Key(1)] public int CostumeLimitBreakCountLowerLimit { get; set; }

    [Key(2)] public int AbilityLevel { get; set; }
}
