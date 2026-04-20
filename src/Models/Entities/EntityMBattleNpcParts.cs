using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleNpcParts))]
public class EntityMBattleNpcParts
{
    [Key(0)] public long BattleNpcId { get; set; }

    [Key(1)] public string BattleNpcPartsUuid { get; set; }

    [Key(2)] public int PartsId { get; set; }

    [Key(3)] public int Level { get; set; }

    [Key(4)] public int PartsStatusMainId { get; set; }

    [Key(5)] public bool IsProtected { get; set; }

    [Key(6)] public long AcquisitionDatetime { get; set; }
}
