using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserSetting
{
    public long UserId { get; set; }

    public bool IsNotifyPurchaseAlert { get; set; }

    public long LatestVersion { get; set; }
}
