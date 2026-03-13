using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMSideStoryQuestLimitContent
{
    public int SideStoryQuestLimitContentId { get; set; }

    public int CharacterId { get; set; }

    public int EventQuestChapterId { get; set; }

    public DifficultyType DifficultyType { get; set; }

    public int NextSideStoryQuestId { get; set; }
}
