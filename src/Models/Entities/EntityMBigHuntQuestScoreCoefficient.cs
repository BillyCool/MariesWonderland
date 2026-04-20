using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMBigHuntQuestScoreCoefficient))]
public class EntityMBigHuntQuestScoreCoefficient
{
    [Key(0)] public int BigHuntQuestScoreCoefficientId { get; set; }

    [Key(1)] public int ScoreDifficultBonusPermil { get; set; }
}
