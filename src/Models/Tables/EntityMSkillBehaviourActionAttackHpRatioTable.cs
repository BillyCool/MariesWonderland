using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMSkillBehaviourActionAttackHpRatioTable : TableBase<EntityMSkillBehaviourActionAttackHpRatio>
{
    private readonly Func<EntityMSkillBehaviourActionAttackHpRatio, int> primaryIndexSelector;

    public EntityMSkillBehaviourActionAttackHpRatioTable(EntityMSkillBehaviourActionAttackHpRatio[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.SkillBehaviourActionId;
    }

    public EntityMSkillBehaviourActionAttackHpRatio FindBySkillBehaviourActionId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
