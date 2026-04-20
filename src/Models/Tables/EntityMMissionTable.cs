using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMMissionTable : TableBase<EntityMMission>
{
    private readonly Func<EntityMMission, int> primaryIndexSelector;

    public EntityMMissionTable(EntityMMission[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.MissionId;
    }

    public EntityMMission FindByMissionId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
