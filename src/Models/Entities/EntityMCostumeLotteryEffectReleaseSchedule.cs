using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCostumeLotteryEffectReleaseSchedule))]
[PrimaryKey(nameof(CostumeLotteryEffectReleaseScheduleId))]
public class EntityMCostumeLotteryEffectReleaseSchedule
{
    [Key(0)] public int CostumeLotteryEffectReleaseScheduleId { get; set; }

    [Key(1)] public long ReleaseDatetime { get; set; }
}
