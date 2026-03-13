using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMMissionPass
{
    public int MissionPassId { get; set; }

    public long StartDatetime { get; set; }

    public long EndDatetime { get; set; }

    public int PremiumItemId { get; set; }

    public int MissionPassLevelGroupId { get; set; }

    public int MissionPassRewardGroupId { get; set; }
}
