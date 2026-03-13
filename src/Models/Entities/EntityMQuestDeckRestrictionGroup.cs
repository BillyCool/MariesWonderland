using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMQuestDeckRestrictionGroup
{
    public int QuestDeckRestrictionGroupId { get; set; }

    public int SlotNumber { get; set; }

    public QuestDeckRestrictionType QuestDeckRestrictionType { get; set; }

    public int RestrictionValue { get; set; }
}
