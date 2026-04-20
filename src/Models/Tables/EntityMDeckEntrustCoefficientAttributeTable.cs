using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Tables;

public class EntityMDeckEntrustCoefficientAttributeTable : TableBase<EntityMDeckEntrustCoefficientAttribute>
{
    private readonly Func<EntityMDeckEntrustCoefficientAttribute, (AttributeType, AttributeType)> primaryIndexSelector;

    public EntityMDeckEntrustCoefficientAttributeTable(EntityMDeckEntrustCoefficientAttribute[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.EntrustAttributeType, element.AttributeType);
    }
}
