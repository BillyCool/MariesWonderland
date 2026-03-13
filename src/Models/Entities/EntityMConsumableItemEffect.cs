using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMConsumableItemEffect
{
    public int ConsumableItemId { get; set; }

    public EffectTargetType EffectTargetType { get; set; }

    public EffectValueType EffectValueType { get; set; }

    public int EffectValue { get; set; }
}
