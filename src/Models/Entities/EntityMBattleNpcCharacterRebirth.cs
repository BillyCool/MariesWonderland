using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleNpcCharacterRebirth))]
[PrimaryKey(nameof(BattleNpcId), nameof(CharacterId))]
public class EntityMBattleNpcCharacterRebirth
{
    [Key(0)] public long BattleNpcId { get; set; }

    [Key(1)] public int CharacterId { get; set; }

    [Key(2)] public int RebirthCount { get; set; }
}
