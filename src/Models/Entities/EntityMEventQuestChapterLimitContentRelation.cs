using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMEventQuestChapterLimitContentRelation))]
public class EntityMEventQuestChapterLimitContentRelation
{
    [Key(0)] public int EventQuestChapterId { get; set; }

    [Key(1)] public int EventQuestLimitContentId { get; set; }
}
