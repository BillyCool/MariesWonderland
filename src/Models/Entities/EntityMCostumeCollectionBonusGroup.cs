using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCostumeCollectionBonusGroup))]
public class EntityMCostumeCollectionBonusGroup
{
    [Key(0)] public int CollectionBonusGroupId { get; set; }

    [Key(1)] public int CostumeId { get; set; }

    [Key(2)] public int SortOrder { get; set; }
}
