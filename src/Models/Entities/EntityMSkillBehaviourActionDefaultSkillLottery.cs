using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMSkillBehaviourActionDefaultSkillLottery
{
    public int SkillBehaviourActionId { get; set; }

    public int TargetCountLower { get; set; }

    public int TargetCountUpper { get; set; }

    public int ValuePermil { get; set; }

    public int CalculationType { get; set; }
}
