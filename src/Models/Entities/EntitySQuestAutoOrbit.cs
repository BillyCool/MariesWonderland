using Microsoft.EntityFrameworkCore;

namespace MariesWonderland.Models.Entities;

/// <summary>
/// Server-side auto-orbit (auto-repeat) state: the drops granted by each cleared run of the current loop,
/// collected so the loop's end and <c>FinishAutoOrbit</c> can show them together. Never sent to the client.
/// </summary>
[PrimaryKey(nameof(UserId))]
public class EntitySQuestAutoOrbit : IUserEntity
{
    public long UserId { get; set; }
    public List<EntitySQuestAutoOrbitDrop> Drops { get; set; } = [];
}

public class EntitySQuestAutoOrbitDrop
{
    public int PossessionType { get; set; }
    public int PossessionId { get; set; }
    public int Count { get; set; }
    public bool IsAutoSale { get; set; }
}
