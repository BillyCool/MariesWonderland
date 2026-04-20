using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMSkillAbnormalLifetimeBehaviourReceiveDamageCountTable : TableBase<EntityMSkillAbnormalLifetimeBehaviourReceiveDamageCount>
{
    private readonly Func<EntityMSkillAbnormalLifetimeBehaviourReceiveDamageCount, int> primaryIndexSelector;

    public EntityMSkillAbnormalLifetimeBehaviourReceiveDamageCountTable(EntityMSkillAbnormalLifetimeBehaviourReceiveDamageCount[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.SkillAbnormalLifetimeBehaviourId;
    }

    public EntityMSkillAbnormalLifetimeBehaviourReceiveDamageCount FindBySkillAbnormalLifetimeBehaviourId(int key) =>
        FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
