using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMQuestReleaseConditionCharacterLevel
{
    public int QuestReleaseConditionId { get; set; }

    public int CharacterId { get; set; }

    public int CharacterLevel { get; set; }
}
