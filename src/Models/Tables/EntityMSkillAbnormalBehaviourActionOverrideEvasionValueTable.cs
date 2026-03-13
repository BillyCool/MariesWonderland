using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMSkillAbnormalBehaviourActionOverrideEvasionValueTable : TableBase<EntityMSkillAbnormalBehaviourActionOverrideEvasionValue>
{
    private readonly Func<EntityMSkillAbnormalBehaviourActionOverrideEvasionValue, int> primaryIndexSelector;

    public EntityMSkillAbnormalBehaviourActionOverrideEvasionValueTable(EntityMSkillAbnormalBehaviourActionOverrideEvasionValue[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.SkillAbnormalBehaviourActionId;
    }

    public EntityMSkillAbnormalBehaviourActionOverrideEvasionValue FindBySkillAbnormalBehaviourActionId(int key) =>
        FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
