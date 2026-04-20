using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMTitleStillGroup))]
public class EntityMTitleStillGroup
{
    [Key(0)] public int TitleStillGroupId { get; set; }

    [Key(1)] public int DisplayStillCount { get; set; }

    [Key(2)] public int Weight { get; set; }

    [Key(3)] public int Priority { get; set; }
}
