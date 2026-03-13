using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMCostumeAwakenAbilityTable : TableBase<EntityMCostumeAwakenAbility>
{
    private readonly Func<EntityMCostumeAwakenAbility, int> primaryIndexSelector;

    public EntityMCostumeAwakenAbilityTable(EntityMCostumeAwakenAbility[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.CostumeAwakenAbilityId;
    }

    public EntityMCostumeAwakenAbility FindByCostumeAwakenAbilityId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);

    public bool TryFindByCostumeAwakenAbilityId(int key, out EntityMCostumeAwakenAbility result) => TryFindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key, out result);
}
