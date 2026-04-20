using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMCostumeEnhancedTable : TableBase<EntityMCostumeEnhanced>
{
    private readonly Func<EntityMCostumeEnhanced, int> primaryIndexSelector;

    public EntityMCostumeEnhancedTable(EntityMCostumeEnhanced[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.CostumeEnhancedId;
    }

    public bool TryFindByCostumeEnhancedId(int key, out EntityMCostumeEnhanced result) => TryFindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key, out result);
}
