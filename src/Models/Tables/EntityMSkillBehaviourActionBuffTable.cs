using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMSkillBehaviourActionBuffTable : TableBase<EntityMSkillBehaviourActionBuff>
{
    private readonly Func<EntityMSkillBehaviourActionBuff, int> primaryIndexSelector;

    public EntityMSkillBehaviourActionBuffTable(EntityMSkillBehaviourActionBuff[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.SkillBehaviourActionId;
    }

    public EntityMSkillBehaviourActionBuff FindBySkillBehaviourActionId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
