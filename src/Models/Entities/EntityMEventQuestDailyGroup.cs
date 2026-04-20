using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMEventQuestDailyGroup))]
public class EntityMEventQuestDailyGroup
{
    [Key(0)] public int EventQuestDailyGroupId { get; set; }

    [Key(1)] public long StartDatetime { get; set; }

    [Key(2)] public long EndDatetime { get; set; }

    [Key(3)] public int EventQuestDailyGroupTargetChapterId { get; set; }

    [Key(4)] public int EventQuestDailyGroupCompleteRewardId { get; set; }

    [Key(5)] public int EventQuestDailyGroupMessageId { get; set; }
}
