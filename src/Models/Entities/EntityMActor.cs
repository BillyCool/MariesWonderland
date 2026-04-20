using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMActor))]
public class EntityMActor
{
    [Key(0)] public int ActorId { get; set; }

    [Key(1)] public int NameActorTextId { get; set; }

    [Key(2)] public string ActorAssetId { get; set; }

    [Key(3)] public string ActorSpeakerIconAssetPath { get; set; }

    [Key(4)] public string AnimatorAssetPath { get; set; }
}
