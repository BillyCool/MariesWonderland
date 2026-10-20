using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMActorAnimationCategory))]
[PrimaryKey(nameof(ActorAnimationCategoryId))]
public class EntityMActorAnimationCategory
{
    [Key(0)] public int ActorAnimationCategoryId { get; set; }

    [Key(1)] public string Name { get; set; }
}
