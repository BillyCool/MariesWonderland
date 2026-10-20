using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

[PrimaryKey(nameof(UserId), nameof(TutorialType))]
public class EntityIUserTutorialProgress : IUserEntity
{
    public long UserId { get; set; }

    public TutorialType TutorialType { get; set; }

    public int ProgressPhase { get; set; }

    public int ChoiceId { get; set; }

    public long LatestVersion { get; set; }
}
