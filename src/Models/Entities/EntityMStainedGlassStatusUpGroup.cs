using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMStainedGlassStatusUpGroup
{
    public int StainedGlassStatusUpGroupId { get; set; }

    public int GroupIndex { get; set; }

    public StatusKindType StatusKindType { get; set; }

    public StatusCalculationType StatusCalculationType { get; set; }

    public int EffectValue { get; set; }
}
