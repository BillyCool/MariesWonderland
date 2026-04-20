using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMSkillCooltimeBehaviourTable : TableBase<EntityMSkillCooltimeBehaviour>
{
    private readonly Func<EntityMSkillCooltimeBehaviour, int> primaryIndexSelector;

    public EntityMSkillCooltimeBehaviourTable(EntityMSkillCooltimeBehaviour[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.SkillCooltimeBehaviourId;
    }

    public EntityMSkillCooltimeBehaviour FindBySkillCooltimeBehaviourId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
