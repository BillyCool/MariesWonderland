using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMEventQuestChapterLimitContentRelation))]
[PrimaryKey(nameof(EventQuestChapterId))]
public class EntityMEventQuestChapterLimitContentRelation
{
    [Key(0)] public int EventQuestChapterId { get; set; }

    [Key(1)] public int EventQuestLimitContentId { get; set; }
}
