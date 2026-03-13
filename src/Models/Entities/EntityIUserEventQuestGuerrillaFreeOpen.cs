using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserEventQuestGuerrillaFreeOpen
{
    public long UserId { get; set; }

    public long StartDatetime { get; set; }

    public int OpenMinutes { get; set; }

    public int DailyOpenedCount { get; set; }

    public long LatestVersion { get; set; }
}
