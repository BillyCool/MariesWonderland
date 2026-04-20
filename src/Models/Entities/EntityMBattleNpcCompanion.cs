using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleNpcCompanion))]
public class EntityMBattleNpcCompanion
{
    [Key(0)] public long BattleNpcId { get; set; }

    [Key(1)] public string BattleNpcCompanionUuid { get; set; }

    [Key(2)] public int CompanionId { get; set; }

    [Key(3)] public int HeadupDisplayViewId { get; set; }

    [Key(4)] public int Level { get; set; }

    [Key(5)] public long AcquisitionDatetime { get; set; }
}
