using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMQuestBonusExp))]
public class EntityMQuestBonusExp
{
    [Key(0)] public int QuestBonusEffectId { get; set; }

    [Key(1)] public int ExpType { get; set; }

    [Key(2)] public int BonusValuePermil { get; set; }
}
