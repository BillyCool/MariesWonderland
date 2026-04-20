using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCostumeLotteryEffect))]
public class EntityMCostumeLotteryEffect
{
    [Key(0)] public int CostumeId { get; set; }

    [Key(1)] public int SlotNumber { get; set; }

    [Key(2)] public int CostumeLotteryEffectOddsGroupId { get; set; }

    [Key(3)] public int CostumeLotteryEffectUnlockMaterialGroupId { get; set; }

    [Key(4)] public int CostumeLotteryEffectDrawMaterialGroupId { get; set; }

    [Key(5)] public int CostumeLotteryEffectReleaseScheduleId { get; set; }
}
