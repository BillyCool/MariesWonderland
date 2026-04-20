using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMEventQuestGuerrillaFreeOpen))]
public class EntityMEventQuestGuerrillaFreeOpen
{
    [Key(0)] public int EventQuestGuerrillaFreeOpenId { get; set; }

    [Key(1)] public int OpenMinutes { get; set; }

    [Key(2)] public int DailyOpenMaxCount { get; set; }

    [Key(3)] public long StartDatetime { get; set; }

    [Key(4)] public long EndDatetime { get; set; }
}
