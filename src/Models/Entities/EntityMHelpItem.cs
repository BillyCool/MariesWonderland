using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMHelpItem))]
[PrimaryKey(nameof(HelpItemId))]
public class EntityMHelpItem
{
    [Key(0)] public int HelpItemId { get; set; }

    [Key(1)] public int HelpCategoryId { get; set; }

    [Key(2)] public int SortOrder { get; set; }

    [Key(3)] public int TotalPageCount { get; set; }

    [Key(4)] public int TitleTextAssetId { get; set; }

    [Key(5)] public string AssetName { get; set; }

    [Key(6)] public bool IsHiddenOnList { get; set; }
}
