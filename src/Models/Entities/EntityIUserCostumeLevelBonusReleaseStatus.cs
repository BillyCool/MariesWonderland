using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserCostumeLevelBonusReleaseStatus : IUserEntity
{
    public long UserId { get; set; }

    public int CostumeId { get; set; }

    public int LastReleasedBonusLevel { get; set; }

    public int ConfirmedBonusLevel { get; set; }

    public long LatestVersion { get; set; }
}
