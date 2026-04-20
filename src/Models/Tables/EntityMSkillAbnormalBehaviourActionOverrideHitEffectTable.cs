using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMSkillAbnormalBehaviourActionOverrideHitEffectTable : TableBase<EntityMSkillAbnormalBehaviourActionOverrideHitEffect>
{
    private readonly Func<EntityMSkillAbnormalBehaviourActionOverrideHitEffect, int> primaryIndexSelector;

    public EntityMSkillAbnormalBehaviourActionOverrideHitEffectTable(EntityMSkillAbnormalBehaviourActionOverrideHitEffect[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.SkillAbnormalBehaviourActionId;
    }

    public EntityMSkillAbnormalBehaviourActionOverrideHitEffect FindBySkillAbnormalBehaviourActionId(int key) =>
        FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
