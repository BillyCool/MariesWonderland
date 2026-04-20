using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMPartsLevelUpRateGroup))]
public class EntityMPartsLevelUpRateGroup
{
    [Key(0)] public int PartsLevelUpRateGroupId { get; set; }

    [Key(1)] public int LevelLowerLimit { get; set; }

    [Key(2)] public int SuccessRatePermil { get; set; }
}
