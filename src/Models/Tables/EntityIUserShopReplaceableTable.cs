using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserShopReplaceableTable : TableBase<EntityIUserShopReplaceable>
{
    private readonly Func<EntityIUserShopReplaceable, long> primaryIndexSelector;

    public EntityIUserShopReplaceableTable(EntityIUserShopReplaceable[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.UserId;
    }

    public EntityIUserShopReplaceable FindByUserId(long key) => FindUniqueCore(data, primaryIndexSelector, Comparer<long>.Default, key);
}
