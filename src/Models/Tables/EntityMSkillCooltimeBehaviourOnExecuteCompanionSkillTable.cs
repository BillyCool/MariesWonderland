using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMSkillCooltimeBehaviourOnExecuteCompanionSkillTable : TableBase<EntityMSkillCooltimeBehaviourOnExecuteCompanionSkill>
{
    private readonly Func<EntityMSkillCooltimeBehaviourOnExecuteCompanionSkill, int> primaryIndexSelector;

    public EntityMSkillCooltimeBehaviourOnExecuteCompanionSkillTable(EntityMSkillCooltimeBehaviourOnExecuteCompanionSkill[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.SkillCooltimeBehaviourActionId;
    }

    public bool TryFindBySkillCooltimeBehaviourActionId(int key, out EntityMSkillCooltimeBehaviourOnExecuteCompanionSkill result) =>
        TryFindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key, out result);
}
