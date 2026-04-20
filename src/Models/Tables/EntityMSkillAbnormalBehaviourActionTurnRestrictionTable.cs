using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMSkillAbnormalBehaviourActionTurnRestrictionTable : TableBase<EntityMSkillAbnormalBehaviourActionTurnRestriction>
{
    private readonly Func<EntityMSkillAbnormalBehaviourActionTurnRestriction, int> primaryIndexSelector;

    public EntityMSkillAbnormalBehaviourActionTurnRestrictionTable(EntityMSkillAbnormalBehaviourActionTurnRestriction[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.SkillBehaviourActionId;
    }

    public EntityMSkillAbnormalBehaviourActionTurnRestriction FindBySkillBehaviourActionId(int key) =>
        FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
