using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMSkillAbnormalBehaviourActionAbnormalResistanceTable : TableBase<EntityMSkillAbnormalBehaviourActionAbnormalResistance>
{
    private readonly Func<EntityMSkillAbnormalBehaviourActionAbnormalResistance, int> primaryIndexSelector;

    public EntityMSkillAbnormalBehaviourActionAbnormalResistanceTable(EntityMSkillAbnormalBehaviourActionAbnormalResistance[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.SkillAbnormalBehaviourActionId;
    }

    public EntityMSkillAbnormalBehaviourActionAbnormalResistance FindBySkillAbnormalBehaviourActionId(int key) =>
        FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
