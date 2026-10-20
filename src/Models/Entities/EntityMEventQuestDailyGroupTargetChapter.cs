using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMEventQuestDailyGroupTargetChapter))]
[PrimaryKey(nameof(EventQuestDailyGroupTargetChapterId), nameof(SortOrder))]
public class EntityMEventQuestDailyGroupTargetChapter
{
    [Key(0)] public int EventQuestDailyGroupTargetChapterId { get; set; }

    [Key(1)] public int SortOrder { get; set; }

    [Key(2)] public int EventQuestChapterId { get; set; }
}
