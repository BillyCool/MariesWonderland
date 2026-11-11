using MariesWonderland.Models.Entities;

namespace MariesWonderland.MasterEditing;

/// <summary>A planned Arena season and its season-change maintenance block.</summary>
public sealed record PlannedSeason(EntityMPvpSeason Season, EntityMMaintenance Maintenance);

/// <summary>
/// Adds a run of Arena seasons after the last one, as <c>docs/dev-tools/scripts/arena-seasons</c> did: back to back,
/// each copying the groups of the last valid season, named 1, 2, … within each calendar year (the client has names
/// 001–040), each with an Arena downtime of 18 hours at its start (maintenance group 7 blocks every PvP call).
/// </summary>
public static class ArenaSeasonPlanner
{
    public const int MaintenanceGroup = 7;
    public const long DowntimeMs = 64_799_000L;
    private const long DayMs = 86_400_000L;

    public static IReadOnlyList<PlannedSeason> Plan(
        IReadOnlyList<EntityMPvpSeason> seasons, IReadOnlyList<EntityMMaintenance> maintenance, int count, int lengthDays, long? firstStartMs = null)
    {
        if (count is < 1 or > 500)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Plan 1 to 500 seasons.");
        }
        if (lengthDays is < 1 or > 365)
        {
            throw new ArgumentOutOfRangeException(nameof(lengthDays), "A season lasts 1 to 365 days.");
        }

        EntityMPvpSeason template = seasons.Where(s => !s.IsInvalid).MaxBy(s => s.SeasonStartDatetime)
            ?? throw new InvalidOperationException("There is no valid season to copy the groups from.");
        EntityMPvpSeason last = seasons.MaxBy(s => s.SeasonEndDatetime)!;

        long start = firstStartMs ?? last.SeasonEndDatetime + 1000;
        int seasonId = seasons.Max(s => s.PvpSeasonId) + 1;
        int maintenanceId = Math.Max(91001, maintenance.Count == 0 ? 0 : maintenance.Max(m => m.MaintenanceId) + 1);

        // Continue the per-year numbering from the seasons already in that year.
        Dictionary<int, int> yearCounts = seasons
            .GroupBy(s => DateTimeOffset.FromUnixTimeMilliseconds(s.SeasonStartDatetime).UtcDateTime.Year)
            .ToDictionary(g => g.Key, g => g.Select(s => int.TryParse(s.NameAssetPath, out int n) ? n : 0).DefaultIfEmpty(0).Max());

        List<PlannedSeason> planned = [];
        for (int i = 0; i < count; i++)
        {
            int year = DateTimeOffset.FromUnixTimeMilliseconds(start).UtcDateTime.Year;
            int number = yearCounts.GetValueOrDefault(year) + 1;
            yearCounts[year] = number;
            long end = start + lengthDays * DayMs - 1000;

            EntityMPvpSeason season = new()
            {
                PvpSeasonId = seasonId++,
                NameAssetPath = number.ToString(),
                SeasonStartDatetime = start,
                SeasonEndDatetime = end,
                PvpSeasonGroupingId = template.PvpSeasonGroupingId,
                IsInvalid = false,
                PvpWeeklyRankRewardRankGroupId = template.PvpWeeklyRankRewardRankGroupId,
                PvpSeasonRankRewardRankGroupId = template.PvpSeasonRankRewardRankGroupId,
                PvpGradeGroupId = template.PvpGradeGroupId,
                PvpInitialPointAdditionGroupId = template.PvpInitialPointAdditionGroupId,
                PvpSeasonDeckPowerThresholdGroupingId = template.PvpSeasonDeckPowerThresholdGroupingId,
            };
            EntityMMaintenance downtime = new()
            {
                MaintenanceId = maintenanceId++,
                StartDatetime = start,
                EndDatetime = start + DowntimeMs,
                MaintenanceGroupId = MaintenanceGroup,
            };
            planned.Add(new PlannedSeason(season, downtime));
            start = end + 1000;
        }
        return planned;
    }
}
