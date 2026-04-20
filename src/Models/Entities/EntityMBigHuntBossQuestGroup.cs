using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBigHuntBossQuestGroup))]
public class EntityMBigHuntBossQuestGroup
{
    [Key(0)] public int BigHuntBossQuestGroupId { get; set; }

    [Key(1)] public int SortOrder { get; set; }

    [Key(2)] public int BigHuntBossQuestId { get; set; }
}
