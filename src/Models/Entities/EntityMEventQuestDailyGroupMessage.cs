using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMEventQuestDailyGroupMessage
{
    public int EventQuestDailyGroupMessageId { get; set; }

    public int OddsNumber { get; set; }

    public int Weight { get; set; }

    public int BeforeClearMessageTextId { get; set; }

    public int AfterClearMessageTextId { get; set; }
}
