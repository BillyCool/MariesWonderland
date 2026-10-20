using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMQuestBonusCostumeGroup))]
[PrimaryKey(nameof(QuestBonusCostumeGroupId), nameof(CostumeId))]
public class EntityMQuestBonusCostumeGroup
{
    [Key(0)] public int QuestBonusCostumeGroupId { get; set; }

    [Key(1)] public int CostumeId { get; set; }

    [Key(2)] public int QuestBonusEffectGroupId { get; set; }

    [Key(3)] public int QuestBonusTermGroupId { get; set; }
}
