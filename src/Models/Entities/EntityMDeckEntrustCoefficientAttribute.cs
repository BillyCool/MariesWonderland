using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMDeckEntrustCoefficientAttribute))]
public class EntityMDeckEntrustCoefficientAttribute
{
    [Key(0)] public AttributeType EntrustAttributeType { get; set; }

    [Key(1)] public AttributeType AttributeType { get; set; }

    [Key(2)] public int CoefficientPermil { get; set; }
}
