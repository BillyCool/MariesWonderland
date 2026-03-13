using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMCostumeEnhanced
{
    public int CostumeEnhancedId { get; set; }

    public int CostumeId { get; set; }

    public int LimitBreakCount { get; set; }

    public int Level { get; set; }

    public int ActiveSkillLevel { get; set; }

    public int AwakenCount { get; set; }
}
