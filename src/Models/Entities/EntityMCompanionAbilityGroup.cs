using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCompanionAbilityGroup))]
public class EntityMCompanionAbilityGroup
{
    [Key(0)] public int CompanionAbilityGroupId { get; set; }

    [Key(1)] public int SlotNumber { get; set; }

    [Key(2)] public int AbilityId { get; set; }
}
