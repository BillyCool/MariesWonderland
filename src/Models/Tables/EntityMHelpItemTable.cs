using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMHelpItemTable : TableBase<EntityMHelpItem>
{
    private readonly Func<EntityMHelpItem, int> primaryIndexSelector;
    private readonly Func<EntityMHelpItem, int> secondaryIndexSelector;

    public EntityMHelpItemTable(EntityMHelpItem[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.HelpItemId;
        secondaryIndexSelector = element => element.HelpCategoryId;
    }

    public EntityMHelpItem FindByHelpItemId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);

    public RangeView<EntityMHelpItem> FindByHelpCategoryId(int key) => FindManyCore(data, secondaryIndexSelector, Comparer<int>.Default, key);
}
