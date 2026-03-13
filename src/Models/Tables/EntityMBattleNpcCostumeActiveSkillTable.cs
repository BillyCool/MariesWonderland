using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMBattleNpcCostumeActiveSkillTable : TableBase<EntityMBattleNpcCostumeActiveSkill>
{
    private readonly Func<EntityMBattleNpcCostumeActiveSkill, (long, string)> primaryIndexSelector;

    public EntityMBattleNpcCostumeActiveSkillTable(EntityMBattleNpcCostumeActiveSkill[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.BattleNpcId, element.BattleNpcCostumeUuid);
    }

    public EntityMBattleNpcCostumeActiveSkill FindByBattleNpcIdAndBattleNpcCostumeUuid(ValueTuple<long, string> key) =>
        FindUniqueCore(data, primaryIndexSelector, Comparer<(long, string)>.Default, key);
}
