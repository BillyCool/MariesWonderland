using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Deck;
using MariesWonderland.Tests.Infrastructure;
using DeckService = MariesWonderland.Services.DeckService;

namespace MariesWonderland.Tests.Services;

public class DeckServiceTests(MasterDatabaseFixture fixture) : ServiceTestBase(fixture), IClassFixture<MasterDatabaseFixture>
{
    private const long UserId = 1L;

    private DeckService CreateService(UserDataStore store) => new(store, MasterDb, TimeProvider.System);

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

        if (subWeapons != null)
        {
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

    [Fact]
    public async Task ReplaceDeck_RemovesStaleLinkedRecordsBeforeRecreatingSlots()
    {
        DarkUserMemoryDatabase userDb = CreateUserDb();
        AddDeckCharacter(userDb, "old-1", "costume-old-1", "main-old-1", "comp-old-1", "thought-old-1", dressupCostumeId: 10, parts: ["part-old-1"], subWeapons: ["sub-old-1"]);
        AddDeckCharacter(userDb, "old-2", "costume-old-2", "main-old-2", "comp-old-2", "thought-old-2", parts: ["part-old-2"], subWeapons: ["sub-old-2"]);
        AddDeckCharacter(userDb, "keep", "costume-keep", "main-keep", "comp-keep", "thought-keep", dressupCostumeId: 20, parts: ["part-keep"], subWeapons: ["sub-keep"]);
        EntityIUserDeck deck = AddDeck(userDb, DeckType.QUEST, 1, "Quest Deck", "old-1", "old-2");

        UserDataStore store = CreateStore(UserId, userDb);
        DeckService service = CreateService(store);

        await service.ReplaceDeck(
            new ReplaceDeckRequest
            {
                DeckType = (int)DeckType.QUEST,
                UserDeckNumber = 1,
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
            ContextFor(UserId));

        Assert.Equal("Quest Deck", deck.Name);
        Assert.DoesNotContain(userDb.EntityIUserDeckCharacter, dc => dc.UserDeckCharacterUuid is "old-1" or "old-2");
        Assert.DoesNotContain(userDb.EntityIUserDeckCharacterDressupCostume, dc => dc.UserDeckCharacterUuid is "old-1" or "old-2");
        Assert.DoesNotContain(userDb.EntityIUserDeckPartsGroup, pg => pg.UserDeckCharacterUuid is "old-1" or "old-2");
        Assert.DoesNotContain(userDb.EntityIUserDeckSubWeaponGroup, swg => swg.UserDeckCharacterUuid is "old-1" or "old-2");

        Assert.Contains(userDb.EntityIUserDeckCharacter, dc => dc.UserDeckCharacterUuid == "keep");
        Assert.NotEqual("old-1", deck.UserDeckCharacterUuid01);
        Assert.NotEmpty(deck.UserDeckCharacterUuid01);
        Assert.Equal(string.Empty, deck.UserDeckCharacterUuid02);
        Assert.Equal(string.Empty, deck.UserDeckCharacterUuid03);

        EntityIUserDeckCharacter newCharacter = Assert.Single(userDb.EntityIUserDeckCharacter, dc => dc.UserDeckCharacterUuid == deck.UserDeckCharacterUuid01);
        Assert.Equal("costume-new", newCharacter.UserCostumeUuid);
        Assert.Equal("main-new", newCharacter.MainUserWeaponUuid);
        Assert.Equal("comp-new", newCharacter.UserCompanionUuid);
        Assert.Equal("thought-new", newCharacter.UserThoughtUuid);
        Assert.Collection(
            userDb.EntityIUserDeckPartsGroup.Where(pg => pg.UserDeckCharacterUuid == deck.UserDeckCharacterUuid01).OrderBy(pg => pg.SortOrder),
            item => Assert.Equal("part-a", item.UserPartsUuid),
            item => Assert.Equal("part-b", item.UserPartsUuid));
        Assert.Collection(
            userDb.EntityIUserDeckSubWeaponGroup.Where(swg => swg.UserDeckCharacterUuid == deck.UserDeckCharacterUuid01).OrderBy(swg => swg.SortOrder),
            item => Assert.Equal("sub-a", item.UserWeaponUuid),
            item => Assert.Equal("sub-b", item.UserWeaponUuid));
        Assert.Equal(30, Assert.Single(userDb.EntityIUserDeckCharacterDressupCostume, dc => dc.UserDeckCharacterUuid == deck.UserDeckCharacterUuid01).DressupCostumeId);
    }

    [Fact]
    public async Task CopyDeck_CopiesSlotsToTargetWithFreshRecordsAndSourceName()
    {
        DarkUserMemoryDatabase userDb = CreateUserDb();
        EntityIUserDeck sourceDeck = AddDeck(userDb, DeckType.QUEST, 1, "Source Deck", "src-1", "src-2");
        AddDeckCharacter(userDb, "src-1", "costume-src-1", "main-src-1", "comp-src-1", "thought-src-1", dressupCostumeId: 100, parts: ["part-1", "part-2"], subWeapons: ["sub-1"]);
        AddDeckCharacter(userDb, "src-2", "costume-src-2", "main-src-2", "comp-src-2", "thought-src-2", parts: ["part-3"], subWeapons: ["sub-2", "sub-3"]);

        EntityIUserDeck targetDeck = AddDeck(userDb, DeckType.MULTI, 2, "Target Deck", "target-old");
        AddDeckCharacter(userDb, "target-old", "costume-target-old", "main-target-old", "comp-target-old", "thought-target-old", dressupCostumeId: 200, parts: ["part-target"], subWeapons: ["sub-target"]);
        AddDeckCharacter(userDb, "keep", "costume-keep", "main-keep", "comp-keep", "thought-keep", parts: ["part-keep"], subWeapons: ["sub-keep"]);

        UserDataStore store = CreateStore(UserId, userDb);
        DeckService service = CreateService(store);

        CopyDeckResponse response = await service.CopyDeck(
            new CopyDeckRequest
            {
                FromDeckType = (int)DeckType.QUEST,
                FromUserDeckNumber = 1,
                ToDeckType = (int)DeckType.MULTI,
                ToUserDeckNumber = 2
            },
            ContextFor(UserId));

        Assert.Equal(1, response.ResultType);
        Assert.Equal("Source Deck", targetDeck.Name);
        Assert.Equal("src-1", sourceDeck.UserDeckCharacterUuid01);
        Assert.Equal("src-2", sourceDeck.UserDeckCharacterUuid02);
        Assert.NotEqual("target-old", targetDeck.UserDeckCharacterUuid01);
        Assert.NotEqual("src-1", targetDeck.UserDeckCharacterUuid01);
        Assert.NotEqual("src-2", targetDeck.UserDeckCharacterUuid02);
        Assert.DoesNotContain(userDb.EntityIUserDeckCharacter, dc => dc.UserDeckCharacterUuid == "target-old");
        Assert.Contains(userDb.EntityIUserDeckCharacter, dc => dc.UserDeckCharacterUuid == "keep");

        EntityIUserDeckCharacter copied01 = Assert.Single(userDb.EntityIUserDeckCharacter, dc => dc.UserDeckCharacterUuid == targetDeck.UserDeckCharacterUuid01);
        EntityIUserDeckCharacter copied02 = Assert.Single(userDb.EntityIUserDeckCharacter, dc => dc.UserDeckCharacterUuid == targetDeck.UserDeckCharacterUuid02);
        Assert.Equal("costume-src-1", copied01.UserCostumeUuid);
        Assert.Equal("main-src-1", copied01.MainUserWeaponUuid);
        Assert.Equal("comp-src-1", copied01.UserCompanionUuid);
        Assert.Equal("thought-src-1", copied01.UserThoughtUuid);
        Assert.Equal("costume-src-2", copied02.UserCostumeUuid);
        Assert.Collection(
            userDb.EntityIUserDeckPartsGroup.Where(pg => pg.UserDeckCharacterUuid == targetDeck.UserDeckCharacterUuid01).OrderBy(pg => pg.SortOrder),
            item => Assert.Equal("part-1", item.UserPartsUuid),
            item => Assert.Equal("part-2", item.UserPartsUuid));
        Assert.Collection(
            userDb.EntityIUserDeckSubWeaponGroup.Where(swg => swg.UserDeckCharacterUuid == targetDeck.UserDeckCharacterUuid02).OrderBy(swg => swg.SortOrder),
            item => Assert.Equal("sub-2", item.UserWeaponUuid),
            item => Assert.Equal("sub-3", item.UserWeaponUuid));
        Assert.Equal(100, Assert.Single(userDb.EntityIUserDeckCharacterDressupCostume, dc => dc.UserDeckCharacterUuid == targetDeck.UserDeckCharacterUuid01).DressupCostumeId);
    }

    [Fact]
    public async Task CopyDeck_WithoutSourceDeck_LeavesTargetUntouched()
    {
        DarkUserMemoryDatabase userDb = CreateUserDb();
        EntityIUserDeck targetDeck = AddDeck(userDb, DeckType.MULTI, 2, "Target Deck", "target-old");
        AddDeckCharacter(userDb, "target-old", "costume-target-old", "main-target-old", "comp-target-old", "thought-target-old");

        UserDataStore store = CreateStore(UserId, userDb);
        DeckService service = CreateService(store);

        CopyDeckResponse response = await service.CopyDeck(
            new CopyDeckRequest
            {
                FromDeckType = (int)DeckType.QUEST,
                FromUserDeckNumber = 99,
                ToDeckType = (int)DeckType.MULTI,
                ToUserDeckNumber = 2
            },
            ContextFor(UserId));

        Assert.Equal(0, response.ResultType);
        Assert.Equal("Target Deck", targetDeck.Name);
        Assert.Equal("target-old", targetDeck.UserDeckCharacterUuid01);
        Assert.Single(userDb.EntityIUserDeckCharacter);
    }

    [Fact]
    public async Task UpdateTripleDeckName_CreatesOrUpdatesTripleDeckRecord()
    {
        DarkUserMemoryDatabase userDb = CreateUserDb();
        UserDataStore store = CreateStore(UserId, userDb);
        DeckService service = CreateService(store);

        await service.UpdateTripleDeckName(
            new UpdateTripleDeckNameRequest
            {
                DeckType = (int)DeckType.BIG_HUNT,
                UserDeckNumber = 5,
                Name = "Initial Preset"
            },
            ContextFor(UserId));

        await service.UpdateTripleDeckName(
            new UpdateTripleDeckNameRequest
            {
                DeckType = (int)DeckType.BIG_HUNT,
                UserDeckNumber = 5,
                Name = "Renamed Preset"
            },
            ContextFor(UserId));

        EntityIUserTripleDeck tripleDeck = Assert.Single(userDb.EntityIUserTripleDeck);
        Assert.Equal(DeckType.BIG_HUNT, tripleDeck.DeckType);
        Assert.Equal(5, tripleDeck.UserDeckNumber);
        Assert.Equal("Renamed Preset", tripleDeck.Name);
    }

    [Fact]
    public async Task ReplaceTripleDeck_PersistsTripleDeckDeckNumbersWithoutLosingName()
    {
        DarkUserMemoryDatabase userDb = CreateUserDb();
        userDb.EntityIUserTripleDeck.Add(new EntityIUserTripleDeck
        {
            UserId = UserId,
            DeckType = DeckType.BIG_HUNT,
            UserDeckNumber = 7,
            Name = "Subjugation Preset"
        });
        UserDataStore store = CreateStore(UserId, userDb);
        DeckService service = CreateService(store);

        await service.ReplaceTripleDeck(
            new ReplaceTripleDeckRequest
            {
                DeckType = (int)DeckType.BIG_HUNT,
                UserDeckNumber = 7,
                DeckDetail01 = new DeckDetail
                {
                    DeckType = (int)DeckType.BIG_HUNT,
                    UserDeckNumber = 11,
                    Deck = new Deck { Character01 = new DeckCharacter { UserCostumeUuid = "costume-1" } }
                },
                DeckDetail02 = new DeckDetail
                {
                    DeckType = (int)DeckType.BIG_HUNT,
                    UserDeckNumber = 12,
                    Deck = new Deck { Character01 = new DeckCharacter { UserCostumeUuid = "costume-2" } }
                },
                DeckDetail03 = new DeckDetail
                {
                    DeckType = (int)DeckType.BIG_HUNT,
                    UserDeckNumber = 13,
                    Deck = new Deck { Character01 = new DeckCharacter { UserCostumeUuid = "costume-3" } }
                }
            },
            ContextFor(UserId));

        EntityIUserTripleDeck tripleDeck = Assert.Single(userDb.EntityIUserTripleDeck);
        Assert.Equal("Subjugation Preset", tripleDeck.Name);
        Assert.Equal(11, tripleDeck.DeckNumber01);
        Assert.Equal(12, tripleDeck.DeckNumber02);
        Assert.Equal(13, tripleDeck.DeckNumber03);
        Assert.Equal(3, userDb.EntityIUserDeck.Count);
    }

    [Fact]
    public async Task RemoveDeck_RemovesDeckAndLinkedRecords()
    {
        DarkUserMemoryDatabase userDb = CreateUserDb();
        AddDeck(userDb, DeckType.PVP, 3, "Pvp Deck", "remove-1", "remove-2");
        AddDeckCharacter(userDb, "remove-1", "costume-remove-1", "main-remove-1", "comp-remove-1", "thought-remove-1", dressupCostumeId: 10, parts: ["part-remove-1"], subWeapons: ["sub-remove-1"]);
        AddDeckCharacter(userDb, "remove-2", "costume-remove-2", "main-remove-2", "comp-remove-2", "thought-remove-2", parts: ["part-remove-2"], subWeapons: ["sub-remove-2"]);
        AddDeckCharacter(userDb, "keep", "costume-keep", "main-keep", "comp-keep", "thought-keep", dressupCostumeId: 30, parts: ["part-keep"], subWeapons: ["sub-keep"]);
        AddDeck(userDb, DeckType.QUEST, 1, "Keep Deck", "keep");

        UserDataStore store = CreateStore(UserId, userDb);
        DeckService service = CreateService(store);

        await service.RemoveDeck(
            new RemoveDeckRequest
            {
                DeckType = (int)DeckType.PVP,
                UserDeckNumber = 3
            },
            ContextFor(UserId));

        Assert.DoesNotContain(userDb.EntityIUserDeck, deck => deck.DeckType == DeckType.PVP && deck.UserDeckNumber == 3);
        Assert.DoesNotContain(userDb.EntityIUserDeckCharacter, dc => dc.UserDeckCharacterUuid is "remove-1" or "remove-2");
        Assert.DoesNotContain(userDb.EntityIUserDeckCharacterDressupCostume, dc => dc.UserDeckCharacterUuid is "remove-1" or "remove-2");
        Assert.DoesNotContain(userDb.EntityIUserDeckPartsGroup, pg => pg.UserDeckCharacterUuid is "remove-1" or "remove-2");
        Assert.DoesNotContain(userDb.EntityIUserDeckSubWeaponGroup, swg => swg.UserDeckCharacterUuid is "remove-1" or "remove-2");
        Assert.Contains(userDb.EntityIUserDeck, deck => deck.DeckType == DeckType.QUEST && deck.UserDeckNumber == 1);
        Assert.Contains(userDb.EntityIUserDeckCharacter, dc => dc.UserDeckCharacterUuid == "keep");
    }
}
