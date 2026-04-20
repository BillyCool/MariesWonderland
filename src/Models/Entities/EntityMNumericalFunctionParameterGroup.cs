using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMNumericalFunctionParameterGroup))]
public class EntityMNumericalFunctionParameterGroup
{
    [Key(0)] public int NumericalFunctionParameterGroupId { get; set; }

    [Key(1)] public int ParameterIndex { get; set; }

    [Key(2)] public int ParameterValue { get; set; }
}
