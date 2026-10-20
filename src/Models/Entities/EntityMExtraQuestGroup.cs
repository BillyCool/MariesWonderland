using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMExtraQuestGroup))]
[PrimaryKey(nameof(QuestId), nameof(ExtraQuestIndex))]
public class EntityMExtraQuestGroup
{
    [Key(0)] public int QuestId { get; set; }

    [Key(1)] public int ExtraQuestIndex { get; set; }

    [Key(2)] public int ExtraQuestId { get; set; }
}
