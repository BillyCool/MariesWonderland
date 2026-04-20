using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBigHuntQuest))]
public class EntityMBigHuntQuest
{
    [Key(0)] public int BigHuntQuestId { get; set; }

    [Key(1)] public int QuestId { get; set; }

    [Key(2)] public int BigHuntQuestScoreCoefficientId { get; set; }
}
