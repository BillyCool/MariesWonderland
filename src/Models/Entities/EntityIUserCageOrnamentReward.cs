using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserCageOrnamentReward : IUserEntity
{
    public long UserId { get; set; }

    public int CageOrnamentId { get; set; }

    public long AcquisitionDatetime { get; set; }

    public long LatestVersion { get; set; }
}
