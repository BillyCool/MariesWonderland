using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMAbilityBehaviour))]
[PrimaryKey(nameof(AbilityBehaviourId))]
public class EntityMAbilityBehaviour
{
    [Key(0)] public int AbilityBehaviourId { get; set; }

    [Key(1)] public AbilityBehaviourType AbilityBehaviourType { get; set; }

    [Key(2)] public int AbilityBehaviourActionId { get; set; }
}
