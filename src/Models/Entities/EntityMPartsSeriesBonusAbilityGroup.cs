using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMPartsSeriesBonusAbilityGroup))]
public class EntityMPartsSeriesBonusAbilityGroup
{
    [Key(0)] public int PartsSeriesBonusAbilityGroupId { get; set; }

    [Key(1)] public int SetCount { get; set; }

    [Key(2)] public int AbilityId { get; set; }

    [Key(3)] public int AbilityLevel { get; set; }
}
