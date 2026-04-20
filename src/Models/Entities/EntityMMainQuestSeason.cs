using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMMainQuestSeason))]
public class EntityMMainQuestSeason
{
    [Key(0)] public int MainQuestSeasonId { get; set; }

    [Key(1)] public int SortOrder { get; set; }
}
