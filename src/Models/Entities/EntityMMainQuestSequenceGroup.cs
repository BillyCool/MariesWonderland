using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityMMainQuestSequenceGroup
{
    public int MainQuestSequenceGroupId { get; set; }

    public DifficultyType DifficultyType { get; set; }

    public int MainQuestSequenceId { get; set; }
}
