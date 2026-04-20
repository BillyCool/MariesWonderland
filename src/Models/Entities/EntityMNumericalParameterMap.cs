using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMNumericalParameterMap))]
public class EntityMNumericalParameterMap
{
    [Key(0)] public int NumericalParameterMapId { get; set; }

    [Key(1)] public int ParameterKey { get; set; }

    [Key(2)] public int ParameterValue { get; set; }
}
