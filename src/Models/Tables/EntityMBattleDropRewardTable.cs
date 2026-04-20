using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMBattleDropRewardTable : TableBase<EntityMBattleDropReward>
{
    private readonly Func<EntityMBattleDropReward, int> primaryIndexSelector;

    public EntityMBattleDropRewardTable(EntityMBattleDropReward[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.BattleDropRewardId;
    }

    public EntityMBattleDropReward FindByBattleDropRewardId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
