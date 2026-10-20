using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMNumericalParameterMap))]
[PrimaryKey(nameof(NumericalParameterMapId), nameof(ParameterKey))]
public class EntityMNumericalParameterMap
{
    [Key(0)] public int NumericalParameterMapId { get; set; }

    [Key(1)] public int ParameterKey { get; set; }

    [Key(2)] public int ParameterValue { get; set; }
}
