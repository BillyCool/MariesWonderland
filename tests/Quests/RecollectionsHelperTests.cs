using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Quests;

namespace MariesWonderland.Tests.Quests;

/// <summary>Recollections of Dusk floors: locking the deck used on a clear, lifting locks when a floor is done, resets.</summary>
public class RecollectionsHelperTests
{
    private const long UserId = 3;
    private const long Now = 5_000;
    private const int CostumeChapter = 500001;   // locks costumes only
    private const int WeaponChapter = 500002;    // locks costumes and weapons
    private static readonly int[] NormalFloor = [1, 2, 3];
    private static readonly int[] HardFloor = [11, 12];

    private static DarkMasterMemoryDatabase Master()
    {
        DarkMasterMemoryDatabase masterDb = new();
        AddChapter(masterDb, CostumeChapter, contentId: 1, restrictionId: 2, floors: [(1, NormalFloor), (2, HardFloor)]);
        AddChapter(masterDb, WeaponChapter, contentId: 2, restrictionId: 1, floors: [(1, [21, 22])]);
        masterDb.EntityMEventQuestLimitContentDeckRestriction.AddRange(
        [
            new EntityMEventQuestLimitContentDeckRestriction { EventQuestLimitContentDeckRestrictionId = 1, GroupIndex = 1, EventQuestLimitContentDeckRestrictionTargetId = 1, StartDatetime = 0, EndDatetime = 10_000 },
            new EntityMEventQuestLimitContentDeckRestriction { EventQuestLimitContentDeckRestrictionId = 2, GroupIndex = 1, EventQuestLimitContentDeckRestrictionTargetId = 2, StartDatetime = 0, EndDatetime = 10_000 }
        ]);
        masterDb.EntityMEventQuestLimitContentDeckRestrictionTarget.AddRange(
        [
            new EntityMEventQuestLimitContentDeckRestrictionTarget { EventQuestLimitContentDeckRestrictionTargetId = 1, LimitContentDeckRestrictionType = LimitContentDeckRestrictionType.COSTUME },
            new EntityMEventQuestLimitContentDeckRestrictionTarget { EventQuestLimitContentDeckRestrictionTargetId = 1, LimitContentDeckRestrictionType = LimitContentDeckRestrictionType.WEAPON },
            new EntityMEventQuestLimitContentDeckRestrictionTarget { EventQuestLimitContentDeckRestrictionTargetId = 2, LimitContentDeckRestrictionType = LimitContentDeckRestrictionType.COSTUME }
        ]);
        return masterDb;
    }

    private static void AddChapter(DarkMasterMemoryDatabase masterDb, int chapterId, int contentId, int restrictionId, (int Difficulty, int[] QuestIds)[] floors)
    {
        masterDb.EntityMEventQuestChapter.Add(new EntityMEventQuestChapter { EventQuestChapterId = chapterId, EventQuestType = EventQuestType.LIMIT_CONTENT, EventQuestSequenceGroupId = chapterId });
        masterDb.EntityMEventQuestChapterLimitContentRelation.Add(new EntityMEventQuestChapterLimitContentRelation { EventQuestChapterId = chapterId, EventQuestLimitContentId = contentId });
        masterDb.EntityMEventQuestLimitContent.Add(new EntityMEventQuestLimitContent { EventQuestLimitContentId = contentId, EventQuestLimitContentDeckRestrictionId = restrictionId });
        foreach ((int difficulty, int[] questIds) in floors)
        {
            int sequenceId = chapterId * 10 + difficulty;
            masterDb.EntityMEventQuestSequenceGroup.Add(new EntityMEventQuestSequenceGroup { EventQuestSequenceGroupId = chapterId, DifficultyType = (DifficultyType)difficulty, EventQuestSequenceId = sequenceId });
            for (int i = 0; i < questIds.Length; i++)
            {
                masterDb.EntityMEventQuestSequence.Add(new EntityMEventQuestSequence { EventQuestSequenceId = sequenceId, SortOrder = i + 1, QuestId = questIds[i] });
            }
        }
    }

    /// <summary>A user with one Recollections deck: two characters, the first also carrying a sub weapon.</summary>
    private static (DarkUserMemoryDatabase UserDb, EntityIUserDeck Deck) UserWithDeck()
    {
        DarkUserMemoryDatabase userDb = new() { UserId = UserId };
        EntityIUserDeck deck = new() { UserId = UserId, DeckType = DeckType.RESTRICTED_LIMIT_CONTENT_QUEST, UserDeckNumber = 101, UserDeckCharacterUuid01 = "dc1", UserDeckCharacterUuid02 = "dc2" };
        userDb.EntityIUserDeck.Add(deck);
        userDb.EntityIUserDeckCharacter.AddRange(
        [
            new EntityIUserDeckCharacter { UserId = UserId, UserDeckCharacterUuid = "dc1", UserCostumeUuid = "costume-1", MainUserWeaponUuid = "weapon-1" },
            new EntityIUserDeckCharacter { UserId = UserId, UserDeckCharacterUuid = "dc2", UserCostumeUuid = "costume-2", MainUserWeaponUuid = "weapon-2" }
        ]);
        userDb.EntityIUserDeckSubWeaponGroup.Add(new EntityIUserDeckSubWeaponGroup { UserId = UserId, UserDeckCharacterUuid = "dc1", UserWeaponUuid = "sub-1" });
        return (userDb, deck);
    }

