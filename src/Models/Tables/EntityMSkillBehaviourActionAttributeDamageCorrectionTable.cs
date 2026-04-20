using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMSkillBehaviourActionAttributeDamageCorrectionTable : TableBase<EntityMSkillBehaviourActionAttributeDamageCorrection>
{
    private readonly Func<EntityMSkillBehaviourActionAttributeDamageCorrection, int> primaryIndexSelector;

    public EntityMSkillBehaviourActionAttributeDamageCorrectionTable(EntityMSkillBehaviourActionAttributeDamageCorrection[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.SkillBehaviourActionId;
    }

    public EntityMSkillBehaviourActionAttributeDamageCorrection FindBySkillBehaviourActionId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
