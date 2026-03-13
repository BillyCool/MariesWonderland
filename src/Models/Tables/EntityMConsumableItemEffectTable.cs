using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMConsumableItemEffectTable : TableBase<EntityMConsumableItemEffect>
{
    private readonly Func<EntityMConsumableItemEffect, int> primaryIndexSelector;

    public EntityMConsumableItemEffectTable(EntityMConsumableItemEffect[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.ConsumableItemId;
    }

    public EntityMConsumableItemEffect FindByConsumableItemId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
