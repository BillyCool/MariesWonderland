using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMSkillBehaviourActionRecovery
{
    public int SkillBehaviourActionId { get; set; }

    public int SkillPower { get; set; }

    public int FixedRecoveryPoint { get; set; }

    public int HpRatioRecoveryPointPermil { get; set; }

    public int RecoveryPointMinValue { get; set; }

    public int RecoveryPointMaxValue { get; set; }
}
