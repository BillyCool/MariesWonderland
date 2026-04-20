using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMMissionPassTable : TableBase<EntityMMissionPass>
{
    private readonly Func<EntityMMissionPass, int> primaryIndexSelector;

    public EntityMMissionPassTable(EntityMMissionPass[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.MissionPassId;
    }

    public EntityMMissionPass FindByMissionPassId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
