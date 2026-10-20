using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCostumeAbilityGroup))]
[PrimaryKey(nameof(CostumeAbilityGroupId), nameof(SlotNumber))]
public class EntityMCostumeAbilityGroup
{
    [Key(0)] public int CostumeAbilityGroupId { get; set; }

    [Key(1)] public int SlotNumber { get; set; }

    [Key(2)] public int AbilityId { get; set; }

    [Key(3)] public int CostumeAbilityLevelGroupId { get; set; }
}
