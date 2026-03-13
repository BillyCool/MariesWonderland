using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMSkillAbnormalBehaviourTable : TableBase<EntityMSkillAbnormalBehaviour>
{
    private readonly Func<EntityMSkillAbnormalBehaviour, int> primaryIndexSelector;

    public EntityMSkillAbnormalBehaviourTable(EntityMSkillAbnormalBehaviour[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.SkillAbnormalBehaviourId;
    }

    public EntityMSkillAbnormalBehaviour FindBySkillAbnormalBehaviourId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
