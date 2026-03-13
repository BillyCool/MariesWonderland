using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Tables;

public class EntityMPowerCalculationConstantValueTable : TableBase<EntityMPowerCalculationConstantValue>
{
    private readonly Func<EntityMPowerCalculationConstantValue, PowerCalculationConstantValueType> primaryIndexSelector;

    public EntityMPowerCalculationConstantValueTable(EntityMPowerCalculationConstantValue[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.PowerCalculationConstantValueType;
    }
}
