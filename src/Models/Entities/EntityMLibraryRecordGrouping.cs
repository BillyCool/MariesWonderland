using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMLibraryRecordGrouping))]
public class EntityMLibraryRecordGrouping
{
    [Key(0)] public LibraryRecordType LibraryRecordType { get; set; }

    [Key(1)] public int SortOrder { get; set; }

    [Key(2)] public int LibraryRecordAssetId { get; set; }
}
