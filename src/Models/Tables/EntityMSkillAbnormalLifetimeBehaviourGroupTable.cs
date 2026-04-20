using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMSkillAbnormalLifetimeBehaviourGroupTable : TableBase<EntityMSkillAbnormalLifetimeBehaviourGroup>
{
    private readonly Func<EntityMSkillAbnormalLifetimeBehaviourGroup, (int, int)> primaryIndexSelector;

    public EntityMSkillAbnormalLifetimeBehaviourGroupTable(EntityMSkillAbnormalLifetimeBehaviourGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.SkillAbnormalLifetimeBehaviourGroupId, element.AbnormalLifetimeBehaviourIndex);
    }
}
