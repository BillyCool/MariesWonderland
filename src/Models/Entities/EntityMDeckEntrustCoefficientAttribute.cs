using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMDeckEntrustCoefficientAttribute))]
[PrimaryKey(nameof(EntrustAttributeType), nameof(AttributeType))]
public class EntityMDeckEntrustCoefficientAttribute
{
    [Key(0)] public AttributeType EntrustAttributeType { get; set; }

    [Key(1)] public AttributeType AttributeType { get; set; }

    [Key(2)] public int CoefficientPermil { get; set; }
}
