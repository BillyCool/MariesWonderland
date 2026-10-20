using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMQuestReleaseConditionQuestChallenge))]
[PrimaryKey(nameof(QuestReleaseConditionId))]
public class EntityMQuestReleaseConditionQuestChallenge
{
    [Key(0)] public int QuestReleaseConditionId { get; set; }

    [Key(1)] public int QuestId { get; set; }
}
