using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMImportantItemEffectTargetQuestGroup
{
    public int ImportantItemEffectTargetQuestGroupId { get; set; }

    public int TargetIndex { get; set; }

    public ImportantItemEffectTargetQuestGroupType ImportantItemEffectTargetQuestGroupType { get; set; }

    public int TargetValue { get; set; }
}
