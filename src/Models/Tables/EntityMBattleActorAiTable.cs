using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMBattleActorAiTable : TableBase<EntityMBattleActorAi>
{
    private readonly Func<EntityMBattleActorAi, int> primaryIndexSelector;

    public EntityMBattleActorAiTable(EntityMBattleActorAi[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.BattleActorAiId;
    }

    public EntityMBattleActorAi FindByBattleActorAiId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
