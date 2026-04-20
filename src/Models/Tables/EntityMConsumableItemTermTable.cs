using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMConsumableItemTermTable : TableBase<EntityMConsumableItemTerm>
{
    private readonly Func<EntityMConsumableItemTerm, int> primaryIndexSelector;

    public EntityMConsumableItemTermTable(EntityMConsumableItemTerm[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.ConsumableItemTermId;
    }

    public EntityMConsumableItemTerm FindByConsumableItemTermId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
