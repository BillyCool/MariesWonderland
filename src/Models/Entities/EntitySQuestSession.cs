using Microsoft.EntityFrameworkCore;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Models.Entities;

/// <summary>
/// Server-side quest session record. Created when a quest starts, deleted when it finishes or is retired.
/// Tracks which deck slot the player selected so EXP can be applied only to units in that deck.
/// Never sent to the client.
/// </summary>
[PrimaryKey(nameof(UserId), nameof(QuestId))]
public class EntitySQuestSession : IUserEntity
{
    public long UserId { get; set; }
    public int QuestId { get; set; }
    public int UserDeckNumber { get; set; }
    /// <summary>
    /// Which kind of deck <see cref="UserDeckNumber"/> is: the normal quest deck, or a restricted one for quests with a
    /// deck restriction (see <c>QuestEngine.DeckTypeFor</c>). UNKNOWN on sessions saved before this was stored.
    /// </summary>
    public DeckType UserDeckType { get; set; }
    public int CurrentEventQuestChapterId { get; set; }
    public int CurrentQuestSceneId { get; set; }
    public int HeadQuestSceneId { get; set; }
    public QuestFlowType CurrentQuestFlowType { get; set; }
    public bool IsBattleOnly { get; set; }

    /// <summary>Started from the quest menu: the story position stays put and the fields below are restored at finish.</summary>
    public bool IsMenuPick { get; set; }
    public QuestFlowType SavedQuestFlowType { get; set; }
    public bool SavedPortalCageInProgress { get; set; }
    public int SavedSideStoryQuestId { get; set; }
    public int SavedSideStoryQuestSceneId { get; set; }
}
