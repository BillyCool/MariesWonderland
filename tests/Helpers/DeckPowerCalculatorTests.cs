using MariesWonderland.Configuration;
using MariesWonderland.Data;
using MariesWonderland.Helpers;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Tests.Infrastructure;
using Microsoft.Extensions.Options;

namespace MariesWonderland.Tests.Helpers;

/// <summary>
/// The force formula checked against the real save: its decks hold the force the client worked out when each was last
/// refreshed. The newest match closely; older ones are a little off, as the gear changed since (so the checks are loose).
/// </summary>
public class DeckPowerCalculatorTests(MasterDatabaseFixture fixture) : ServiceTestBase(fixture), IClassFixture<MasterDatabaseFixture>
{
    private static readonly string SavePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "src", "Data", "UserData"));

    private DarkUserMemoryDatabase LoadSave()
    {
        Assert.SkipUnless(Directory.Exists(SavePath), $"The real save isn't at '{SavePath}'.");
        UserDataSeeder seeder = new(Options.Create(new ServerOptions { Data = new DataOptions { UserDataPath = SavePath } }));
        DarkUserMemoryDatabase userDb = seeder.LoadFromFiles();
        Assert.SkipUnless(userDb.EntityIUserDeck.Count > 0, "The real save has no decks.");
        return userDb;
    }

    [Fact]
    public void Calculate_QuestAndArenaDecksOfTheRealSave_StayCloseToTheForceTheClientStored()
    {
        DarkUserMemoryDatabase userDb = LoadSave();

        List<double> errors = [];
        foreach (EntityIUserDeck deck in userDb.EntityIUserDeck.Where(d => d.DeckType is DeckType.QUEST or DeckType.PVP && d.Power > 0))
        {
            List<EntityIUserDeckCharacter> characters = MariesWonderland.Arena.ArenaDecks.DeckCharacters(userDb, deck.DeckType, deck.UserDeckNumber);
            int[] powers = DeckPowerCalculator.Calculate(userDb, MasterDb, characters);
            for (int i = 0; i < characters.Count; i++)
            {
                errors.Add(Math.Abs(powers[i] - characters[i].Power) / (double)characters[i].Power);
            }
        }

        Assert.True(errors.Count > 50, $"Only {errors.Count} characters were checked.");
        Assert.True(errors.Average() < 0.03, $"The force is off by {errors.Average():P1} on average.");
    }

    /// <summary>
    /// Quest decks 1 and 15 are the quest decks refreshed most recently (and use lottery effects, a companion and memoir
    /// sub statuses), so every input of the formula was current when the client stored their force: it must match exactly.
    /// </summary>
    [Theory]
    [InlineData(1)]
    [InlineData(15)]
    public void Calculate_TheNewestQuestDecksOfTheRealSave_MatchTheStoredForceExactly(int deckNumber)
    {
        DarkUserMemoryDatabase userDb = LoadSave();
        List<EntityIUserDeckCharacter> characters = MariesWonderland.Arena.ArenaDecks.DeckCharacters(userDb, DeckType.QUEST, deckNumber);

        int[] powers = DeckPowerCalculator.Calculate(userDb, MasterDb, characters);

        Assert.Equal(3, characters.Count);
        Assert.Equal(characters.Select(c => c.Power), powers);
    }

    [Fact]
    public void Refresh_ADeckSavedWithoutForce_GetsForceForItselfAndItsCharacters()
    {
        DarkUserMemoryDatabase userDb = LoadSave();
        EntityIUserDeck deck = userDb.EntityIUserDeck.First(d => d.DeckType == DeckType.PVP && d.UserDeckNumber == 1);
        List<EntityIUserDeckCharacter> characters = MariesWonderland.Arena.ArenaDecks.DeckCharacters(userDb, DeckType.PVP, 1);
        int stored = deck.Power;
        deck.Power = 0;
        foreach (EntityIUserDeckCharacter character in characters)
        {
            character.Power = 0;
        }

        DeckPowerCalculator.Refresh(userDb, MasterDb, DeckType.PVP, 1);

        Assert.InRange(deck.Power, stored * 0.95, stored * 1.05);
        Assert.All(characters, c => Assert.True(c.Power > 0));
        Assert.Equal(deck.Power, characters.Sum(c => c.Power));
    }
}
