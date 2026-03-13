using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMGimmickSequenceSchedule
{
    public int GimmickSequenceScheduleId { get; set; }

    public long StartDatetime { get; set; }

    public long EndDatetime { get; set; }

    public int FirstGimmickSequenceId { get; set; }

    public int ReleaseEvaluateConditionId { get; set; }
}
