using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMEventQuestChapterCharacter))]
[PrimaryKey(nameof(EventQuestChapterId))]
public class EntityMEventQuestChapterCharacter
{
    [Key(0)] public int EventQuestChapterId { get; set; }

    [Key(1)] public int CharacterId { get; set; }
}
