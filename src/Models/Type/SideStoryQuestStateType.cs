namespace MariesWonderland.Models.Type;

/// <summary>
/// <c>IUserSideStoryQuest.SideStoryQuestStateType</c>. The client has no enum for it and reads no value (only the head
/// scene); these come from the real save: 3 on every side story whose last scene was reached, 2 on one of them
/// (meaning unknown), 1 as the server's value for a story in progress.
/// </summary>
public enum SideStoryQuestStateType
{
    UNKNOWN = 0,
    ACTIVE = 1,
    CLEARED = 3
}
