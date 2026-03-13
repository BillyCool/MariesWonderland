using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMFieldEffectGroup
{
    public int FieldEffectGroupId { get; set; }

    public int FieldEffectGroupIndex { get; set; }

    public int AbilityId { get; set; }

    public int DefaultAbilityLevel { get; set; }

    public FieldEffectApplyScopeType FieldEffectApplyScopeType { get; set; }

    public int FieldEffectAssetId { get; set; }
}
