using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMAbilityBehaviourGroup))]
[PrimaryKey(nameof(AbilityBehaviourGroupId), nameof(AbilityBehaviourIndex))]
public class EntityMAbilityBehaviourGroup
{
    [Key(0)] public int AbilityBehaviourGroupId { get; set; }

    [Key(1)] public int AbilityBehaviourIndex { get; set; }

    [Key(2)] public int AbilityBehaviourId { get; set; }
}
