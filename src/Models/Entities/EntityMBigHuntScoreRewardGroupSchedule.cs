using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBigHuntScoreRewardGroupSchedule))]
public class EntityMBigHuntScoreRewardGroupSchedule
{
    [Key(0)] public int BigHuntScoreRewardGroupScheduleId { get; set; }

    [Key(1)] public int GroupIndex { get; set; }

    [Key(2)] public int BigHuntScoreRewardGroupId { get; set; }

    [Key(3)] public long StartDatetime { get; set; }
}
