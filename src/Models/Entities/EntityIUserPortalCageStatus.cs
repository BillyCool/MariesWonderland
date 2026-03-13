using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserPortalCageStatus
{
    public long UserId { get; set; }

    public bool IsCurrentProgress { get; set; }

    public long DropItemStartDatetime { get; set; }

    public int CurrentDropItemCount { get; set; }

    public long LatestVersion { get; set; }
}
