using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMHelpCategory))]
public class EntityMHelpCategory
{
    [Key(0)] public int HelpCategoryId { get; set; }

    [Key(1)] public int SortOrder { get; set; }

    [Key(2)] public int TitleTextAssetId { get; set; }

    [Key(3)] public bool IsHiddenOnList { get; set; }
}
