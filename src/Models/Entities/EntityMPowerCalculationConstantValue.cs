using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMPowerCalculationConstantValue))]
[PrimaryKey(nameof(PowerCalculationConstantValueType))]
public class EntityMPowerCalculationConstantValue
{
    [Key(0)] public PowerCalculationConstantValueType PowerCalculationConstantValueType { get; set; }

    [Key(1)] public int ConstantValue { get; set; }
}
