using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMEventQuestLabyrinthSeasonRewardGroup))]
[PrimaryKey(nameof(EventQuestLabyrinthSeasonRewardGroupId), nameof(HeadQuestId))]
public class EntityMEventQuestLabyrinthSeasonRewardGroup
{
    [Key(0)] public int EventQuestLabyrinthSeasonRewardGroupId { get; set; }

    [Key(1)] public int HeadQuestId { get; set; }

    [Key(2)] public int EventQuestLabyrinthRewardGroupId { get; set; }
}
