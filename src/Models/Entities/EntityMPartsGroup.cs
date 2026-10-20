using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMPartsGroup))]
[PrimaryKey(nameof(PartsGroupId))]
public class EntityMPartsGroup
{
    [Key(0)] public int PartsGroupId { get; set; }

    [Key(1)] public int PartsSeriesId { get; set; }

    [Key(2)] public int SortOrder { get; set; }

    [Key(3)] public int PartsGroupAssetId { get; set; }
}
