using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMSkillBehaviourActionSkillRecoveryPowerCorrectionTable : TableBase<EntityMSkillBehaviourActionSkillRecoveryPowerCorrection>
{
    private readonly Func<EntityMSkillBehaviourActionSkillRecoveryPowerCorrection, int> primaryIndexSelector;

    public EntityMSkillBehaviourActionSkillRecoveryPowerCorrectionTable(EntityMSkillBehaviourActionSkillRecoveryPowerCorrection[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.SkillBehaviourActionId;
    }

    public EntityMSkillBehaviourActionSkillRecoveryPowerCorrection FindBySkillBehaviourActionId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
