using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMImportantItem
{
    public int ImportantItemId { get; set; }

    public int NameImportantItemTextId { get; set; }

    public int DescriptionImportantItemTextId { get; set; }

    public int SortOrder { get; set; }

    public int AssetCategoryId { get; set; }

    public int AssetVariationId { get; set; }

    public int ImportantItemEffectId { get; set; }

    public int ReportId { get; set; }

    public int CageMemoryId { get; set; }

    public int ImportantItemType { get; set; }

    public int ExternalReferenceId { get; set; }
}
