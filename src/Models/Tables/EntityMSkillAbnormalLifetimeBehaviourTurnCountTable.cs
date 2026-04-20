using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMSkillAbnormalLifetimeBehaviourTurnCountTable : TableBase<EntityMSkillAbnormalLifetimeBehaviourTurnCount>
{
    private readonly Func<EntityMSkillAbnormalLifetimeBehaviourTurnCount, int> primaryIndexSelector;

    public EntityMSkillAbnormalLifetimeBehaviourTurnCountTable(EntityMSkillAbnormalLifetimeBehaviourTurnCount[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.SkillAbnormalLifetimeBehaviourId;
    }

    public EntityMSkillAbnormalLifetimeBehaviourTurnCount FindBySkillAbnormalLifetimeBehaviourId(int key) =>
        FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
