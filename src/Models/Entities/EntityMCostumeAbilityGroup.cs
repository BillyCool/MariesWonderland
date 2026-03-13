using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMCostumeAbilityGroup
{
    public int CostumeAbilityGroupId { get; set; }

    public int SlotNumber { get; set; }

    public int AbilityId { get; set; }

    public int CostumeAbilityLevelGroupId { get; set; }
}
