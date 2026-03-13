using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMQuestSchedule
{
    public int QuestScheduleId { get; set; }

    public string QuestScheduleCronExpression { get; set; }

    public long StartDatetime { get; set; }

    public long EndDatetime { get; set; }
}
