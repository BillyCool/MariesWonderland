using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCageOrnamentMainQuestChapterStill))]
[PrimaryKey(nameof(MainQuestChapterId))]
public class EntityMCageOrnamentMainQuestChapterStill
{
    [Key(0)] public int MainQuestChapterId { get; set; }

    [Key(1)] public int CageOrnamentStillReleaseConditionId { get; set; }
}
