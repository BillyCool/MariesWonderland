using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMAbilityDetailTable : TableBase<EntityMAbilityDetail>
{
    private readonly Func<EntityMAbilityDetail, int> primaryIndexSelector;

    public EntityMAbilityDetailTable(EntityMAbilityDetail[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.AbilityDetailId;
    }

    public EntityMAbilityDetail FindByAbilityDetailId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
