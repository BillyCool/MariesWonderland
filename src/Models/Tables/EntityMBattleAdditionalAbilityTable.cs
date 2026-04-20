using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMBattleAdditionalAbilityTable : TableBase<EntityMBattleAdditionalAbility>
{
    private readonly Func<EntityMBattleAdditionalAbility, (int, int, int)> primaryIndexSelector;
    private readonly Func<EntityMBattleAdditionalAbility, (int, int)> secondaryIndexSelector;

    public EntityMBattleAdditionalAbilityTable(EntityMBattleAdditionalAbility[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.BattleGroupId, element.TargetActorAppearanceWaveNumber, element.AbilityIndex);
        secondaryIndexSelector = element => (element.BattleGroupId, element.TargetActorAppearanceWaveNumber);
    }

    public RangeView<EntityMBattleAdditionalAbility> FindByBattleGroupIdAndTargetActorAppearanceWaveNumber(ValueTuple<int, int> key) =>
        FindManyCore(data, secondaryIndexSelector, Comparer<(int, int)>.Default, key);
}
