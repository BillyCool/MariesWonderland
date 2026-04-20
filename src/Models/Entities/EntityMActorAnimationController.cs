using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMActorAnimationController))]
public class EntityMActorAnimationController
{
    [Key(0)] public int ActorAnimationControllerId { get; set; }

    [Key(1)] public int ActorId { get; set; }

    [Key(2)] public int ActorAnimationControllerType { get; set; }

    [Key(3)] public string AssetPath { get; set; }
}
