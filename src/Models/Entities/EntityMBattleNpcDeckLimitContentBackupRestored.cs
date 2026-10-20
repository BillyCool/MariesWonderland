using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBattleNpcDeckLimitContentBackupRestored))]
[PrimaryKey(nameof(BattleNpcId), nameof(EventQuestChapterId))]
public class EntityMBattleNpcDeckLimitContentBackupRestored
{
    [Key(0)] public long BattleNpcId { get; set; }

    [Key(1)] public int EventQuestChapterId { get; set; }

    [Key(2)] public DifficultyType DifficultyType { get; set; }
}
