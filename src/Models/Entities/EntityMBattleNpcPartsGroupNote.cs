using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleNpcPartsGroupNote))]
public class EntityMBattleNpcPartsGroupNote
{
    [Key(0)] public long BattleNpcId { get; set; }

    [Key(1)] public int PartsGroupId { get; set; }

    [Key(2)] public long FirstAcquisitionDatetime { get; set; }
}
