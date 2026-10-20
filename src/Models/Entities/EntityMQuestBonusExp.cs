using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMQuestBonusExp))]
[PrimaryKey(nameof(QuestBonusEffectId))]
public class EntityMQuestBonusExp
{
    [Key(0)] public int QuestBonusEffectId { get; set; }

    [Key(1)] public int ExpType { get; set; }

    [Key(2)] public int BonusValuePermil { get; set; }
}
