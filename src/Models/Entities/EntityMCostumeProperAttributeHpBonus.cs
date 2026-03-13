using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMCostumeProperAttributeHpBonus
{
    public int CostumeId { get; set; }

    public AttributeType CostumeProperAttributeType { get; set; }

    public int MainWeaponHpAdditionalValue { get; set; }

    public int SubWeaponHpAdditionalValue { get; set; }
}
