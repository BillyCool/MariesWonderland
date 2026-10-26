using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Quests;
using MariesWonderland.Tests.Infrastructure;

namespace MariesWonderland.Tests.Quests;

/// <summary>
/// The deck a quest run uses: normal quests use the quest deck, quests with a deck restriction a restricted deck
/// (Recollections of Dusk ones their own type), and EXP and loadout missions read that deck.
/// </summary>
public class QuestDeckTypeTests
{
    private const long UserId = 7;
    private const int NormalQuestId = 1;
    private const int RestrictedQuestId = 2;
    private const int RecollectionsQuestId = 3;
    private const int RecollectionsChapterId = 500001;
    private const int OtherEventChapterId = 600001;

    private static DarkMasterMemoryDatabase Master()
    {
        DarkMasterMemoryDatabase masterDb = new();
        masterDb.EntityMQuest.AddRange(
        [
            new EntityMQuest { QuestId = NormalQuestId },
            new EntityMQuest { QuestId = RestrictedQuestId, QuestDeckRestrictionGroupId = 10 },
            new EntityMQuest { QuestId = RecollectionsQuestId, QuestDeckRestrictionGroupId = 11 }
        ]);
        masterDb.EntityMEventQuestChapter.AddRange(
        [
            new EntityMEventQuestChapter { EventQuestChapterId = RecollectionsChapterId, EventQuestType = EventQuestType.LIMIT_CONTENT },
            new EntityMEventQuestChapter { EventQuestChapterId = OtherEventChapterId, EventQuestType = EventQuestType.CHARACTER }
        ]);
        masterDb.EntityMCostume.AddRange(
        [
            new EntityMCostume { CostumeId = 100, CharacterId = 1 },
            new EntityMCostume { CostumeId = 200, CharacterId = 2 }
        ]);
        return masterDb;
    }

    /// <summary>A user with deck number 101 twice: a normal quest deck wearing costume 100, a restricted one wearing 200.</summary>
    private static DarkUserMemoryDatabase UserWithTwoDecks(DeckType restrictedType)
    {
        DarkUserMemoryDatabase userDb = new() { UserId = UserId };
        AddDeck(userDb, DeckType.QUEST, "normal", 100);
        AddDeck(userDb, restrictedType, "restricted", 200);
        return userDb;
    }

    private static void AddDeck(DarkUserMemoryDatabase userDb, DeckType deckType, string name, int costumeId)
    {
        userDb.EntityIUserDeck.Add(new EntityIUserDeck { UserId = UserId, DeckType = deckType, UserDeckNumber = 101, UserDeckCharacterUuid01 = name + "-dc" });
        userDb.EntityIUserDeckCharacter.Add(new EntityIUserDeckCharacter { UserId = UserId, UserDeckCharacterUuid = name + "-dc", UserCostumeUuid = name + "-costume" });
        userDb.EntityIUserCostume.Add(new EntityIUserCostume { UserId = UserId, UserCostumeUuid = name + "-costume", CostumeId = costumeId });
    }

    [Fact]
    public void DeckTypeFor_FollowsTheQuestsDeckRestriction()
    {
        QuestEngine engine = TestServices.QuestEngine(Master());

        Assert.Equal(DeckType.QUEST, engine.DeckTypeFor(engine.FindQuest(NormalQuestId)));
        Assert.Equal(DeckType.RESTRICTED_QUEST, engine.DeckTypeFor(engine.FindQuest(RestrictedQuestId)));
        Assert.Equal(DeckType.RESTRICTED_QUEST, engine.DeckTypeFor(engine.FindQuest(RestrictedQuestId), OtherEventChapterId));
        Assert.Equal(DeckType.RESTRICTED_LIMIT_CONTENT_QUEST, engine.DeckTypeFor(engine.FindQuest(RecollectionsQuestId), RecollectionsChapterId));
        Assert.Equal(DeckType.QUEST, engine.DeckTypeFor(engine.FindQuest(NormalQuestId), RecollectionsChapterId));
    }

    [Fact]
    public void RestrictedQuest_ReadsTheRestrictedDeck_NotTheQuestDeckWithTheSameNumber()
    {
        QuestEngine engine = TestServices.QuestEngine(Master());
        DarkUserMemoryDatabase userDb = UserWithTwoDecks(DeckType.RESTRICTED_QUEST);
        EntitySQuestSession session = new() { UserId = UserId, QuestId = RestrictedQuestId };
        userDb.EntitySQuestSession.Add(session);

        engine.SetSessionDeck(session, 101);

        Assert.Equal(DeckType.RESTRICTED_QUEST, session.UserDeckType);
        Assert.Equal(DeckType.RESTRICTED_QUEST, QuestEngine.SessionDeck(userDb, RestrictedQuestId)?.DeckType);
        (HashSet<int> characters, HashSet<int> costumes) = engine.DeckMembers(userDb, RestrictedQuestId);
        Assert.Equal([2], characters);
        Assert.Equal([200], costumes);
    }

    [Fact]
    public void RecollectionsQuest_ReadsItsOwnDeckType()
    {
        QuestEngine engine = TestServices.QuestEngine(Master());
        DarkUserMemoryDatabase userDb = UserWithTwoDecks(DeckType.RESTRICTED_LIMIT_CONTENT_QUEST);
        EntitySQuestSession session = new() { UserId = UserId, QuestId = RecollectionsQuestId };
        userDb.EntitySQuestSession.Add(session);

        engine.SetSessionDeck(session, 101, RecollectionsChapterId);

        Assert.Equal([200], engine.DeckMembers(userDb, RecollectionsQuestId).CostumeIds);
    }

    [Fact]
    public void SessionWithoutAStoredType_ReadsTheQuestDeck()
    {
        QuestEngine engine = TestServices.QuestEngine(Master());
        DarkUserMemoryDatabase userDb = UserWithTwoDecks(DeckType.RESTRICTED_QUEST);
        userDb.EntitySQuestSession.Add(new EntitySQuestSession { UserId = UserId, QuestId = RestrictedQuestId, UserDeckNumber = 101 });

        Assert.Equal([100], engine.DeckMembers(userDb, RestrictedQuestId).CostumeIds);
    }
}
