using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(GachaId))]
public class EntitySGachaBannerState : IUserEntity
{
    public long UserId { get; set; }

    public int GachaId { get; set; }

    public int BoxNumber { get; set; } = 1;

    public int DrawCount { get; set; }

    public int MedalCount { get; set; }

    public int StepNumber { get; set; }

    public int LoopCount { get; set; }

    /// <summary>Box summons: prizes won so far, by prize index (a chapter summon's featured prizes, refilled monthly).</summary>
    public Dictionary<int, int> BoxDrewCounts { get; set; } = [];

    /// <summary>Box summons: when <see cref="BoxDrewCounts"/> was last refilled (Unix millis; chapter summons refill monthly).</summary>
    public long BoxRefillDatetime { get; set; }
}
