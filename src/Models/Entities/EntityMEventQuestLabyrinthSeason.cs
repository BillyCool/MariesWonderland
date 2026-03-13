using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMEventQuestLabyrinthSeason
{
    public int EventQuestChapterId { get; set; }

    public int SeasonNumber { get; set; }

    public long StartDatetime { get; set; }

    public long EndDatetime { get; set; }

    public int SeasonRewardGroupId { get; set; }
}
