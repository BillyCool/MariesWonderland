using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMCostumeAwakenStatusUpGroup
{
    public int CostumeAwakenStatusUpGroupId { get; set; }

    public int SortOrder { get; set; }

    public StatusKindType StatusKindType { get; set; }

    public StatusCalculationType StatusCalculationType { get; set; }

    public int EffectValue { get; set; }
}
