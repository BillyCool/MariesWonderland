using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCollectionBonusQuestAssignmentGroup))]
[PrimaryKey(nameof(CollectionBonusQuestAssignmentGroupId), nameof(CollectionBonusQuestAssignmentId))]
public class EntityMCollectionBonusQuestAssignmentGroup
{
    [Key(0)] public int CollectionBonusQuestAssignmentGroupId { get; set; }

    [Key(1)] public int CollectionBonusQuestAssignmentId { get; set; }
}
