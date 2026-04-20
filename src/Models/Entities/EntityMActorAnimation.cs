using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMActorAnimation))]
public class EntityMActorAnimation
{
    [Key(0)] public int ActorAnimationId { get; set; }

    [Key(1)] public int ActorId { get; set; }

    [Key(2)] public int ActorAnimationCategoryId { get; set; }

    [Key(3)] public int ActorAnimationType { get; set; }

    [Key(4)] public string AssetPath { get; set; }

    [Key(5)] public bool IsDefault { get; set; }
}
