using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMImportantItemEffectTargetItemGroup
{
    public int ImportantItemEffectTargetItemGroupId { get; set; }

    public int TargetIndex { get; set; }

    public PossessionType PossessionType { get; set; }

    public int PossessionId { get; set; }
}
