using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMEventQuestLabyrinthQuestDisplay))]
[PrimaryKey(nameof(QuestId))]
public class EntityMEventQuestLabyrinthQuestDisplay
{
    [Key(0)] public int QuestId { get; set; }

    [Key(1)] public int BgAssetId { get; set; }

    [Key(2)] public int MobId { get; set; }
}
