using MariesWonderland.Helpers;

namespace MariesWonderland.Tests.Helpers;

/// <summary>Daily reset at 08:00 UTC, weekly reset on Monday at 08:00 UTC.</summary>
public class GameDayHelperTests
{
    private static long Ms(int year, int month, int day, int hour, int minute = 0)
        => new DateTimeOffset(year, month, day, hour, minute, 0, TimeSpan.Zero).ToUnixTimeMilliseconds();

    [Fact]
    public void StartOfDay_TurnsOverAt0800Utc()
    {
        Assert.Equal(Ms(2024, 4, 28, 8), GameDayHelper.StartOfDay(Ms(2024, 4, 29, 7, 59)));
        Assert.Equal(Ms(2024, 4, 29, 8), GameDayHelper.StartOfDay(Ms(2024, 4, 29, 8)));
        Assert.Equal(Ms(2024, 4, 29, 8), GameDayHelper.StartOfDay(Ms(2024, 4, 30, 7, 59)));
    }

    [Fact]
    public void StartOfWeek_IsMonday0800Utc_MatchingTheRealSave()
    {
        // 1645430400000 is the first BigHuntWeeklyVersion in the real save: Monday 2022-02-21 08:00 UTC.
        Assert.Equal(1645430400000, GameDayHelper.StartOfWeek(Ms(2022, 2, 21, 8)));
        Assert.Equal(1645430400000, GameDayHelper.StartOfWeek(Ms(2022, 2, 28, 7, 59)));
        Assert.Equal(1645430400000 - 7L * 24 * 60 * 60 * 1000, GameDayHelper.StartOfWeek(Ms(2022, 2, 21, 7, 59)));
    }

    [Fact]
    public void IsBeforeToday_ComparesAgainstTheLastReset()
    {
        long now = Ms(2024, 4, 29, 9);

        Assert.True(GameDayHelper.IsBeforeToday(Ms(2024, 4, 29, 7, 59), now));
        Assert.False(GameDayHelper.IsBeforeToday(Ms(2024, 4, 29, 8), now));
    }
}
