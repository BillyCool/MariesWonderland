using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCostumeLotteryEffectReleaseSchedule))]
public class EntityMCostumeLotteryEffectReleaseSchedule
{
    [Key(0)] public int CostumeLotteryEffectReleaseScheduleId { get; set; }

    [Key(1)] public long ReleaseDatetime { get; set; }
}
