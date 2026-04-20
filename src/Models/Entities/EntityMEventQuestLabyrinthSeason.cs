using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMEventQuestLabyrinthSeason))]
public class EntityMEventQuestLabyrinthSeason
{
    [Key(0)] public int EventQuestChapterId { get; set; }

    [Key(1)] public int SeasonNumber { get; set; }

    [Key(2)] public long StartDatetime { get; set; }

    [Key(3)] public long EndDatetime { get; set; }

    [Key(4)] public int SeasonRewardGroupId { get; set; }
}
