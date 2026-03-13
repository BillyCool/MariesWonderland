using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Tables;

public class EntityMBattleSkillFireActConditionAttributeTypeTable : TableBase<EntityMBattleSkillFireActConditionAttributeType>
{
    private readonly Func<EntityMBattleSkillFireActConditionAttributeType, int> primaryIndexSelector;

    public EntityMBattleSkillFireActConditionAttributeTypeTable(EntityMBattleSkillFireActConditionAttributeType[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.BattleSkillFireActConditionId;
    }

    public bool TryFindByBattleSkillFireActConditionId(int key, out EntityMBattleSkillFireActConditionAttributeType result) =>
        TryFindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key, out result);
}
