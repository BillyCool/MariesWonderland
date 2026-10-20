using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(PossessionAutoSaleItemType))]
public class EntityIUserAutoSaleSettingDetail : IUserEntity
{
    public long UserId { get; set; }

    public int PossessionAutoSaleItemType { get; set; }

    public string PossessionAutoSaleItemValue { get; set; }

    public long LatestVersion { get; set; }
}
