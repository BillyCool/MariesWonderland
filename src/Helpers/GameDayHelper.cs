namespace MariesWonderland.Helpers;

/// <summary>
/// The game's daily and weekly reset boundaries. The day turns over at 08:00 UTC; the week on Monday at 08:00 UTC
/// (every <c>BigHuntWeeklyVersion</c> in the real save sits on that instant).
/// </summary>
public static class GameDayHelper
{
    public const int ResetHourUtc = 8;

    private static readonly TimeSpan ResetOffset = TimeSpan.FromHours(ResetHourUtc);

    /// <summary>Start of the game day containing <paramref name="nowMs"/>, as Unix millis.</summary>
    public static long StartOfDay(long nowMs)
    {
        DateTime shifted = DateTimeOffset.FromUnixTimeMilliseconds(nowMs).UtcDateTime - ResetOffset;
        return new DateTimeOffset(shifted.Date + ResetOffset, TimeSpan.Zero).ToUnixTimeMilliseconds();
    }

    /// <summary>Start of the game week (Monday reset) containing <paramref name="nowMs"/>, as Unix millis.</summary>
    public static long StartOfWeek(long nowMs)
    {
        DateTime dayStart = DateTimeOffset.FromUnixTimeMilliseconds(StartOfDay(nowMs)).UtcDateTime;
        int daysSinceMonday = ((int)dayStart.DayOfWeek + 6) % 7;
        return new DateTimeOffset(dayStart.AddDays(-daysSinceMonday), TimeSpan.Zero).ToUnixTimeMilliseconds();
    }

    /// <summary>True when <paramref name="thenMs"/> falls before the start of the game day containing <paramref name="nowMs"/>.</summary>
    public static bool IsBeforeToday(long thenMs, long nowMs) => thenMs < StartOfDay(nowMs);
}
