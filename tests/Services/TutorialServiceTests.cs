using MariesWonderland.Constants;
using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Deck;
using MariesWonderland.Proto.Tutorial;
using MariesWonderland.Tests.Infrastructure;
using TutorialServiceImpl = MariesWonderland.Services.TutorialService;

namespace MariesWonderland.Tests.Services;

public class TutorialServiceTests
{
    private const long UserId = 1L;
    private const int SkipTicketId = 2003;

    [Fact]
    public async Task SetTutorialProgress_SkipTicketPickupGivesTicketsOnce()
    {
        DarkUserMemoryDatabase userDb = CreateUserDb();
        TutorialServiceImpl service = CreateService(new DarkMasterMemoryDatabase(), userDb);

        SetTutorialProgressResponse first = await service.SetTutorialProgress(
            new SetTutorialProgressRequest { TutorialType = (int)TutorialType.MAIN_QUEST_3_3_TICKET_GET_1, ProgressPhase = 20, ChoiceId = 1 },
            FakeServerCallContext.For(UserId));
        SetTutorialProgressResponse replay = await service.SetTutorialProgress(
            new SetTutorialProgressRequest { TutorialType = (int)TutorialType.MAIN_QUEST_3_3_TICKET_GET_1, ProgressPhase = 20, ChoiceId = 1 },
            FakeServerCallContext.For(UserId));

        TutorialChoiceReward reward = Assert.Single(first.TutorialChoiceReward);
        Assert.Equal((int)PossessionType.CONSUMABLE_ITEM, reward.PossessionType);
        Assert.Equal(SkipTicketId, reward.PossessionId);
        Assert.Equal(20, reward.Count);
        Assert.Empty(replay.TutorialChoiceReward);
        Assert.Equal(20, Assert.Single(userDb.EntityIUserConsumableItem).Count);

        EntityIUserTutorialProgress progress = Assert.Single(userDb.EntityIUserTutorialProgress);
        Assert.Equal(20, progress.ProgressPhase);
        Assert.Equal(1, progress.ChoiceId);
    }

    [Theory]
    [InlineData(1, 2)]
    [InlineData(2, 1)]
    [InlineData(3, 15)]
    [InlineData(4, 14)]
    public async Task SetTutorialProgress_CompanionChoiceGivesTheStarterCompanionOnce(int choiceId, int companionId)
    {
        DarkUserMemoryDatabase userDb = CreateUserDb();
        TutorialServiceImpl service = CreateService(new DarkMasterMemoryDatabase(), userDb);
        SetTutorialProgressRequest request = new() { TutorialType = (int)TutorialType.COMPANION, ProgressPhase = 10, ChoiceId = choiceId };

        SetTutorialProgressResponse first = await service.SetTutorialProgress(request, FakeServerCallContext.For(UserId));
        await service.SetTutorialProgress(new SetTutorialProgressRequest { TutorialType = (int)TutorialType.COMPANION, ProgressPhase = 40 }, FakeServerCallContext.For(UserId));
        SetTutorialProgressResponse replay = await service.SetTutorialProgress(request, FakeServerCallContext.For(UserId));

        TutorialChoiceReward reward = Assert.Single(first.TutorialChoiceReward);
        Assert.Equal((int)PossessionType.COMPANION, reward.PossessionType);
        Assert.Equal(companionId, reward.PossessionId);
        Assert.Empty(replay.TutorialChoiceReward);
        Assert.Equal(companionId, Assert.Single(userDb.EntityIUserCompanion).CompanionId);
    }

