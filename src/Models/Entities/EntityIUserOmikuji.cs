using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserOmikuji
{
    public long UserId { get; set; }

    public int OmikujiId { get; set; }

    public long LatestDrawDatetime { get; set; }

    public long LatestVersion { get; set; }
}
