using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(CostumeId))]
public class EntityIUserCostumeLevelBonusReleaseStatus : IUserEntity
{
    public long UserId { get; set; }

    public int CostumeId { get; set; }

    public int LastReleasedBonusLevel { get; set; }

    public int ConfirmedBonusLevel { get; set; }

    public long LatestVersion { get; set; }
}
