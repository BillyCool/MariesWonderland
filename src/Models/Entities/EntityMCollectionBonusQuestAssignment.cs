using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCollectionBonusQuestAssignment))]
public class EntityMCollectionBonusQuestAssignment
{
    [Key(0)] public int CollectionBonusQuestAssignmentId { get; set; }

    [Key(1)] public QuestAssignmentType QuestAssignmentType { get; set; }

    [Key(2)] public int MainQuestChapterId { get; set; }

    [Key(3)] public int EventQuestChapterId { get; set; }

    [Key(4)] public int QuestId { get; set; }

    [Key(5)] public int SortOrder { get; set; }
}
