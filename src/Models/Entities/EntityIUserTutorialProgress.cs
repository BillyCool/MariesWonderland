using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserTutorialProgress
{
    public long UserId { get; set; }

    public TutorialType TutorialType { get; set; }

    public int ProgressPhase { get; set; }

    public int ChoiceId { get; set; }

    public long LatestVersion { get; set; }
}
