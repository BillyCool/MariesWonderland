using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMPartsEnhancedTable : TableBase<EntityMPartsEnhanced>
{
    private readonly Func<EntityMPartsEnhanced, int> primaryIndexSelector;

    public EntityMPartsEnhancedTable(EntityMPartsEnhanced[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.PartsEnhancedId;
    }

    public bool TryFindByPartsEnhancedId(int key, out EntityMPartsEnhanced result) => TryFindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key, out result);
}