    [Fact]
    public async Task SetTutorialProgress_SecondPickupAndLibraryBooksGiveTenEach()
    {
        DarkUserMemoryDatabase userDb = CreateUserDb();
        TutorialServiceImpl service = CreateService(new DarkMasterMemoryDatabase(), userDb);

        TutorialType[] pickups =
        [
            TutorialType.MAIN_QUEST_3_3_TICKET_GET_2,
            TutorialType.MAIN_QUEST_3_3_TICKET_GET_3,
            TutorialType.MAIN_QUEST_3_3_TICKET_GET_4,
            TutorialType.MAIN_QUEST_3_3_TICKET_GET_5
        ];
        foreach (TutorialType pickup in pickups)
        {
            SetTutorialProgressResponse response = await service.SetTutorialProgress(
                new SetTutorialProgressRequest { TutorialType = (int)pickup, ProgressPhase = 20, ChoiceId = 1 },
                FakeServerCallContext.For(UserId));
            Assert.Equal(10, Assert.Single(response.TutorialChoiceReward).Count);
        }

        Assert.Equal(40, Assert.Single(userDb.EntityIUserConsumableItem).Count);
    }

    [Fact]
    public async Task SetTutorialProgress_BarrierUsesOneTicketOnlyTheFirstTime()
    {
        DarkMasterMemoryDatabase masterDb = MasterWithTicketUse();
        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserConsumableItem.Add(new EntityIUserConsumableItem { UserId = UserId, ConsumableItemId = SkipTicketId, Count = 30 });
        TutorialServiceImpl service = CreateService(masterDb, userDb);

        SetTutorialProgressRequest yes = new() { TutorialType = (int)TutorialType.MAIN_QUEST_3_3_TICKET_CONSUME, ProgressPhase = 20, ChoiceId = 1 };
        SetTutorialProgressResponse response = await service.SetTutorialProgress(yes, FakeServerCallContext.For(UserId));
        await service.SetTutorialProgress(yes, FakeServerCallContext.For(UserId));

        Assert.Empty(response.TutorialChoiceReward);
        Assert.Equal(29, Assert.Single(userDb.EntityIUserConsumableItem).Count);
    }

    [Fact]
    public async Task SetTutorialProgress_BarrierTakesNothingWithoutATicket()
    {
        DarkUserMemoryDatabase userDb = CreateUserDb();
        TutorialServiceImpl service = CreateService(MasterWithTicketUse(), userDb);

        await service.SetTutorialProgress(
            new SetTutorialProgressRequest { TutorialType = (int)TutorialType.MAIN_QUEST_3_3_TICKET_CONSUME, ProgressPhase = 20, ChoiceId = 1 },
            FakeServerCallContext.For(UserId));

        Assert.Empty(userDb.EntityIUserConsumableItem);
        Assert.Equal(1, Assert.Single(userDb.EntityIUserTutorialProgress).ChoiceId);
    }

    private static DarkMasterMemoryDatabase MasterWithTicketUse()
    {
        DarkMasterMemoryDatabase masterDb = new();
        masterDb.EntityMTutorialConsumePossessionGroup.Add(new EntityMTutorialConsumePossessionGroup
        {
            TutorialType = TutorialType.MAIN_QUEST_3_3_TICKET_CONSUME,
            PossessionType = PossessionType.CONSUMABLE_ITEM,
            PossessionId = SkipTicketId,
            Count = 1
        });
        return masterDb;
    }

    [Fact]
    public async Task SetTutorialProgress_DoesNotOverwriteHigherExistingProgress()
    {
        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserTutorialProgress.Add(new EntityIUserTutorialProgress
        {
            UserId = UserId,
            TutorialType = TutorialType.EXPLORE,
            ProgressPhase = 5,
            ChoiceId = 7
        });

        TutorialServiceImpl service = CreateService(new DarkMasterMemoryDatabase(), userDb);

        SetTutorialProgressResponse response = await service.SetTutorialProgress(
            new SetTutorialProgressRequest
            {
                TutorialType = (int)TutorialType.EXPLORE,
                ProgressPhase = 4,
                ChoiceId = 99
            },
            FakeServerCallContext.For(UserId));

        EntityIUserTutorialProgress progress = Assert.Single(userDb.EntityIUserTutorialProgress);
        Assert.Equal(5, progress.ProgressPhase);
        Assert.Equal(7, progress.ChoiceId);
        Assert.Empty(response.TutorialChoiceReward);
    }

