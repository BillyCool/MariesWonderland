using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMSkillBehaviourActionRecoveryPointCorrectionTable : TableBase<EntityMSkillBehaviourActionRecoveryPointCorrection>
{
    private readonly Func<EntityMSkillBehaviourActionRecoveryPointCorrection, int> primaryIndexSelector;

    public EntityMSkillBehaviourActionRecoveryPointCorrectionTable(EntityMSkillBehaviourActionRecoveryPointCorrection[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.SkillBehaviourActionId;
    }
}
