using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMPortalCageScene))]
public class EntityMPortalCageScene
{
    [Key(0)] public int PortalCageSceneId { get; set; }

    [Key(1)] public int PortalCageCharacterGroupId { get; set; }

    [Key(2)] public int PortalCageDropId { get; set; }

    [Key(3)] public int PortalCageGateId { get; set; }
}
