using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMQuestSceneChoiceCostumeEffectGroup))]
[PrimaryKey(nameof(QuestSceneChoiceCostumeEffectGroupId), nameof(SortOrder))]
public class EntityMQuestSceneChoiceCostumeEffectGroup
{
    [Key(0)] public int QuestSceneChoiceCostumeEffectGroupId { get; set; }

    [Key(1)] public int SortOrder { get; set; }

    [Key(2)] public int CostumeId { get; set; }
}
