using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(UserCostumeUuid))]
public class EntityIUserCostumeLotteryEffectPending : IUserEntity
{
    public long UserId { get; set; }

    public string UserCostumeUuid { get; set; }

    public int SlotNumber { get; set; }

    public int OddsNumber { get; set; }

    public long LatestVersion { get; set; }
}
