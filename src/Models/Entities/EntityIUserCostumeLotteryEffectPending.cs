using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserCostumeLotteryEffectPending
{
    public long UserId { get; set; }

    public string UserCostumeUuid { get; set; }

    public int SlotNumber { get; set; }

    public int OddsNumber { get; set; }

    public long LatestVersion { get; set; }
}
