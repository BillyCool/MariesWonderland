using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCatalogCompanion))]
[PrimaryKey(nameof(CompanionId))]
public class EntityMCatalogCompanion
{
    [Key(0)] public int CompanionId { get; set; }

    [Key(1)] public int SortOrder { get; set; }

    [Key(2)] public int CatalogTermId { get; set; }
}
