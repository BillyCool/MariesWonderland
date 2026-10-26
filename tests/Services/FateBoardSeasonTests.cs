using MariesWonderland.Models.Entities;
using MariesWonderland.Quests;
using MariesWonderland.Tests.Infrastructure;

namespace MariesWonderland.Tests.Services;

/// <summary>The Fate Board season rotation replayed in the modded master file (two-week seasons until 2041-03).</summary>
public class FateBoardSeasonTests(MasterDatabaseFixture fixture) : ServiceTestBase(fixture), IClassFixture<MasterDatabaseFixture>
{
    [Theory]
    [InlineData(801)]
    [InlineData(802)]
    public void EachBoardHasExactlyOneSeasonRunning(int chapterId)
    {
        DateTimeOffset[] days =
        [
            new(2024, 5, 1, 0, 0, 0, TimeSpan.Zero),
            new(2026, 10, 26, 12, 0, 0, TimeSpan.Zero),
            new(2033, 6, 15, 2, 0, 0, TimeSpan.Zero),
            new(2041, 2, 20, 0, 0, 0, TimeSpan.Zero)
        ];
        foreach (DateTimeOffset day in days)
        {
            EntityMEventQuestLabyrinthSeason? season = FateBoardHelper.CurrentSeason(MasterDb, chapterId, day.ToUnixTimeMilliseconds());
            Assert.NotNull(season);
            Assert.Equal(14, Math.Round((season.EndDatetime - season.StartDatetime) / 86_400_000.0));
        }
    }

    [Fact]
    public void BothBoardsKeepTheirOriginalSeasons()
    {
        EntityMEventQuestLabyrinthSeason shadowOne = MasterDb.EntityMEventQuestLabyrinthSeason.First(s => s.EventQuestChapterId == 801 && s.SeasonNumber == 1);
        Assert.Equal(new DateTimeOffset(2023, 2, 28, 1, 59, 59, TimeSpan.Zero).ToUnixTimeMilliseconds(), shadowOne.EndDatetime);

        EntityMEventQuestLabyrinthSeason ternaryOne = MasterDb.EntityMEventQuestLabyrinthSeason.First(s => s.EventQuestChapterId == 802 && s.SeasonNumber == 1);
        Assert.Equal(new DateTimeOffset(2023, 3, 17, 1, 59, 59, TimeSpan.Zero).ToUnixTimeMilliseconds(), ternaryOne.EndDatetime);

        List<EntityMEventQuestLabyrinthSeason> ternary = [.. MasterDb.EntityMEventQuestLabyrinthSeason.Where(s => s.EventQuestChapterId == 802).OrderBy(s => s.SeasonNumber)];
        Assert.All(ternary.Zip(ternary.Skip(1)), pair => Assert.True(pair.First.EndDatetime < pair.Second.StartDatetime));
        long after = new DateTimeOffset(2041, 3, 15, 0, 0, 0, TimeSpan.Zero).ToUnixTimeMilliseconds();
        Assert.DoesNotContain(MasterDb.EntityMEventQuestLabyrinthSeason, s => s.EndDatetime > after);
    }
}
