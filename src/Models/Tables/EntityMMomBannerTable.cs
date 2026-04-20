using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMMomBannerTable : TableBase<EntityMMomBanner>
{
    private readonly Func<EntityMMomBanner, int> primaryIndexSelector;

    public EntityMMomBannerTable(EntityMMomBanner[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.MomBannerId;
    }
}
