using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCatalogTerm))]
[PrimaryKey(nameof(CatalogTermId))]
public class EntityMCatalogTerm
{
    [Key(0)] public int CatalogTermId { get; set; }

    [Key(1)] public long StartDatetime { get; set; }
}
