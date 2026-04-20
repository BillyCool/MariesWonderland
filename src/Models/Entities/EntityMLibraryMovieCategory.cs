using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMLibraryMovieCategory))]
public class EntityMLibraryMovieCategory
{
    [Key(0)] public int LibraryMovieCategoryId { get; set; }

    [Key(1)] public int NameLibraryTextId { get; set; }

    [Key(2)] public int SortOrder { get; set; }
}
