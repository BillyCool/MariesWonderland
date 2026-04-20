using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMAbilityTable : TableBase<EntityMAbility>
{
    private readonly Func<EntityMAbility, int> primaryIndexSelector;

    public EntityMAbilityTable(EntityMAbility[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.AbilityId;
    }

    public EntityMAbility FindByAbilityId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);

    public bool TryFindByAbilityId(int key, out EntityMAbility result) => TryFindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key, out result);
}
