using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMSkillAbnormal
{
    public int SkillAbnormalId { get; set; }

    public int SkillAbnormalTypeId { get; set; }

    public AbnormalPolarityType AbnormalPolarityType { get; set; }

    public int SkillAbnormalLifetimeId { get; set; }

    public int SkillAbnormalBehaviourGroupId { get; set; }
}
