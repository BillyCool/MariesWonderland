using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMSkillBehaviourActionChangestepTable : TableBase<EntityMSkillBehaviourActionChangestep>
{
    private readonly Func<EntityMSkillBehaviourActionChangestep, int> primaryIndexSelector;

    public EntityMSkillBehaviourActionChangestepTable(EntityMSkillBehaviourActionChangestep[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.SkillBehaviourActionId;
    }

    public EntityMSkillBehaviourActionChangestep FindBySkillBehaviourActionId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
