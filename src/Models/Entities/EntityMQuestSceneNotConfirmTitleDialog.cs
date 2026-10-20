using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMQuestSceneNotConfirmTitleDialog))]
[PrimaryKey(nameof(QuestSceneId))]
public class EntityMQuestSceneNotConfirmTitleDialog
{
    [Key(0)] public int QuestSceneId { get; set; }
}
