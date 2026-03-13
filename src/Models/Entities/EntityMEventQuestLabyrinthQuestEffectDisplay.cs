using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMEventQuestLabyrinthQuestEffectDisplay
{
    public int QuestId { get; set; }

    public int SortOrder { get; set; }

    public LabyrinthQuestEffectDescriptionType LabyrinthQuestEffectDescriptionType { get; set; }

    public int EventQuestLabyrinthQuestEffectDescriptionId { get; set; }

    public AttributeType EffectTargetWeaponAttributeType { get; set; }
}
