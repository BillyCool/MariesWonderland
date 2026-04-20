using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserNaviCutIn : IUserEntity
{
    public long UserId { get; set; }

    public int NaviCutInId { get; set; }

    public long PlayDatetime { get; set; }

    public long LatestVersion { get; set; }
}
