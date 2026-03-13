using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMSkillBehaviourActionOverlimitDamageMultiplyTable : TableBase<EntityMSkillBehaviourActionOverlimitDamageMultiply>
{
    private readonly Func<EntityMSkillBehaviourActionOverlimitDamageMultiply, int> primaryIndexSelector;

    public EntityMSkillBehaviourActionOverlimitDamageMultiplyTable(EntityMSkillBehaviourActionOverlimitDamageMultiply[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.SkillBehaviourActionId;
    }

    public EntityMSkillBehaviourActionOverlimitDamageMultiply FindBySkillBehaviourActionId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
