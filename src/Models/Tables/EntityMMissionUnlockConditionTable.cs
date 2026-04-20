using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMMissionUnlockConditionTable : TableBase<EntityMMissionUnlockCondition>
{
    private readonly Func<EntityMMissionUnlockCondition, int> primaryIndexSelector;

    public EntityMMissionUnlockConditionTable(EntityMMissionUnlockCondition[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.MissionUnlockConditionId;
    }

    public EntityMMissionUnlockCondition FindByMissionUnlockConditionId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
