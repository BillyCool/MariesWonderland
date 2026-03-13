using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserImportantItemTable : TableBase<EntityIUserImportantItem>
{
    private readonly Func<EntityIUserImportantItem, (long, int)> primaryIndexSelector;

    public EntityIUserImportantItemTable(EntityIUserImportantItem[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.UserId, element.ImportantItemId);
    }

    public EntityIUserImportantItem FindByUserIdAndImportantItemId(ValueTuple<long, int> key) => FindUniqueCore(data, primaryIndexSelector, Comparer<(long, int)>.Default, key);

    public bool TryFindByUserIdAndImportantItemId(ValueTuple<long, int> key, out EntityIUserImportantItem result) =>
        TryFindUniqueCore(data, primaryIndexSelector, Comparer<(long, int)>.Default, key, out result);

    public RangeView<EntityIUserImportantItem> FindRangeByUserIdAndImportantItemId(ValueTuple<long, int> min, ValueTuple<long, int> max, bool ascendant = true) =>
        FindUniqueRangeCore(data, primaryIndexSelector, Comparer<(long, int)>.Default, min, max, ascendant);
}
