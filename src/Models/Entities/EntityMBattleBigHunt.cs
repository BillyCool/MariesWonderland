using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleBigHunt))]
public class EntityMBattleBigHunt
{
    [Key(0)] public int BattleGroupId { get; set; }

    [Key(1)] public int BattleBigHuntPhaseGroupId { get; set; }

    [Key(2)] public int KnockDownGaugeValueConfigGroupId { get; set; }
}
