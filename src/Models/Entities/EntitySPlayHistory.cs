using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

/// <summary>
/// A lifetime play-history counter (<see cref="GamePlayHistoryType"/>) that no client table holds, such as metres
/// walked in The Cage. Never sent to the client; read back through <c>GetUserGamePlayNote</c>.
/// </summary>
[PrimaryKey(nameof(UserId), nameof(GamePlayHistoryType))]
public class EntitySPlayHistory : IUserEntity
{
    public long UserId { get; set; }

    public GamePlayHistoryType GamePlayHistoryType { get; set; }

    public long Count { get; set; }
}
