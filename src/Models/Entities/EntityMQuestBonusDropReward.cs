using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMQuestBonusDropReward))]
[PrimaryKey(nameof(QuestBonusEffectId))]
public class EntityMQuestBonusDropReward
{
    [Key(0)] public int QuestBonusEffectId { get; set; }

    [Key(1)] public PossessionType PossessionType { get; set; }

    [Key(2)] public int PossessionId { get; set; }

    [Key(3)] public int AdditionalCount { get; set; }
}
