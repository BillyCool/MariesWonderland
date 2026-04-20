using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMNumericalFunction))]
public class EntityMNumericalFunction
{
    [Key(0)] public int NumericalFunctionId { get; set; }

    [Key(1)] public NumericalFunctionType NumericalFunctionType { get; set; }

    [Key(2)] public int NumericalFunctionParameterGroupId { get; set; }
}
