using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMSkillAbnormalBehaviourActionDamageMultiplyDetailAlwaysTable : TableBase<EntityMSkillAbnormalBehaviourActionDamageMultiplyDetailAlways>
{
    private readonly Func<EntityMSkillAbnormalBehaviourActionDamageMultiplyDetailAlways, int> primaryIndexSelector;

    public EntityMSkillAbnormalBehaviourActionDamageMultiplyDetailAlwaysTable(EntityMSkillAbnormalBehaviourActionDamageMultiplyDetailAlways[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.DamageMultiplyAbnormalDetailId;
    }

    public EntityMSkillAbnormalBehaviourActionDamageMultiplyDetailAlways FindByDamageMultiplyAbnormalDetailId(int key) =>
        FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
