using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMPartsLevelUpRateGroup
{
    public int PartsLevelUpRateGroupId { get; set; }

    public int LevelLowerLimit { get; set; }

    public int SuccessRatePermil { get; set; }
}
