using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserPartsPresetTable : TableBase<EntityIUserPartsPreset>
{
    private readonly Func<EntityIUserPartsPreset, (long, int)> primaryIndexSelector;

    public EntityIUserPartsPresetTable(EntityIUserPartsPreset[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.UserId, element.UserPartsPresetNumber);
    }

    public EntityIUserPartsPreset FindByUserIdAndUserPartsPresetNumber(ValueTuple<long, int> key) =>
        FindUniqueCore(data, primaryIndexSelector, Comparer<(long, int)>.Default, key);

    public bool TryFindByUserIdAndUserPartsPresetNumber(ValueTuple<long, int> key, out EntityIUserPartsPreset result) =>
        TryFindUniqueCore(data, primaryIndexSelector, Comparer<(long, int)>.Default, key, out result);
}
