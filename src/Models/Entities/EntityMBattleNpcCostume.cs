using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleNpcCostume))]
public class EntityMBattleNpcCostume
{
    [Key(0)] public long BattleNpcId { get; set; }

    [Key(1)] public string BattleNpcCostumeUuid { get; set; }

    [Key(2)] public int CostumeId { get; set; }

    [Key(3)] public int LimitBreakCount { get; set; }

    [Key(4)] public int Level { get; set; }

    [Key(5)] public int Exp { get; set; }

    [Key(6)] public int HeadupDisplayViewId { get; set; }

    [Key(7)] public long AcquisitionDatetime { get; set; }

    [Key(8)] public int AwakenCount { get; set; }
}
