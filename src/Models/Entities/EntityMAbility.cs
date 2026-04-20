using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMAbility))]
public class EntityMAbility
{
    [Key(0)] public int AbilityId { get; set; }

    [Key(1)] public int AbilityLevelGroupId { get; set; }
}
