using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMSkillAbnormalBehaviourActionBuffResistanceTable : TableBase<EntityMSkillAbnormalBehaviourActionBuffResistance>
{
    private readonly Func<EntityMSkillAbnormalBehaviourActionBuffResistance, int> primaryIndexSelector;

    public EntityMSkillAbnormalBehaviourActionBuffResistanceTable(EntityMSkillAbnormalBehaviourActionBuffResistance[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.SkillAbnormalBehaviourActionId;
    }

    public EntityMSkillAbnormalBehaviourActionBuffResistance FindBySkillAbnormalBehaviourActionId(int key) =>
        FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
