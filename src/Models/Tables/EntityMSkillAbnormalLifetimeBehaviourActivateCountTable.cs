using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMSkillAbnormalLifetimeBehaviourActivateCountTable : TableBase<EntityMSkillAbnormalLifetimeBehaviourActivateCount>
{
    private readonly Func<EntityMSkillAbnormalLifetimeBehaviourActivateCount, int> primaryIndexSelector;

    public EntityMSkillAbnormalLifetimeBehaviourActivateCountTable(EntityMSkillAbnormalLifetimeBehaviourActivateCount[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.SkillAbnormalLifetimeBehaviourId;
    }

    public EntityMSkillAbnormalLifetimeBehaviourActivateCount FindBySkillAbnormalLifetimeBehaviourId(int key) =>
        FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
