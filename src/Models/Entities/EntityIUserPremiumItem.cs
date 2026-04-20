using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserPremiumItem : IUserEntity
{
    public long UserId { get; set; }

    public int PremiumItemId { get; set; }

    public long AcquisitionDatetime { get; set; }

    public long LatestVersion { get; set; }
}
