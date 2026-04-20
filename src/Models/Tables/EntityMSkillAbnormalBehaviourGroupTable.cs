using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMSkillAbnormalBehaviourGroupTable : TableBase<EntityMSkillAbnormalBehaviourGroup>
{
    private readonly Func<EntityMSkillAbnormalBehaviourGroup, (int, int)> primaryIndexSelector;

    public EntityMSkillAbnormalBehaviourGroupTable(EntityMSkillAbnormalBehaviourGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.SkillAbnormalBehaviourGroupId, element.AbnormalBehaviourIndex);
    }
}
