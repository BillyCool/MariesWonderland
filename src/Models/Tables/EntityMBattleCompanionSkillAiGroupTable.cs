using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMBattleCompanionSkillAiGroupTable : TableBase<EntityMBattleCompanionSkillAiGroup>
{
    private readonly Func<EntityMBattleCompanionSkillAiGroup, (int, int)> primaryIndexSelector;

    public EntityMBattleCompanionSkillAiGroupTable(EntityMBattleCompanionSkillAiGroup[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.BattleCompanionSkillAiGroupId, element.Priority);
    }
}
