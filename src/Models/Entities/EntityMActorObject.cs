using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMActorObject))]
[PrimaryKey(nameof(ActorObjectId))]
public class EntityMActorObject
{
    [Key(0)] public int ActorObjectId { get; set; }

    [Key(1)] public int ActorId { get; set; }
}
