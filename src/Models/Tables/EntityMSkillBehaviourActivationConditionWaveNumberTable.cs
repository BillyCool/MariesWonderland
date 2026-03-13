using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMSkillBehaviourActivationConditionWaveNumberTable : TableBase<EntityMSkillBehaviourActivationConditionWaveNumber>
{
    private readonly Func<EntityMSkillBehaviourActivationConditionWaveNumber, int> primaryIndexSelector;

    public EntityMSkillBehaviourActivationConditionWaveNumberTable(EntityMSkillBehaviourActivationConditionWaveNumber[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.SkillBehaviourActivationConditionId;
    }

    public EntityMSkillBehaviourActivationConditionWaveNumber FindBySkillBehaviourActivationConditionId(int key) =>
        FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
