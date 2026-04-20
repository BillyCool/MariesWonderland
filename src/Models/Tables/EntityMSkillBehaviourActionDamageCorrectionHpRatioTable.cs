using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMSkillBehaviourActionDamageCorrectionHpRatioTable : TableBase<EntityMSkillBehaviourActionDamageCorrectionHpRatio>
{
    private readonly Func<EntityMSkillBehaviourActionDamageCorrectionHpRatio, int> primaryIndexSelector;

    public EntityMSkillBehaviourActionDamageCorrectionHpRatioTable(EntityMSkillBehaviourActionDamageCorrectionHpRatio[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.SkillBehaviourActionId;
    }

    public EntityMSkillBehaviourActionDamageCorrectionHpRatio FindBySkillBehaviourActionId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
