using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMGimmickSequenceSchedule))]
public class EntityMGimmickSequenceSchedule
{
    [Key(0)] public int GimmickSequenceScheduleId { get; set; }

    [Key(1)] public long StartDatetime { get; set; }

    [Key(2)] public long EndDatetime { get; set; }

    [Key(3)] public int FirstGimmickSequenceId { get; set; }

    [Key(4)] public int ReleaseEvaluateConditionId { get; set; }
}
