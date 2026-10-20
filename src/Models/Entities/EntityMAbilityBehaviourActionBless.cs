using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMAbilityBehaviourActionBless))]
[PrimaryKey(nameof(AbilityBehaviourActionId))]
public class EntityMAbilityBehaviourActionBless
{
    [Key(0)] public int AbilityBehaviourActionId { get; set; }

    [Key(1)] public int AbilityId { get; set; }

    [Key(2)] public int DecreasePoint { get; set; }
}
