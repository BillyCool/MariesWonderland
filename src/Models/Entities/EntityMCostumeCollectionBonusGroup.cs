using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCostumeCollectionBonusGroup))]
[PrimaryKey(nameof(CollectionBonusGroupId), nameof(CostumeId))]
public class EntityMCostumeCollectionBonusGroup
{
    [Key(0)] public int CollectionBonusGroupId { get; set; }

    [Key(1)] public int CostumeId { get; set; }

    [Key(2)] public int SortOrder { get; set; }
}
