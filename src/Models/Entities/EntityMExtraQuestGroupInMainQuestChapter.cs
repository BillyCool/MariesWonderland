using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMExtraQuestGroupInMainQuestChapter))]
[PrimaryKey(nameof(MainQuestChapterId), nameof(ExtraQuestIndex))]
public class EntityMExtraQuestGroupInMainQuestChapter
{
    [Key(0)] public int MainQuestChapterId { get; set; }

    [Key(1)] public int ExtraQuestIndex { get; set; }

    [Key(2)] public int ExtraQuestId { get; set; }
}
