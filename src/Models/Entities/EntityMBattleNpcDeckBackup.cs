using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleNpcDeckBackup))]
[PrimaryKey(nameof(BattleNpcId), nameof(BattleNpcDeckBackupUuid))]
public class EntityMBattleNpcDeckBackup
{
    [Key(0)] public long BattleNpcId { get; set; }

    [Key(1)] public string BattleNpcDeckBackupUuid { get; set; }

    [Key(2)] public string DeckJson { get; set; }
}
