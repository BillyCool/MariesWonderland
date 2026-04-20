using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleNpcDeckLimitContentBackup))]
public class EntityMBattleNpcDeckLimitContentBackup
{
    [Key(0)] public long BattleNpcId { get; set; }

    [Key(1)] public int EventQuestChapterId { get; set; }

    [Key(2)] public int EventQuestSequenceSortOrder { get; set; }

    [Key(3)] public string BattleNpcDeckBackupUuid { get; set; }
}
