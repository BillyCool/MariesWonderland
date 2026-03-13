using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMCompanionEnhancedTable : TableBase<EntityMCompanionEnhanced>
{
    private readonly Func<EntityMCompanionEnhanced, int> primaryIndexSelector;

    public EntityMCompanionEnhancedTable(EntityMCompanionEnhanced[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.CompanionEnhancedId;
    }

    public bool TryFindByCompanionEnhancedId(int key, out EntityMCompanionEnhanced result) => TryFindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key, out result);
}
