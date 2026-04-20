namespace MariesWonderland.Models.Entities;

/// <summary>
/// Server-side quest session record. Created when a quest starts, deleted when it finishes or is retired.
/// Tracks which deck slot the player selected so EXP can be applied only to units in that deck.
/// Never sent to the client.
/// </summary>
public class EntitySQuestSession : IUserEntity
{
    public long UserId { get; set; }
    public int QuestId { get; set; }
    public int UserDeckNumber { get; set; }
}
