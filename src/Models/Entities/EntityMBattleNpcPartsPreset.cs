using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMBattleNpcPartsPreset
{
    public long BattleNpcId { get; set; }

    public int BattleNpcPartsPresetNumber { get; set; }

    public string BattleNpcPartsUuid01 { get; set; }

    public string BattleNpcPartsUuid02 { get; set; }

    public string BattleNpcPartsUuid03 { get; set; }

    public string Name { get; set; }

    public int BattleNpcPartsPresetTagNumber { get; set; }
}
