using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleNpcCostumeLotteryEffectAbility))]
[PrimaryKey(nameof(BattleNpcId), nameof(BattleNpcCostumeUuid), nameof(SlotNumber))]
public class EntityMBattleNpcCostumeLotteryEffectAbility
{
    [Key(0)] public long BattleNpcId { get; set; }

    [Key(1)] public string BattleNpcCostumeUuid { get; set; }

    [Key(2)] public int SlotNumber { get; set; }

    [Key(3)] public int AbilityId { get; set; }

    [Key(4)] public int AbilityLevel { get; set; }
}
