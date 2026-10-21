namespace MariesWonderland.Helpers;

/// <summary>
/// The game's daily, weekly and monthly reset boundaries. The day turns over at 08:00 UTC; the week on Monday at
/// 08:00 UTC (every <c>BigHuntWeeklyVersion</c> in the real save sits on that instant); the month on the 1st at 08:00
/// UTC (00:00 PST, when chapter summons refilled).
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

    /// <summary>Start of the game month (the 1st's reset) containing <paramref name="nowMs"/>, as Unix millis.</summary>
    public static long StartOfMonth(long nowMs)
    {
        DateTime dayStart = DateTimeOffset.FromUnixTimeMilliseconds(StartOfDay(nowMs)).UtcDateTime;
        return new DateTimeOffset(dayStart.AddDays(1 - dayStart.Day), TimeSpan.Zero).ToUnixTimeMilliseconds();
    }

    /// <summary>Start of the game month after the one containing <paramref name="nowMs"/>, as Unix millis.</summary>
    public static long StartOfNextMonth(long nowMs)
        => new DateTimeOffset(DateTimeOffset.FromUnixTimeMilliseconds(StartOfMonth(nowMs)).UtcDateTime.AddMonths(1), TimeSpan.Zero).ToUnixTimeMilliseconds();

    /// <summary>True when <paramref name="thenMs"/> falls before the start of the game day containing <paramref name="nowMs"/>.</summary>
    public static bool IsBeforeToday(long thenMs, long nowMs) => thenMs < StartOfDay(nowMs);
}
