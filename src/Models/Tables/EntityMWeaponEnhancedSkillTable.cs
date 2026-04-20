using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMWeaponEnhancedSkillTable : TableBase<EntityMWeaponEnhancedSkill>
{
    private readonly Func<EntityMWeaponEnhancedSkill, (int, int)> primaryIndexSelector;

    public EntityMWeaponEnhancedSkillTable(EntityMWeaponEnhancedSkill[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.WeaponEnhancedId, element.SkillId);
    }

    public bool TryFindByWeaponEnhancedIdAndSkillId(ValueTuple<int, int> key, out EntityMWeaponEnhancedSkill result) =>
        TryFindUniqueCore(data, primaryIndexSelector, Comparer<(int, int)>.Default, key, out result);
}
