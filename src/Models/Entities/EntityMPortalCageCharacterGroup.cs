using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMPortalCageCharacterGroup))]
public class EntityMPortalCageCharacterGroup
{
    [Key(0)] public int PortalCageCharacterGroupId { get; set; }

    [Key(1)] public int PlayerCharacterActorObjectId { get; set; }

    [Key(2)] public int NaviCharacterActorObjectId { get; set; }

    [Key(3)] public int NaviMenuActorObjectId { get; set; }

    [Key(4)] public TutorialType TutorialType { get; set; }
}
