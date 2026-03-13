using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityIUserCostumeActiveSkillTable : TableBase<EntityIUserCostumeActiveSkill>
{
    private readonly Func<EntityIUserCostumeActiveSkill, (long, string)> primaryIndexSelector;

    public EntityIUserCostumeActiveSkillTable(EntityIUserCostumeActiveSkill[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.UserId, element.UserCostumeUuid);
    }

    public EntityIUserCostumeActiveSkill FindByUserIdAndUserCostumeUuid(ValueTuple<long, string> key) => FindUniqueCore(data, primaryIndexSelector, Comparer<(long, string)>.Default, key);
}
