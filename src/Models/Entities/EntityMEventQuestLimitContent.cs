using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMEventQuestLimitContent
{
    public int EventQuestLimitContentId { get; set; }

    public int CostumeId { get; set; }

    public int UnlockEvaluateConditionId { get; set; }

    public int SortOrder { get; set; }

    public int DeckGroupNumber { get; set; }

    public long StartDatetime { get; set; }

    public long EndDatetime { get; set; }

    public int EventQuestLimitContentDeckRestrictionId { get; set; }
}
