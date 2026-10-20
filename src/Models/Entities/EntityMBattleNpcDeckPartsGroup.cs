using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleNpcDeckPartsGroup))]
[PrimaryKey(nameof(BattleNpcId), nameof(BattleNpcDeckCharacterUuid), nameof(BattleNpcPartsUuid))]
public class EntityMBattleNpcDeckPartsGroup
{
    [Key(0)] public long BattleNpcId { get; set; }

    [Key(1)] public string BattleNpcDeckCharacterUuid { get; set; }

    [Key(2)] public string BattleNpcPartsUuid { get; set; }

    [Key(3)] public int SortOrder { get; set; }
}
