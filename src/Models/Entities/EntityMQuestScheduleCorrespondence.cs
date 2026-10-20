using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMQuestScheduleCorrespondence))]
[PrimaryKey(nameof(QuestId), nameof(QuestScheduleId))]
public class EntityMQuestScheduleCorrespondence
{
    [Key(0)] public int QuestId { get; set; }

    [Key(1)] public int QuestScheduleId { get; set; }
}
