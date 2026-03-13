using MariesWonderland.Models.Entities;

namespace MariesWonderland.Models.Tables;

public class EntityMBattleNpcPartsPresetTable : TableBase<EntityMBattleNpcPartsPreset>
{
    private readonly Func<EntityMBattleNpcPartsPreset, (long, int)> primaryIndexSelector;

    public EntityMBattleNpcPartsPresetTable(EntityMBattleNpcPartsPreset[] sortedData) : base(sortedData)
    {
        primaryIndexSelector = element => (element.BattleNpcId, element.BattleNpcPartsPresetNumber);
    }
}
