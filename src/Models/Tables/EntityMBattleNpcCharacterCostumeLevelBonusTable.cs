using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Tables;

public class EntityMBattleNpcCharacterCostumeLevelBonusTable : TableBase<EntityMBattleNpcCharacterCostumeLevelBonus>
{
    private readonly Func<EntityMBattleNpcCharacterCostumeLevelBonus, (long, int, StatusCalculationType)> primaryIndexSelector;

    public EntityMBattleNpcCharacterCostumeLevelBonusTable(EntityMBattleNpcCharacterCostumeLevelBonus[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.BattleNpcId, element.CharacterId, element.StatusCalculationType);
    }
}
