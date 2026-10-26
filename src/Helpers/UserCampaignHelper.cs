using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Helpers;

/// <summary>
/// Starts a user's beginner and comeback campaigns by writing <c>IUserBeginnerCampaign</c> / <c>IUserComebackCampaign</c>.
/// The client only reads those rows: it shows the beginner and comeback enhance campaigns (and their end time, the
/// row's date plus the master campaign's term days) for them, and <see cref="EnhanceCampaignHelper"/> applies them.
/// </summary>
public static class UserCampaignHelper
{
    private const long DayMs = 24L * 60 * 60 * 1000;

    /// <summary>
    /// On the first clear of a beginner campaign's unlock quest, starts that campaign for an account registered inside
    /// its judge window. An account has at most one beginner campaign, ever.
    /// </summary>
    public static void StartBeginnerCampaignOnFirstClear(DarkMasterMemoryDatabase masterDb, DarkUserMemoryDatabase userDb, int questId, long nowMs)
    {
        if (userDb.EntityIUserBeginnerCampaign.Count > 0)
        {
            return;
        }

        long registeredMs = userDb.EntityIUser.FirstOrDefault()?.RegisterDatetime ?? 0;
        EntityMBeginnerCampaign? campaign = masterDb.EntityMBeginnerCampaign
            .Where(c => c.CampaignUnlockQuestId == questId
                && registeredMs >= c.BeginnerJudgeStartDatetime && registeredMs <= c.BeginnerJudgeEndDatetime)
            .OrderByDescending(c => c.BeginnerJudgeStartDatetime)
            .FirstOrDefault();
        if (campaign == null)
        {
            return;
        }

        userDb.EntityIUserBeginnerCampaign.Add(new EntityIUserBeginnerCampaign
        {
            UserId = userDb.UserId,
            BeginnerCampaignId = campaign.BeginnerCampaignId,
            CampaignRegisterDatetime = nowMs
        });
    }

    /// <summary>
    /// At a login, starts a comeback campaign when the user was away at least its judge day count since
    /// <paramref name="previousLoginMs"/>, the login is inside its judge window and its unlock quest is cleared.
    /// A new comeback replaces an earlier one. Nothing happens on a first login.
    /// </summary>
    public static void StartComebackCampaignOnLogin(DarkMasterMemoryDatabase masterDb, DarkUserMemoryDatabase userDb, long previousLoginMs, long nowMs)
    {
        if (previousLoginMs <= 0)
        {
            return;
        }

        EntityMComebackCampaign? campaign = masterDb.EntityMComebackCampaign
            .Where(c => nowMs >= c.ComebackJudgeStartDatetime && nowMs <= c.ComebackJudgeEndDatetime
                && nowMs - previousLoginMs >= c.ComebackJudgeDayCount * DayMs
                && IsQuestCleared(userDb, c.CampaignUnlockQuestId))
            .OrderByDescending(c => c.ComebackJudgeStartDatetime)
            .FirstOrDefault();
        if (campaign == null)
        {
            return;
        }

        EntityIUserComebackCampaign comeback = userDb.EntityIUserComebackCampaign.GetOrCreate(userDb.UserId);
        comeback.ComebackCampaignId = campaign.ComebackCampaignId;
        comeback.ComebackDatetime = nowMs;
    }

    private static bool IsQuestCleared(DarkUserMemoryDatabase userDb, int questId)
        => questId == 0 || userDb.EntityIUserQuest.Any(q => q.QuestId == questId && q.QuestStateType == (int)QuestStateType.CLEARED);
}
