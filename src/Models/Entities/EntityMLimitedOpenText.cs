using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMLimitedOpenText))]
[PrimaryKey(nameof(LimitedOpenTargetType), nameof(TargetId))]
public class EntityMLimitedOpenText
{
    [Key(0)] public LimitedOpenTargetType LimitedOpenTargetType { get; set; }

    [Key(1)] public int TargetId { get; set; }

    [Key(2)] public int OpenAchievementTextAssetId { get; set; }

    [Key(3)] public int LocalPushTextAssetId { get; set; }

    [Key(4)] public int OpenAchievementTextGroupId { get; set; }
}
