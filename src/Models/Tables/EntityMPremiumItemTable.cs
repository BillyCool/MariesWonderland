using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMPremiumItemTable : TableBase<EntityMPremiumItem>
{
    private readonly Func<EntityMPremiumItem, int> primaryIndexSelector;

    public EntityMPremiumItemTable(EntityMPremiumItem[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.PremiumItemId;
    }
}
