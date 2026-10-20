using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleNpcPartsPresetTag))]
[PrimaryKey(nameof(BattleNpcId), nameof(BattleNpcPartsPresetTagNumber))]
public class EntityMBattleNpcPartsPresetTag
{
    [Key(0)] public long BattleNpcId { get; set; }

    [Key(1)] public int BattleNpcPartsPresetTagNumber { get; set; }

    [Key(2)] public string Name { get; set; }
}
