using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMShopItemContentEffect
{
    public int ShopItemId { get; set; }

    public EffectTargetType EffectTargetType { get; set; }

    public EffectValueType EffectValueType { get; set; }

    public int EffectValue { get; set; }
}
