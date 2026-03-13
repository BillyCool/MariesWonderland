using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMBattleNpcPartsGroupNote
{
    public long BattleNpcId { get; set; }

    public int PartsGroupId { get; set; }

    public long FirstAcquisitionDatetime { get; set; }
}