    private static string[] Locked(DarkUserMemoryDatabase userDb, PossessionType type)
        => [.. userDb.EntityIUserDeckLimitContentRestricted.Where(r => r.PossessionType == type).Select(r => r.TargetUuid).Order()];

    [Fact]
    public void FloorQuestIds_IsTheQuestsOfOneDifficulty()
    {
        Assert.Equal(NormalFloor, RecollectionsHelper.FloorQuestIds(Master(), CostumeChapter, 2).Order());
        Assert.Equal(HardFloor, RecollectionsHelper.FloorQuestIds(Master(), CostumeChapter, 11).Order());
        Assert.Empty(RecollectionsHelper.FloorQuestIds(Master(), CostumeChapter, 99));
    }

    [Fact]
    public void Clear_MarksTheQuest_AndLocksTheDecksCostumes()
    {
        (DarkUserMemoryDatabase userDb, EntityIUserDeck deck) = UserWithDeck();

        RecollectionsHelper.RecordClear(Master(), userDb, CostumeChapter, 1, deck, Now);

        EntityIUserQuestLimitContentStatus status = Assert.Single(userDb.EntityIUserQuestLimitContentStatus);
        Assert.Equal((1, 1, CostumeChapter), (status.QuestId, status.LimitContentQuestStatusType, status.EventQuestChapterId));
        Assert.Equal(["costume-1", "costume-2"], Locked(userDb, PossessionType.COSTUME));
        Assert.Empty(Locked(userDb, PossessionType.WEAPON));
        Assert.All(userDb.EntityIUserDeckLimitContentRestricted, r => Assert.Equal((CostumeChapter, 1), (r.EventQuestChapterId, r.QuestId)));
        Assert.Equal(2, userDb.EntityIUserDeckLimitContentRestricted.Select(r => r.DeckRestrictedUuid).Distinct().Count());

        // Clearing the same quest again doesn't add the same locks twice.
        RecollectionsHelper.RecordClear(Master(), userDb, CostumeChapter, 1, deck, Now);
        Assert.Equal(2, userDb.EntityIUserDeckLimitContentRestricted.Count);
    }

    [Fact]
    public void Clear_OnAWeaponFloor_AlsoLocksMainAndSubWeapons()
    {
        (DarkUserMemoryDatabase userDb, EntityIUserDeck deck) = UserWithDeck();

        RecollectionsHelper.RecordClear(Master(), userDb, WeaponChapter, 21, deck, Now);

        Assert.Equal(["costume-1", "costume-2"], Locked(userDb, PossessionType.COSTUME));
        Assert.Equal(["sub-1", "weapon-1", "weapon-2"], Locked(userDb, PossessionType.WEAPON));
    }

    [Fact]
    public void Clear_OutsideTheRestrictionsTerm_LocksNothing()
    {
        (DarkUserMemoryDatabase userDb, EntityIUserDeck deck) = UserWithDeck();

        RecollectionsHelper.RecordClear(Master(), userDb, CostumeChapter, 1, deck, 10_001);

        Assert.Empty(userDb.EntityIUserDeckLimitContentRestricted);
        Assert.Single(userDb.EntityIUserQuestLimitContentStatus);
    }

    [Fact]
    public void ClearingTheWholeFloor_LiftsItsLocks_ButNotOtherFloors()
    {
        (DarkUserMemoryDatabase userDb, EntityIUserDeck deck) = UserWithDeck();
        RecollectionsHelper.RecordClear(Master(), userDb, CostumeChapter, 11, deck, Now);

        RecollectionsHelper.RecordClear(Master(), userDb, CostumeChapter, 1, deck, Now);
        RecollectionsHelper.RecordClear(Master(), userDb, CostumeChapter, 2, deck, Now);
        Assert.Contains(userDb.EntityIUserDeckLimitContentRestricted, r => r.QuestId == 2);

        RecollectionsHelper.RecordClear(Master(), userDb, CostumeChapter, 3, deck, Now);

        // The normal floor is done: its locks are gone, its quests stay cleared; the hard floor's lock remains.
        Assert.All(userDb.EntityIUserDeckLimitContentRestricted, r => Assert.Equal(11, r.QuestId));
        Assert.NotEmpty(userDb.EntityIUserDeckLimitContentRestricted);
        Assert.Equal(4, userDb.EntityIUserQuestLimitContentStatus.Count);
    }

    [Fact]
    public void ResetFloor_StartsTheWholeFloorOver_AndLeavesOtherFloorsAlone()
    {
        (DarkUserMemoryDatabase userDb, EntityIUserDeck deck) = UserWithDeck();
        RecollectionsHelper.RecordClear(Master(), userDb, CostumeChapter, 1, deck, Now);
        RecollectionsHelper.RecordClear(Master(), userDb, CostumeChapter, 2, deck, Now);
        RecollectionsHelper.RecordClear(Master(), userDb, CostumeChapter, 11, deck, Now);

        RecollectionsHelper.ResetFloor(Master(), userDb, CostumeChapter, 1);

        Assert.Equal([11], userDb.EntityIUserQuestLimitContentStatus.Select(s => s.QuestId));
        Assert.All(userDb.EntityIUserDeckLimitContentRestricted, r => Assert.Equal(11, r.QuestId));
        Assert.NotEmpty(userDb.EntityIUserDeckLimitContentRestricted);
    }
}
