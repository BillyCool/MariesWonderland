using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMBattleNpcCostumeActiveSkill
{
    public long BattleNpcId { get; set; }

    public string BattleNpcCostumeUuid { get; set; }

    public int Level { get; set; }

    public long AcquisitionDatetime { get; set; }
}
