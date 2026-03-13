using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMSkillAbnormalLifetimeBehaviourFrameCountTable : TableBase<EntityMSkillAbnormalLifetimeBehaviourFrameCount>
{
    private readonly Func<EntityMSkillAbnormalLifetimeBehaviourFrameCount, int> primaryIndexSelector;

    public EntityMSkillAbnormalLifetimeBehaviourFrameCountTable(EntityMSkillAbnormalLifetimeBehaviourFrameCount[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.SkillAbnormalLifetimeBehaviourId;
    }

    public EntityMSkillAbnormalLifetimeBehaviourFrameCount FindBySkillAbnormalLifetimeBehaviourId(int key) =>
        FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
