using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMQuestReleaseConditionUserLevel))]
[PrimaryKey(nameof(QuestReleaseConditionId))]
public class EntityMQuestReleaseConditionUserLevel
{
    [Key(0)] public int QuestReleaseConditionId { get; set; }

    [Key(1)] public int UserLevel { get; set; }
}
