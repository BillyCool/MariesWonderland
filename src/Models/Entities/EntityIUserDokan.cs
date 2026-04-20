using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

public class EntityIUserDokan : IUserEntity
{
    public long UserId { get; set; }

    public int DokanId { get; set; }

    public long DisplayDatetime { get; set; }

    public long LatestVersion { get; set; }
}
