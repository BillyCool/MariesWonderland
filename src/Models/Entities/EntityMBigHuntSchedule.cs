using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBigHuntSchedule))]
[PrimaryKey(nameof(BigHuntScheduleId))]
public class EntityMBigHuntSchedule
{
    [Key(0)] public int BigHuntScheduleId { get; set; }

    [Key(1)] public long NoticeStartDatetime { get; set; }

    [Key(2)] public long ChallengeStartDatetime { get; set; }

    [Key(3)] public long ChallengeEndDatetime { get; set; }

    [Key(4)] public int SeasonAssetId { get; set; }
}
