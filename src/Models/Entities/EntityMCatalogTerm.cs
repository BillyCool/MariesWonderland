using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCatalogTerm))]
public class EntityMCatalogTerm
{
    [Key(0)] public int CatalogTermId { get; set; }

    [Key(1)] public long StartDatetime { get; set; }
}
