using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMHeadupDisplayViewTable : TableBase<EntityMHeadupDisplayView>
{
    private readonly Func<EntityMHeadupDisplayView, int> primaryIndexSelector;

    public EntityMHeadupDisplayViewTable(EntityMHeadupDisplayView[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.HeadupDisplayViewId;
    }

    public EntityMHeadupDisplayView FindByHeadupDisplayViewId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
