using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMEventQuestSequenceGroup
{
    // Properties
    public int EventQuestSequenceGroupId { get; set; }

    public DifficultyType DifficultyType { get; set; }

    public int EventQuestSequenceId { get; set; }
}
