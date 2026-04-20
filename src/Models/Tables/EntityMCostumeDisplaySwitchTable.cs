using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMCostumeDisplaySwitchTable : TableBase<EntityMCostumeDisplaySwitch>
{
    private readonly Func<EntityMCostumeDisplaySwitch, int> primaryIndexSelector;

    public EntityMCostumeDisplaySwitchTable(EntityMCostumeDisplaySwitch[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.CostumeId;
    }

    public bool TryFindByCostumeId(int key, out EntityMCostumeDisplaySwitch result) => TryFindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key, out result);
}
