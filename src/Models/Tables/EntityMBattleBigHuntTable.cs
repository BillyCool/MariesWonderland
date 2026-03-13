using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMBattleBigHuntTable : TableBase<EntityMBattleBigHunt>
{
    private readonly Func<EntityMBattleBigHunt, int> primaryIndexSelector;

    public EntityMBattleBigHuntTable(EntityMBattleBigHunt[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.BattleGroupId;
    }

    public EntityMBattleBigHunt FindByBattleGroupId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
