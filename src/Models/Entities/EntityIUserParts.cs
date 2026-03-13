using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserParts
{
    public long UserId { get; set; }

    public string UserPartsUuid { get; set; }

    public int PartsId { get; set; }

    public int Level { get; set; }

    public int PartsStatusMainId { get; set; }

    public bool IsProtected { get; set; }

    public long AcquisitionDatetime { get; set; }

    public long LatestVersion { get; set; }
}
