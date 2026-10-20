using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleNpcCostumeLotteryEffectPending))]
[PrimaryKey(nameof(BattleNpcId), nameof(BattleNpcCostumeUuid))]
public class EntityMBattleNpcCostumeLotteryEffectPending
{
    [Key(0)] public long BattleNpcId { get; set; }

    [Key(1)] public string BattleNpcCostumeUuid { get; set; }

    [Key(2)] public int SlotNumber { get; set; }

    [Key(3)] public int OddsNumber { get; set; }
}
