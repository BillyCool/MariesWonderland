using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMTipGroupTable : TableBase<EntityMTipGroup>
{
    private readonly Func<EntityMTipGroup, int> primaryIndexSelector;

    public EntityMTipGroupTable(EntityMTipGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.TipGroupId;
    }

    public EntityMTipGroup FindByTipGroupId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
