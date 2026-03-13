using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMHelpCategory
{
    public int HelpCategoryId { get; set; }

    public int SortOrder { get; set; }

    public int TitleTextAssetId { get; set; }

    public bool IsHiddenOnList { get; set; }
}
