using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMSkillBehaviourActionAttackMainWeaponAttributeTable : TableBase<EntityMSkillBehaviourActionAttackMainWeaponAttribute>
{
    private readonly Func<EntityMSkillBehaviourActionAttackMainWeaponAttribute, int> primaryIndexSelector;

    public EntityMSkillBehaviourActionAttackMainWeaponAttributeTable(EntityMSkillBehaviourActionAttackMainWeaponAttribute[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => element.SkillBehaviourActionId;
    }

    public EntityMSkillBehaviourActionAttackMainWeaponAttribute FindBySkillBehaviourActionId(int key) => FindUniqueCore(data, primaryIndexSelector, Comparer<int>.Default, key);
}
