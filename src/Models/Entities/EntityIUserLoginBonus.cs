using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserLoginBonus
{
    public long UserId { get; set; }

    public int LoginBonusId { get; set; }

    public int CurrentPageNumber { get; set; }

    public int CurrentStampNumber { get; set; }

    public long LatestRewardReceiveDatetime { get; set; }

    public long LatestVersion { get; set; }
}
