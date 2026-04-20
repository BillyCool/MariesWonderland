using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMAbilityBehaviourActionBless))]
public class EntityMAbilityBehaviourActionBless
{
    [Key(0)] public int AbilityBehaviourActionId { get; set; }

    [Key(1)] public int AbilityId { get; set; }

    [Key(2)] public int DecreasePoint { get; set; }
}
