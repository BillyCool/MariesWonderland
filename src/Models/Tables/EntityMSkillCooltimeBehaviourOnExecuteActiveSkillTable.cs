using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMSkillCooltimeBehaviourOnExecuteActiveSkillTable : TableBase<EntityMSkillCooltimeBehaviourOnExecuteActiveSkill>
{
    private readonly Func<EntityMSkillCooltimeBehaviourOnExecuteActiveSkill, int> primaryIndexSelector;

    public EntityMSkillCooltimeBehaviourOnExecuteActiveSkillTable(EntityMSkillCooltimeBehaviourOnExecuteActiveSkill[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.SkillCooltimeBehaviourActionId;
    }

    public bool TryFindBySkillCooltimeBehaviourActionId(int key, out EntityMSkillCooltimeBehaviourOnExecuteActiveSkill result) =>
        TryFindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key, out result);
}
