using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMImportantItemTable : TableBase<EntityMImportantItem>
{
    private readonly Func<EntityMImportantItem, int> primaryIndexSelector;
    private readonly Func<EntityMImportantItem, int> secondaryIndexSelector;

    public EntityMImportantItemTable(EntityMImportantItem[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.ImportantItemId;
        secondaryIndexSelector = element => element.ImportantItemType;
    }

    public EntityMImportantItem FindByImportantItemId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);

    public bool TryFindByImportantItemId(int key, out EntityMImportantItem result) => TryFindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key, out result);

    public RangeView<EntityMImportantItem> FindByImportantItemType(int key) => FindManyCore(data, secondaryIndexSelector, Comparer<int>.Default, key);
}
