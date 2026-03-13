using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMHelpItem
{
    public int HelpItemId { get; set; }

    public int HelpCategoryId { get; set; }

    public int SortOrder { get; set; }

    public int TotalPageCount { get; set; }

    public int TitleTextAssetId { get; set; }

    public string AssetName { get; set; }

    public bool IsHiddenOnList { get; set; }
}
