using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMAbilityLevelGroup))]
[PrimaryKey(nameof(AbilityLevelGroupId), nameof(LevelLowerLimit))]
public class EntityMAbilityLevelGroup
{
    [Key(0)] public int AbilityLevelGroupId { get; set; }

    [Key(1)] public int LevelLowerLimit { get; set; }

    [Key(2)] public int AbilityDetailId { get; set; }
}
