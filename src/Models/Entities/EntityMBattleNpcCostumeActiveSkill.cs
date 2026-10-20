using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleNpcCostumeActiveSkill))]
[PrimaryKey(nameof(BattleNpcId), nameof(BattleNpcCostumeUuid))]
public class EntityMBattleNpcCostumeActiveSkill
{
    [Key(0)] public long BattleNpcId { get; set; }

    [Key(1)] public string BattleNpcCostumeUuid { get; set; }

    [Key(2)] public int Level { get; set; }

    [Key(3)] public long AcquisitionDatetime { get; set; }
}
