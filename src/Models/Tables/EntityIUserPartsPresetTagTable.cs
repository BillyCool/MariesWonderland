using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserPartsPresetTagTable : TableBase<EntityIUserPartsPresetTag>
{
    private readonly Func<EntityIUserPartsPresetTag, (long, int)> primaryIndexSelector;

    public EntityIUserPartsPresetTagTable(EntityIUserPartsPresetTag[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.UserId, element.UserPartsPresetTagNumber);
    }

    public EntityIUserPartsPresetTag FindByUserIdAndUserPartsPresetTagNumber(ValueTuple<long, int> key) =>
        FindUniqueCore(data, primaryIndexSelector, Comparer<(long, int)>.Default, key);

    public bool TryFindByUserIdAndUserPartsPresetTagNumber(ValueTuple<long, int> key, out EntityIUserPartsPresetTag result) =>
        TryFindUniqueCore(data, primaryIndexSelector, Comparer<(long, int)>.Default, key, out result);
}