    [Fact]
    public async Task SetTutorialProgress_ForMenuTutorialCreatesStarterDeckOnce()
    {
        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserCostume.Add(new EntityIUserCostume
        {
            UserId = UserId,
            UserCostumeUuid = "starter-costume",
            CostumeId = GameConstants.StartingDeckCostumeId
        });
        userDb.EntityIUserWeapon.Add(new EntityIUserWeapon
        {
            UserId = UserId,
            UserWeaponUuid = "starter-weapon",
            WeaponId = GameConstants.StartingDeckWeaponId
        });

        TutorialServiceImpl service = CreateService(new DarkMasterMemoryDatabase(), userDb);

        await service.SetTutorialProgress(
            new SetTutorialProgressRequest
            {
                TutorialType = (int)TutorialType.MENU_FIRST,
                ProgressPhase = 1
            },
            FakeServerCallContext.For(UserId));

        EntityIUserDeck deck = Assert.Single(userDb.EntityIUserDeck);
        string starterDeckCharacterUuid = deck.UserDeckCharacterUuid01;

        Assert.Equal(DeckType.QUEST, deck.DeckType);
        Assert.Equal(1, deck.UserDeckNumber);
        Assert.Equal("Loadout 1", deck.Name);
        Assert.NotEmpty(starterDeckCharacterUuid);

        EntityIUserDeckCharacter starterCharacter = Assert.Single(userDb.EntityIUserDeckCharacter);
        Assert.Equal(starterDeckCharacterUuid, starterCharacter.UserDeckCharacterUuid);
        Assert.Equal("starter-costume", starterCharacter.UserCostumeUuid);
        Assert.Equal("starter-weapon", starterCharacter.MainUserWeaponUuid);
        Assert.Single(userDb.EntityIUserDeckTypeNote, note => note.DeckType == DeckType.QUEST);

        await service.SetTutorialProgress(
            new SetTutorialProgressRequest
            {
                TutorialType = (int)TutorialType.MENU_SECOND,
                ProgressPhase = 2
            },
            FakeServerCallContext.For(UserId));

        deck = Assert.Single(userDb.EntityIUserDeck);
        Assert.Equal(starterDeckCharacterUuid, deck.UserDeckCharacterUuid01);
        Assert.Single(userDb.EntityIUserDeckCharacter);
        Assert.Single(userDb.EntityIUserDeckTypeNote);
    }

    [Fact]
    public async Task SetTutorialProgress_ForMenuTutorialMakesNoDeckBeforeTheStartingWeaponIsOwned()
    {
        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserCostume.Add(new EntityIUserCostume
        {
            UserId = UserId,
            UserCostumeUuid = "starter-costume",
            CostumeId = GameConstants.StartingDeckCostumeId
        });
        userDb.EntityIUserWeapon.Add(new EntityIUserWeapon
        {
            UserId = UserId,
            UserWeaponUuid = "prologue-weapon",
            WeaponId = 100001
        });

        TutorialServiceImpl service = CreateService(new DarkMasterMemoryDatabase(), userDb);

        await service.SetTutorialProgress(
            new SetTutorialProgressRequest
            {
                TutorialType = (int)TutorialType.MENU_FIRST,
                ProgressPhase = 1
            },
            FakeServerCallContext.For(UserId));

        Assert.Empty(userDb.EntityIUserDeck);
        Assert.Empty(userDb.EntityIUserDeckCharacter);
        Assert.Single(userDb.EntityIUserTutorialProgress, progress => progress.TutorialType == TutorialType.MENU_FIRST);

        userDb.EntityIUserWeapon.Add(new EntityIUserWeapon
        {
            UserId = UserId,
            UserWeaponUuid = "starter-weapon",
            WeaponId = GameConstants.StartingDeckWeaponId
        });

        await service.SetTutorialProgress(
            new SetTutorialProgressRequest
            {
                TutorialType = (int)TutorialType.MENU_SECOND,
                ProgressPhase = 2
            },
            FakeServerCallContext.For(UserId));

        EntityIUserDeckCharacter starterCharacter = Assert.Single(userDb.EntityIUserDeckCharacter);
        Assert.Equal("starter-weapon", starterCharacter.MainUserWeaponUuid);
        Assert.Single(userDb.EntityIUserDeck);
    }

