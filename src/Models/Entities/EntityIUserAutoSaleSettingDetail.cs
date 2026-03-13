using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserAutoSaleSettingDetail
{
    public long UserId { get; set; }

    public int PossessionAutoSaleItemType { get; set; }

    public string PossessionAutoSaleItemValue { get; set; }

    public long LatestVersion { get; set; }
}
