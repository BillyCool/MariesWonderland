using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMQuestDisplayAttributeGroup))]
public class EntityMQuestDisplayAttributeGroup
{
    [Key(0)] public int QuestDisplayAttributeGroupId { get; set; }

    [Key(1)] public int SortOrder { get; set; }

    [Key(2)] public QuestDisplayAttributeType QuestDisplayAttributeType { get; set; }

    [Key(3)] public QuestDisplayAttributeIconSizeType QuestDisplayAttributeIconSizeType { get; set; }
}
