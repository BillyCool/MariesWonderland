using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMPartsStatusMain))]
public class EntityMPartsStatusMain
{
    [Key(0)] public int PartsStatusMainId { get; set; }

    [Key(1)] public StatusKindType StatusKindType { get; set; }

    [Key(2)] public StatusCalculationType StatusCalculationType { get; set; }

    [Key(3)] public int StatusChangeInitialValue { get; set; }

    [Key(4)] public int StatusNumericalFunctionId { get; set; }
}
