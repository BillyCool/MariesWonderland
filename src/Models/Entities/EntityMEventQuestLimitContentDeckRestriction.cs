using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMEventQuestLimitContentDeckRestriction
{
    public int EventQuestLimitContentDeckRestrictionId { get; set; }

    public int GroupIndex { get; set; }

    public int EventQuestLimitContentDeckRestrictionTargetId { get; set; }

    public long StartDatetime { get; set; }

    public long EndDatetime { get; set; }
}
