using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCostumeEmblem))]
[PrimaryKey(nameof(CostumeEmblemAssetId))]
public class EntityMCostumeEmblem
{
    [Key(0)] public int CostumeEmblemAssetId { get; set; }

    [Key(1)] public int SortOrder { get; set; }
}
