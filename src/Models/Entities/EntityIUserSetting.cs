using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId))]
public class EntityIUserSetting : IUserEntity
{
    public long UserId { get; set; }

    public bool IsNotifyPurchaseAlert { get; set; }

    public long LatestVersion { get; set; }
}
