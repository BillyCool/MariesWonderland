using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBigHuntQuestGroup))]
public class EntityMBigHuntQuestGroup
{
    [Key(0)] public int BigHuntQuestGroupId { get; set; }

    [Key(1)] public int SortOrder { get; set; }

    [Key(2)] public int BigHuntQuestId { get; set; }
}
