using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCollectionBonusQuestAssignmentGroup))]
public class EntityMCollectionBonusQuestAssignmentGroup
{
    [Key(0)] public int CollectionBonusQuestAssignmentGroupId { get; set; }

    [Key(1)] public int CollectionBonusQuestAssignmentId { get; set; }
}
