using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMMainQuestPortalCageCharacter))]
public class EntityMMainQuestPortalCageCharacter
{
    [Key(0)] public int QuestSceneId { get; set; }

    [Key(1)] public int PortalCageCharacterGroupId { get; set; }
}
