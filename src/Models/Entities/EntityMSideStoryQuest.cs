using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSideStoryQuest))]
public class EntityMSideStoryQuest
{
    [Key(0)] public int SideStoryQuestId { get; set; }

    [Key(1)] public int SideStoryQuestType { get; set; }

    [Key(2)] public int TargetId { get; set; }
}
