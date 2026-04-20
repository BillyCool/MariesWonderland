using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserSettingTable : TableBase<EntityIUserSetting>
{
    private readonly Func<EntityIUserSetting, long> primaryIndexSelector;

    public EntityIUserSettingTable(EntityIUserSetting[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.UserId;
    }

    public bool TryFindByUserId(long key, out EntityIUserSetting result) => TryFindUniqueCore(data, primaryIndexSelector, Comparer<long>.Default, key, out result);
}