    [Fact]
    public async Task SetTutorialProgressAndReplaceDeck_ReplacesExistingDeckRecordsAndResetsChoiceId()
    {
        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserTutorialProgress.Add(new EntityIUserTutorialProgress
        {
            UserId = UserId,
            TutorialType = TutorialType.EXPLORE,
            ProgressPhase = 1,
            ChoiceId = 5
        });

        AddDeckCharacter(
            userDb,
            "old-1",
            "costume-old-1",
            "main-old-1",
            "comp-old-1",
            "thought-old-1",
            dressupCostumeId: 10,
            parts: ["part-old"],
            subWeapons: ["sub-old"]);
        AddDeckCharacter(userDb, "keep", "costume-keep", "main-keep", "comp-keep", "thought-keep");
        EntityIUserDeck deck = AddDeck(userDb, DeckType.MULTI, 2, "Existing Deck", "old-1");

        TutorialServiceImpl service = CreateService(new DarkMasterMemoryDatabase(), userDb);

        await service.SetTutorialProgressAndReplaceDeck(
            new SetTutorialProgressAndReplaceDeckRequest
            {
                TutorialType = (int)TutorialType.EXPLORE,
                ProgressPhase = 4,
                DeckType = (int)DeckType.MULTI,
                UserDeckNumber = 2,
                Deck = new Deck
                {
                    Character01 = new DeckCharacter
                    {
                        UserCostumeUuid = "costume-new",
                        MainUserWeaponUuid = "main-new",
                        UserCompanionUuid = "comp-new",
                        UserThoughtUuid = "thought-new",
                        DressupCostumeId = 30,
                        UserPartsUuid = { "part-a", "part-b" },
                        SubUserWeaponUuid = { "sub-a", "sub-b" }
                    }
                }
            },
            FakeServerCallContext.For(UserId));

        EntityIUserTutorialProgress progress = Assert.Single(userDb.EntityIUserTutorialProgress);
        Assert.Equal(4, progress.ProgressPhase);
        Assert.Equal(0, progress.ChoiceId);

        Assert.Equal("Existing Deck", deck.Name);
        Assert.NotEqual("old-1", deck.UserDeckCharacterUuid01);
        Assert.Equal(string.Empty, deck.UserDeckCharacterUuid02);
        Assert.Equal(string.Empty, deck.UserDeckCharacterUuid03);

        Assert.DoesNotContain(userDb.EntityIUserDeckCharacter, character => character.UserDeckCharacterUuid == "old-1");
        Assert.DoesNotContain(userDb.EntityIUserDeckCharacterDressupCostume, dressup => dressup.UserDeckCharacterUuid == "old-1");
        Assert.DoesNotContain(userDb.EntityIUserDeckPartsGroup, parts => parts.UserDeckCharacterUuid == "old-1");
        Assert.DoesNotContain(userDb.EntityIUserDeckSubWeaponGroup, weapon => weapon.UserDeckCharacterUuid == "old-1");
        Assert.Contains(userDb.EntityIUserDeckCharacter, character => character.UserDeckCharacterUuid == "keep");

        EntityIUserDeckCharacter newCharacter = Assert.Single(
            userDb.EntityIUserDeckCharacter,
            character => character.UserDeckCharacterUuid == deck.UserDeckCharacterUuid01);
        Assert.Equal("costume-new", newCharacter.UserCostumeUuid);
        Assert.Equal("main-new", newCharacter.MainUserWeaponUuid);
        Assert.Equal("comp-new", newCharacter.UserCompanionUuid);
        Assert.Equal("thought-new", newCharacter.UserThoughtUuid);
        Assert.Equal(30, Assert.Single(userDb.EntityIUserDeckCharacterDressupCostume, dressup => dressup.UserDeckCharacterUuid == deck.UserDeckCharacterUuid01).DressupCostumeId);
        Assert.Collection(
            userDb.EntityIUserDeckPartsGroup.Where(parts => parts.UserDeckCharacterUuid == deck.UserDeckCharacterUuid01).OrderBy(parts => parts.SortOrder),
            parts => Assert.Equal("part-a", parts.UserPartsUuid),
            parts => Assert.Equal("part-b", parts.UserPartsUuid));
        Assert.Collection(
            userDb.EntityIUserDeckSubWeaponGroup.Where(weapon => weapon.UserDeckCharacterUuid == deck.UserDeckCharacterUuid01).OrderBy(weapon => weapon.SortOrder),
            weapon => Assert.Equal("sub-a", weapon.UserWeaponUuid),
            weapon => Assert.Equal("sub-b", weapon.UserWeaponUuid));
    }

