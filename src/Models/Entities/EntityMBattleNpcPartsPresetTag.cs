using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleNpcPartsPresetTag))]
public class EntityMBattleNpcPartsPresetTag
{
    [Key(0)] public long BattleNpcId { get; set; }

    [Key(1)] public int BattleNpcPartsPresetTagNumber { get; set; }

    [Key(2)] public string Name { get; set; }
}
