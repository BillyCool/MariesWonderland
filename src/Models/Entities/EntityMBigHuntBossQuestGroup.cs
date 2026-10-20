using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBigHuntBossQuestGroup))]
[PrimaryKey(nameof(BigHuntBossQuestGroupId), nameof(SortOrder))]
public class EntityMBigHuntBossQuestGroup
{
    [Key(0)] public int BigHuntBossQuestGroupId { get; set; }

    [Key(1)] public int SortOrder { get; set; }

    [Key(2)] public int BigHuntBossQuestId { get; set; }
}
