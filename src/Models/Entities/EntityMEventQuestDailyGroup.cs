using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMEventQuestDailyGroup
{
    public int EventQuestDailyGroupId { get; set; }

    public long StartDatetime { get; set; }

    public long EndDatetime { get; set; }

    public int EventQuestDailyGroupTargetChapterId { get; set; }

    public int EventQuestDailyGroupCompleteRewardId { get; set; }

    public int EventQuestDailyGroupMessageId { get; set; }
}
