using MariesWonderland.Data;
using MariesWonderland.Helpers;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Tests.Helpers;

/// <summary>Starting beginner and comeback campaigns: who qualifies and what the user rows hold.</summary>
public class UserCampaignHelperTests
{
    private const long DayMs = 24L * 60 * 60 * 1000;
    private const long JudgeStart = 100 * DayMs;
    private const long JudgeEnd = 1000 * DayMs;
    private const int UnlockQuestId = 1;

    private static DarkMasterMemoryDatabase Master()
    {
        DarkMasterMemoryDatabase masterDb = new();
        masterDb.EntityMBeginnerCampaign.Add(new EntityMBeginnerCampaign
        {
            BeginnerCampaignId = 1,
            BeginnerJudgeStartDatetime = JudgeStart,
            BeginnerJudgeEndDatetime = JudgeEnd,
            GrantCampaignTermDayCount = 7,
            CampaignUnlockQuestId = UnlockQuestId
        });
        masterDb.EntityMComebackCampaign.Add(new EntityMComebackCampaign
        {
            ComebackCampaignId = 2,
            ComebackJudgeStartDatetime = JudgeStart,
            ComebackJudgeEndDatetime = JudgeEnd,
            ComebackJudgeDayCount = 28,
            GrantCampaignTermDayCount = 7,
            CampaignUnlockQuestId = UnlockQuestId
        });
        return masterDb;
    }

    private static DarkUserMemoryDatabase User(long registeredMs, bool unlockQuestCleared = true)
    {
        DarkUserMemoryDatabase userDb = new() { UserId = 9 };
        userDb.EntityIUser.Add(new EntityIUser { UserId = 9, RegisterDatetime = registeredMs });
        if (unlockQuestCleared)
        {
            userDb.EntityIUserQuest.Add(new EntityIUserQuest { UserId = 9, QuestId = UnlockQuestId, QuestStateType = (int)QuestStateType.CLEARED });
        }
        return userDb;
    }

    [Fact]
    public void Beginner_StartsOnFirstClearOfTheUnlockQuest_ForAnAccountRegisteredInTheWindow()
    {
        DarkUserMemoryDatabase userDb = User(JudgeStart + DayMs);
        long clearedAt = JudgeStart + 2 * DayMs;

        UserCampaignHelper.StartBeginnerCampaignOnFirstClear(Master(), userDb, UnlockQuestId, clearedAt);

        EntityIUserBeginnerCampaign row = Assert.Single(userDb.EntityIUserBeginnerCampaign);
        Assert.Equal(9, row.UserId);
        Assert.Equal(1, row.BeginnerCampaignId);
        Assert.Equal(clearedAt, row.CampaignRegisterDatetime);
    }

    [Fact]
    public void Beginner_NotForAccountsRegisteredBeforeTheWindow_OrOtherQuests_OrTwice()
    {
        DarkUserMemoryDatabase oldAccount = User(JudgeStart - 1);
        UserCampaignHelper.StartBeginnerCampaignOnFirstClear(Master(), oldAccount, UnlockQuestId, JudgeStart + DayMs);
        Assert.Empty(oldAccount.EntityIUserBeginnerCampaign);

        DarkUserMemoryDatabase otherQuest = User(JudgeStart + DayMs);
        UserCampaignHelper.StartBeginnerCampaignOnFirstClear(Master(), otherQuest, 2, JudgeStart + DayMs);
        Assert.Empty(otherQuest.EntityIUserBeginnerCampaign);

        DarkUserMemoryDatabase already = User(JudgeStart + DayMs);
        already.EntityIUserBeginnerCampaign.Add(new EntityIUserBeginnerCampaign { UserId = 9, BeginnerCampaignId = 1, CampaignRegisterDatetime = 5 });
        UserCampaignHelper.StartBeginnerCampaignOnFirstClear(Master(), already, UnlockQuestId, JudgeStart + 3 * DayMs);
        Assert.Equal(5, Assert.Single(already.EntityIUserBeginnerCampaign).CampaignRegisterDatetime);
    }

    [Fact]
    public void Comeback_StartsAfterEnoughDaysAway_AndReplacesAnOlderOne()
    {
        DarkUserMemoryDatabase userDb = User(1);
        userDb.EntityIUserComebackCampaign.Add(new EntityIUserComebackCampaign { UserId = 9, ComebackCampaignId = 1, ComebackDatetime = 3 });
        long lastLogin = JudgeStart;
        long now = lastLogin + 28 * DayMs;

        UserCampaignHelper.StartComebackCampaignOnLogin(Master(), userDb, lastLogin, now);

        EntityIUserComebackCampaign row = Assert.Single(userDb.EntityIUserComebackCampaign);
        Assert.Equal(2, row.ComebackCampaignId);
        Assert.Equal(now, row.ComebackDatetime);
    }

    [Fact]
    public void Comeback_NotWhenAwayTooShort_OutsideTheWindow_FirstLogin_OrUnlockQuestNotCleared()
    {
        long lastLogin = JudgeStart;

        DarkUserMemoryDatabase tooShort = User(1);
        UserCampaignHelper.StartComebackCampaignOnLogin(Master(), tooShort, lastLogin, lastLogin + 28 * DayMs - 1);
        Assert.Empty(tooShort.EntityIUserComebackCampaign);

        DarkUserMemoryDatabase afterWindow = User(1);
        UserCampaignHelper.StartComebackCampaignOnLogin(Master(), afterWindow, JudgeEnd, JudgeEnd + 30 * DayMs);
        Assert.Empty(afterWindow.EntityIUserComebackCampaign);

        DarkUserMemoryDatabase firstLogin = User(1);
        UserCampaignHelper.StartComebackCampaignOnLogin(Master(), firstLogin, 0, JudgeStart + DayMs);
        Assert.Empty(firstLogin.EntityIUserComebackCampaign);

        DarkUserMemoryDatabase notCleared = User(1, unlockQuestCleared: false);
        UserCampaignHelper.StartComebackCampaignOnLogin(Master(), notCleared, lastLogin, lastLogin + 30 * DayMs);
        Assert.Empty(notCleared.EntityIUserComebackCampaign);
    }
}
