using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMQuestMissionConditionValueGroup))]
[PrimaryKey(nameof(QuestMissionConditionValueGroupId), nameof(SortOrder))]
public class EntityMQuestMissionConditionValueGroup
{
    [Key(0)] public int QuestMissionConditionValueGroupId { get; set; }

    [Key(1)] public int SortOrder { get; set; }

    [Key(2)] public int ConditionValue { get; set; }
}
