using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMImportantItem))]
public class EntityMImportantItem
{
    [Key(0)] public int ImportantItemId { get; set; }

    [Key(1)] public int NameImportantItemTextId { get; set; }

    [Key(2)] public int DescriptionImportantItemTextId { get; set; }

    [Key(3)] public int SortOrder { get; set; }

    [Key(4)] public int AssetCategoryId { get; set; }

    [Key(5)] public int AssetVariationId { get; set; }

    [Key(6)] public int ImportantItemEffectId { get; set; }

    [Key(7)] public int ReportId { get; set; }

    [Key(8)] public int CageMemoryId { get; set; }

    [Key(9)] public int ImportantItemType { get; set; }

    [Key(10)] public int ExternalReferenceId { get; set; }
}
