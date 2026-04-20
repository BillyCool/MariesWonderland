using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Tables;

public class EntityMLibraryRecordGroupingTable : TableBase<EntityMLibraryRecordGrouping>
{
    private readonly Func<EntityMLibraryRecordGrouping, LibraryRecordType> primaryIndexSelector;

    public EntityMLibraryRecordGroupingTable(EntityMLibraryRecordGrouping[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.LibraryRecordType;
    }
}
