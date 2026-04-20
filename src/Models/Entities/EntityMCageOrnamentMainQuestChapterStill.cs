using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCageOrnamentMainQuestChapterStill))]
public class EntityMCageOrnamentMainQuestChapterStill
{
    [Key(0)] public int MainQuestChapterId { get; set; }

    [Key(1)] public int CageOrnamentStillReleaseConditionId { get; set; }
}
