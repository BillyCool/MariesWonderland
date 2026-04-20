using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMMomPointBannerTable : TableBase<EntityMMomPointBanner>
{
    private readonly Func<EntityMMomPointBanner, int> primaryIndexSelector;

    public EntityMMomPointBannerTable(EntityMMomPointBanner[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.MomPointBannerId;
    }
}
