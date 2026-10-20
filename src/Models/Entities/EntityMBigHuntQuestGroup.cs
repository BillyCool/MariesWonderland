using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBigHuntQuestGroup))]
[PrimaryKey(nameof(BigHuntQuestGroupId), nameof(SortOrder))]
public class EntityMBigHuntQuestGroup
{
    [Key(0)] public int BigHuntQuestGroupId { get; set; }

    [Key(1)] public int SortOrder { get; set; }

    [Key(2)] public int BigHuntQuestId { get; set; }
}
