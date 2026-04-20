namespace MariesWonderland.Models.Entities;

public class EntitySGachaBannerState : IUserEntity
{
    public long UserId { get; set; }

    public int GachaId { get; set; }

    public int BoxNumber { get; set; } = 1;

    public int DrawCount { get; set; }

    public int MedalCount { get; set; }

    public int StepNumber { get; set; }

    public int LoopCount { get; set; }

    public Dictionary<int, int> BoxDrewCounts { get; set; } = [];
}
