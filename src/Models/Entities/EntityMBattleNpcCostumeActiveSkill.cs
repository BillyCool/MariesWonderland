using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleNpcCostumeActiveSkill))]
public class EntityMBattleNpcCostumeActiveSkill
{
    [Key(0)] public long BattleNpcId { get; set; }

    [Key(1)] public string BattleNpcCostumeUuid { get; set; }

    [Key(2)] public int Level { get; set; }

    [Key(3)] public long AcquisitionDatetime { get; set; }
}
