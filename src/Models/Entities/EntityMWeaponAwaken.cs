using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMWeaponAwaken
{
    public int WeaponId { get; set; }

    public int WeaponAwakenEffectGroupId { get; set; }

    public int WeaponAwakenMaterialGroupId { get; set; }

    public int ConsumeGold { get; set; }

    public int LevelLimitUp { get; set; }
}
