using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMLibraryRecordGrouping
{
    public LibraryRecordType LibraryRecordType { get; set; }

    public int SortOrder { get; set; }

    public int LibraryRecordAssetId { get; set; }
}
