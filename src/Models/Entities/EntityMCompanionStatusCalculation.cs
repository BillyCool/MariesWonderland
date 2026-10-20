using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCompanionStatusCalculation))]
[PrimaryKey(nameof(CompanionStatusCalculationId))]
public class EntityMCompanionStatusCalculation
{
    [Key(0)] public int CompanionStatusCalculationId { get; set; }

    [Key(1)] public int AttackNumericalFunctionId { get; set; }

    [Key(2)] public int HpNumericalFunctionId { get; set; }

    [Key(3)] public int VitalityNumericalFunctionId { get; set; }
}
