using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMSideStoryQuest))]
[PrimaryKey(nameof(SideStoryQuestId))]
public class EntityMSideStoryQuest
{
    [Key(0)] public int SideStoryQuestId { get; set; }

    [Key(1)] public int SideStoryQuestType { get; set; }

    [Key(2)] public int TargetId { get; set; }
}
