using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMPartsEnhancedSubStatusTable : TableBase<EntityMPartsEnhancedSubStatus>
{
    private readonly Func<EntityMPartsEnhancedSubStatus, (int, int)> primaryIndexSelector;

    public EntityMPartsEnhancedSubStatusTable(EntityMPartsEnhancedSubStatus[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.PartsEnhancedId, element.StatusIndex);
    }
}
