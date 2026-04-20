using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMPartsLevelUpPriceGroup))]
public class EntityMPartsLevelUpPriceGroup
{
    [Key(0)] public int PartsLevelUpPriceGroupId { get; set; }

    [Key(1)] public int LevelLowerLimit { get; set; }

    [Key(2)] public int Gold { get; set; }
}
