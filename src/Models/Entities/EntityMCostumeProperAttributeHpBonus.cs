using MariesWonderland.MasterMemory;
using MariesWonderland.Models.Type;
using MessagePack;

namespace MariesWonderland.Models.Entities;

[MessagePackObject]
[MemoryTable(nameof(EntityMCostumeProperAttributeHpBonus))]
public class EntityMCostumeProperAttributeHpBonus
{
    [Key(0)] public int CostumeId { get; set; }

    [Key(1)] public AttributeType CostumeProperAttributeType { get; set; }

    [Key(2)] public int MainWeaponHpAdditionalValue { get; set; }

    [Key(3)] public int SubWeaponHpAdditionalValue { get; set; }
}
