using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMFieldEffectDecreasePoint
{
    public int WeaponId { get; set; }

    public int FieldEffectAbilityId { get; set; }

    public int DecreasePoint { get; set; }
}
