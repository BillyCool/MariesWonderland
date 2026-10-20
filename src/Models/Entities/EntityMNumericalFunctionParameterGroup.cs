using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMNumericalFunctionParameterGroup))]
[PrimaryKey(nameof(NumericalFunctionParameterGroupId), nameof(ParameterIndex))]
public class EntityMNumericalFunctionParameterGroup
{
    [Key(0)] public int NumericalFunctionParameterGroupId { get; set; }

    [Key(1)] public int ParameterIndex { get; set; }

    [Key(2)] public int ParameterValue { get; set; }
}
