using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMQuestSchedule))]
public class EntityMQuestSchedule
{
    [Key(0)] public int QuestScheduleId { get; set; }

    [Key(1)] public string QuestScheduleCronExpression { get; set; }

    [Key(2)] public long StartDatetime { get; set; }

    [Key(3)] public long EndDatetime { get; set; }
}
