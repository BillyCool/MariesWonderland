using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCostumeDelete))]
public class EntityMCostumeDelete
{
    [Key(0)] public int CostumeId { get; set; }

    [Key(1)] public int DeleteConditionClearQuestId { get; set; }

    [Key(2)] public int CostumeAlternativeGroupId { get; set; }

    [Key(3)] public TutorialType DeleteCostumeTutorialType { get; set; }

    [Key(4)] public int MaterialReturnGiftGrantRouteType { get; set; }
}
