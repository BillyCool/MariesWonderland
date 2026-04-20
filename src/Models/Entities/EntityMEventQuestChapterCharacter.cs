using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMEventQuestChapterCharacter))]
public class EntityMEventQuestChapterCharacter
{
    [Key(0)] public int EventQuestChapterId { get; set; }

    [Key(1)] public int CharacterId { get; set; }
}
