using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMAbilityBehaviour))]
public class EntityMAbilityBehaviour
{
    [Key(0)] public int AbilityBehaviourId { get; set; }

    [Key(1)] public AbilityBehaviourType AbilityBehaviourType { get; set; }

    [Key(2)] public int AbilityBehaviourActionId { get; set; }
}
