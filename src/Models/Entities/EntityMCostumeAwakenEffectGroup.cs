using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMCostumeAwakenEffectGroup
{
    public int CostumeAwakenEffectGroupId { get; set; }

    public int AwakenStep { get; set; }

    public CostumeAwakenEffectType CostumeAwakenEffectType { get; set; }

    public int CostumeAwakenEffectId { get; set; }
}
