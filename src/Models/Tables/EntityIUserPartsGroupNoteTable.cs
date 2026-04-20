using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserPartsGroupNoteTable : TableBase<EntityIUserPartsGroupNote>
{
    private readonly Func<EntityIUserPartsGroupNote, (long, int)> primaryIndexSelector;

    public EntityIUserPartsGroupNoteTable(EntityIUserPartsGroupNote[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.UserId, element.PartsGroupId);
    }

    public bool TryFindByUserIdAndPartsGroupId(ValueTuple<long, int> key, out EntityIUserPartsGroupNote result) =>
        TryFindUniqueCore(data, primaryIndexSelector, Comparer<(long, int)>.Default, key, out result);
}
