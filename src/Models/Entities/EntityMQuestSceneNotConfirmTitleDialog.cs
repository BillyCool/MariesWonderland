using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMQuestSceneNotConfirmTitleDialog))]
public class EntityMQuestSceneNotConfirmTitleDialog
{
    [Key(0)] public int QuestSceneId { get; set; }
}