    private static TutorialServiceImpl CreateService(DarkMasterMemoryDatabase masterDb, DarkUserMemoryDatabase userDb)
    {
        UserDataStore store = new(TimeProvider.System);
        store.Set(UserId, userDb);
        return new TutorialServiceImpl(store, masterDb, new GameConfig { ConsumableItemIdForQuestSkipTicket = SkipTicketId }, TimeProvider.System);
    }

    private static DarkUserMemoryDatabase CreateUserDb() => new();

    private static EntityIUserDeck AddDeck(
        DarkUserMemoryDatabase userDb,
        DeckType deckType,
        int deckNumber,
        string name,
        params string[] characterUuids)
    {
        EntityIUserDeck deck = new()
        {
            UserId = UserId,
            DeckType = deckType,
            UserDeckNumber = deckNumber,
            Name = name,
            Power = 123,
            UserDeckCharacterUuid01 = characterUuids.ElementAtOrDefault(0) ?? string.Empty,
            UserDeckCharacterUuid02 = characterUuids.ElementAtOrDefault(1) ?? string.Empty,
            UserDeckCharacterUuid03 = characterUuids.ElementAtOrDefault(2) ?? string.Empty
        };
        userDb.EntityIUserDeck.Add(deck);
        return deck;
    }

    private static void AddDeckCharacter(
        DarkUserMemoryDatabase userDb,
        string uuid,
        string costumeUuid,
        string mainWeaponUuid,
        string companionUuid,
        string thoughtUuid,
        int dressupCostumeId = 0,
        string[]? parts = null,
        string[]? subWeapons = null)
    {
        userDb.EntityIUserDeckCharacter.Add(new EntityIUserDeckCharacter
        {
            UserId = UserId,
            UserDeckCharacterUuid = uuid,
            UserCostumeUuid = costumeUuid,
            MainUserWeaponUuid = mainWeaponUuid,
            UserCompanionUuid = companionUuid,
            UserThoughtUuid = thoughtUuid,
            Power = 77
        });

        if (dressupCostumeId != 0)
        {
            userDb.EntityIUserDeckCharacterDressupCostume.Add(new EntityIUserDeckCharacterDressupCostume
            {
                UserId = UserId,
                UserDeckCharacterUuid = uuid,
                DressupCostumeId = dressupCostumeId
            });
        }

        if (parts != null)
        {
            for (int i = 0; i < parts.Length; i++)
            {
                userDb.EntityIUserDeckPartsGroup.Add(new EntityIUserDeckPartsGroup
                {
                    UserId = UserId,
                    UserDeckCharacterUuid = uuid,
                    UserPartsUuid = parts[i],
                    SortOrder = i + 1
                });
            }
        }

        if (subWeapons == null)
        {
            return;
        }

        for (int i = 0; i < subWeapons.Length; i++)
        {
            userDb.EntityIUserDeckSubWeaponGroup.Add(new EntityIUserDeckSubWeaponGroup
            {
                UserId = UserId,
                UserDeckCharacterUuid = uuid,
                UserWeaponUuid = subWeapons[i],
                SortOrder = i + 1
            });
        }
    }
}
