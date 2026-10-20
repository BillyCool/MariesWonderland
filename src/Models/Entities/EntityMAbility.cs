using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMAbility))]
[PrimaryKey(nameof(AbilityId))]
public class EntityMAbility
{
    [Key(0)] public int AbilityId { get; set; }

    [Key(1)] public int AbilityLevelGroupId { get; set; }
}
