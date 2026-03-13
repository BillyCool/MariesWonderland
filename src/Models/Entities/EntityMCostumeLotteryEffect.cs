using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMCostumeLotteryEffect
{
    public int CostumeId { get; set; }

    public int SlotNumber { get; set; }

    public int CostumeLotteryEffectOddsGroupId { get; set; }

    public int CostumeLotteryEffectUnlockMaterialGroupId { get; set; }

    public int CostumeLotteryEffectDrawMaterialGroupId { get; set; }

    public int CostumeLotteryEffectReleaseScheduleId { get; set; }
}
