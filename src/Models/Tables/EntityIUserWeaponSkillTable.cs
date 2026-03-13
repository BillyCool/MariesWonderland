using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserWeaponSkillTable : TableBase<EntityIUserWeaponSkill>
{
    private readonly Func<EntityIUserWeaponSkill, (long, string, int)> primaryIndexSelector;

    public EntityIUserWeaponSkillTable(EntityIUserWeaponSkill[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.UserId, element.UserWeaponUuid, element.SlotNumber);
    }

    public bool TryFindByUserIdAndUserWeaponUuidAndSlotNumber(ValueTuple<long, string, int> key, out EntityIUserWeaponSkill result) =>
        TryFindUniqueCore(data, primaryIndexSelector, Comparer<(long, string, int)>.Default, key, out result);
}
