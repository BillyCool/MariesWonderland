using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMPowerCalculationConstantValue))]
public class EntityMPowerCalculationConstantValue
{
    [Key(0)] public PowerCalculationConstantValueType PowerCalculationConstantValueType { get; set; }

    [Key(1)] public int ConstantValue { get; set; }
}
