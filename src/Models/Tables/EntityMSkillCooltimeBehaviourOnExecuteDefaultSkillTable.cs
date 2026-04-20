using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMSkillCooltimeBehaviourOnExecuteDefaultSkillTable : TableBase<EntityMSkillCooltimeBehaviourOnExecuteDefaultSkill>
{
    private readonly Func<EntityMSkillCooltimeBehaviourOnExecuteDefaultSkill, int> primaryIndexSelector;

    public EntityMSkillCooltimeBehaviourOnExecuteDefaultSkillTable(EntityMSkillCooltimeBehaviourOnExecuteDefaultSkill[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.SkillCooltimeBehaviourActionId;
    }

    public bool TryFindBySkillCooltimeBehaviourActionId(int key, out EntityMSkillCooltimeBehaviourOnExecuteDefaultSkill result) =>
        TryFindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key, out result);
}
