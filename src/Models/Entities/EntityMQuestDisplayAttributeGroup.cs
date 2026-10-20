using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMQuestDisplayAttributeGroup))]
[PrimaryKey(nameof(QuestDisplayAttributeGroupId), nameof(SortOrder))]
public class EntityMQuestDisplayAttributeGroup
{
    [Key(0)] public int QuestDisplayAttributeGroupId { get; set; }

    [Key(1)] public int SortOrder { get; set; }

    [Key(2)] public QuestDisplayAttributeType QuestDisplayAttributeType { get; set; }

    [Key(3)] public QuestDisplayAttributeIconSizeType QuestDisplayAttributeIconSizeType { get; set; }
}
