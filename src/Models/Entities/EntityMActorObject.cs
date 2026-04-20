using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMActorObject))]
public class EntityMActorObject
{
    [Key(0)] public int ActorObjectId { get; set; }

    [Key(1)] public int ActorId { get; set; }
}
