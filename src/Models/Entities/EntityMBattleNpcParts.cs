using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMBattleNpcParts
{
    public long BattleNpcId { get; set; }

    public string BattleNpcPartsUuid { get; set; }

    public int PartsId { get; set; }

    public int Level { get; set; }

    public int PartsStatusMainId { get; set; }

    public bool IsProtected { get; set; }

    public long AcquisitionDatetime { get; set; }
}
