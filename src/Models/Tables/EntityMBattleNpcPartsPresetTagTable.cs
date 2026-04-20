using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMBattleNpcPartsPresetTagTable : TableBase<EntityMBattleNpcPartsPresetTag>
{
    private readonly Func<EntityMBattleNpcPartsPresetTag, (long, int)> primaryIndexSelector;

    public EntityMBattleNpcPartsPresetTagTable(EntityMBattleNpcPartsPresetTag[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.BattleNpcId, element.BattleNpcPartsPresetTagNumber);
    }
}
