using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(LoginBonusId))]
public class EntityIUserLoginBonus : IUserEntity
{
    public long UserId { get; set; }

    public int LoginBonusId { get; set; }

    public int CurrentPageNumber { get; set; }

    public int CurrentStampNumber { get; set; }

    public long LatestRewardReceiveDatetime { get; set; }

    public long LatestVersion { get; set; }
}
