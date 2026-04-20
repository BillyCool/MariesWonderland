using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserPremiumItemTable : TableBase<EntityIUserPremiumItem>
{
    private readonly Func<EntityIUserPremiumItem, (long, int)> primaryIndexSelector;

    public EntityIUserPremiumItemTable(EntityIUserPremiumItem[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.UserId, element.PremiumItemId);
    }

    public EntityIUserPremiumItem FindByUserIdAndPremiumItemId(ValueTuple<long, int> key) => FindUniqueCore(data, primaryIndexSelector, Comparer<(long, int)>.Default, key);

    public bool TryFindByUserIdAndPremiumItemId(ValueTuple<long, int> key, out EntityIUserPremiumItem result) =>
        TryFindUniqueCore(data, primaryIndexSelector, Comparer<(long, int)>.Default, key, out result);
}
