using MariesWonderland.Data;
using MariesWonderland.Helpers;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Quest;

namespace MariesWonderland.Tests.Helpers;

/// <summary>Beginner and comeback quest campaigns: half stamina and bonus drop items, only for those players.</summary>
public class QuestCampaignUserStatusTests
{
    private const long DayMs = 24L * 60 * 60 * 1000;
    private const long Start = 100 * DayMs;
    private const long End = 1000 * DayMs;
    private const long Since = 200 * DayMs;
    private const int BonusItemId = 77;

    private static readonly QuestCampaignTarget AnyQuest = new(1, QuestType.MAIN_QUEST);

    private static QuestCampaignHelper Build(TargetUserStatusType userStatus)
    {
        DarkMasterMemoryDatabase masterDb = new();
        AddCampaign(masterDb, 1, userStatus, QuestCampaignEffectType.STAMINA_CONSUME_AMOUNT, 500, itemGroupId: 0);
        AddCampaign(masterDb, 2, userStatus, QuestCampaignEffectType.DROP_ITEM_ADD, 1000, itemGroupId: 2);
        masterDb.EntityMQuestCampaignTargetItemGroup.Add(new EntityMQuestCampaignTargetItemGroup
        {
            QuestCampaignTargetItemGroupId = 2,
            PossessionType = PossessionType.CONSUMABLE_ITEM,
            PossessionId = BonusItemId,
            Count = 3
        });
        return new QuestCampaignHelper(masterDb);
    }

    private static void AddCampaign(DarkMasterMemoryDatabase masterDb, int id, TargetUserStatusType userStatus, QuestCampaignEffectType effect, int value, int itemGroupId)
    {
        masterDb.EntityMQuestCampaign.Add(new EntityMQuestCampaign
        {
            QuestCampaignId = id,
            QuestCampaignTargetGroupId = id,
            QuestCampaignEffectGroupId = id,
            StartDatetime = Start,
            EndDatetime = End,
            TargetUserStatusType = userStatus
        });
        masterDb.EntityMQuestCampaignTargetGroup.Add(new EntityMQuestCampaignTargetGroup
        {
            QuestCampaignTargetGroupId = id,
            QuestCampaignTargetType = QuestCampaignTargetType.WHOLE_QUEST
        });
        masterDb.EntityMQuestCampaignEffectGroup.Add(new EntityMQuestCampaignEffectGroup
        {
            QuestCampaignEffectGroupId = id,
            QuestCampaignEffectType = effect,
            QuestCampaignEffectValue = value,
            QuestCampaignTargetItemGroupId = itemGroupId
        });
    }

    private static UserCampaignStatus Beginner(long sinceMs) => new(sinceMs, sinceMs + 7 * DayMs, null, 0);

    private static UserCampaignStatus Comeback(long sinceMs) => new(null, 0, sinceMs, sinceMs + 7 * DayMs);

    [Fact]
    public void BeginnerCampaign_HalvesStaminaAndAddsItems_OnlyForAnActiveBeginner()
    {
        QuestCampaignHelper campaigns = Build(TargetUserStatusType.BEGINNER);
        long now = Since + DayMs;

        Assert.Equal(5, campaigns.ApplyStaminaCampaign(10, AnyQuest, now, Beginner(Since)));
        QuestReward bonus = Assert.Single(campaigns.ApplyDropCampaignBonuses([], AnyQuest, now, userStatus: Beginner(Since)));
        Assert.Equal(BonusItemId, bonus.PossessionId);
        Assert.Equal(3, bonus.Count);

        // Not a beginner, a returning player instead, or past the 7-day term.
        Assert.Equal(10, campaigns.ApplyStaminaCampaign(10, AnyQuest, now));
        Assert.Equal(10, campaigns.ApplyStaminaCampaign(10, AnyQuest, now, Comeback(Since)));
        Assert.Equal(10, campaigns.ApplyStaminaCampaign(10, AnyQuest, Since + 7 * DayMs + 1, Beginner(Since)));
        Assert.Empty(campaigns.ApplyDropCampaignBonuses([], AnyQuest, now));
    }

    [Fact]
    public void ComebackCampaign_NeedsTheComebackDateInsideTheCampaign()
    {
        QuestCampaignHelper campaigns = Build(TargetUserStatusType.COMEBACK);

        Assert.Equal(5, campaigns.ApplyStaminaCampaign(10, AnyQuest, Since + DayMs, Comeback(Since)));
        Assert.Equal(10, campaigns.ApplyStaminaCampaign(10, AnyQuest, Start + DayMs, Comeback(Start - 1)));
    }

    [Fact]
    public void UserCampaignStatus_ReadsTheUserRowsAndMasterTerms()
    {
        DarkMasterMemoryDatabase masterDb = new();
        masterDb.EntityMBeginnerCampaign.Add(new EntityMBeginnerCampaign { BeginnerCampaignId = 1, GrantCampaignTermDayCount = 7 });
        masterDb.EntityMComebackCampaign.Add(new EntityMComebackCampaign { ComebackCampaignId = 2, GrantCampaignTermDayCount = 14 });
        DarkUserMemoryDatabase userDb = new() { UserId = 5 };
        userDb.EntityIUserBeginnerCampaign.Add(new EntityIUserBeginnerCampaign { UserId = 5, BeginnerCampaignId = 1, CampaignRegisterDatetime = Since });
        userDb.EntityIUserComebackCampaign.Add(new EntityIUserComebackCampaign { UserId = 5, ComebackCampaignId = 2, ComebackDatetime = Since });

        UserCampaignStatus status = UserCampaignStatus.Of(masterDb, userDb);

        Assert.Equal(new UserCampaignStatus(Since, Since + 7 * DayMs, Since, Since + 14 * DayMs), status);
        Assert.Equal(UserCampaignStatus.None, UserCampaignStatus.Of(masterDb, new DarkUserMemoryDatabase()));
        Assert.False(status.Covers(TargetUserStatusType.COMEBACK_GRADE, Start, End, Since));
    }
}
