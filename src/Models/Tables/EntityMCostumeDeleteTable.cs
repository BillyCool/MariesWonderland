using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMCostumeDeleteTable : TableBase<EntityMCostumeDelete>
{
    private readonly Func<EntityMCostumeDelete, int> primaryIndexSelector;

    public EntityMCostumeDeleteTable(EntityMCostumeDelete[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.CostumeId;
    }

    public bool TryFindByCostumeId(int key, out EntityMCostumeDelete result) => TryFindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key, out result);
}
