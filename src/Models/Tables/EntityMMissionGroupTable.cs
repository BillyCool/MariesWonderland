using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMMissionGroupTable : TableBase<EntityMMissionGroup>
{
    private readonly Func<EntityMMissionGroup, int> primaryIndexSelector;

    public EntityMMissionGroupTable(EntityMMissionGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.MissionGroupId;
    }

    public EntityMMissionGroup FindByMissionGroupId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
