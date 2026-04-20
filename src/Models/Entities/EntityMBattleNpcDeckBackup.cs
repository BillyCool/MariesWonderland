using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleNpcDeckBackup))]
public class EntityMBattleNpcDeckBackup
{
    [Key(0)] public long BattleNpcId { get; set; }

    [Key(1)] public string BattleNpcDeckBackupUuid { get; set; }

    [Key(2)] public string DeckJson { get; set; }
}
