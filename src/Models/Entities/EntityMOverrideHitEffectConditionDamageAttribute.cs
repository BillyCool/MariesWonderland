using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMOverrideHitEffectConditionDamageAttribute
{
    public int OverrideHitEffectConditionId { get; set; }

    public bool IsExcepting { get; set; }

    public AttributeType AttributeType { get; set; }
}
