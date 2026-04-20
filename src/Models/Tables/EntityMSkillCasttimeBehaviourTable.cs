using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMSkillCasttimeBehaviourTable : TableBase<EntityMSkillCasttimeBehaviour>
{
    private readonly Func<EntityMSkillCasttimeBehaviour, int> primaryIndexSelector;

    public EntityMSkillCasttimeBehaviourTable(EntityMSkillCasttimeBehaviour[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.SkillCasttimeBehaviourId;
    }

    public EntityMSkillCasttimeBehaviour FindBySkillCasttimeBehaviourId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
