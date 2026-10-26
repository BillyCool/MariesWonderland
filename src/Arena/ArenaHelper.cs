using MariesWonderland.Data;
using MariesWonderland.Helpers;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Arena;

/// <summary>Arena pieces shared by the services: the season, grades, entering a season and the player's weekly rows.</summary>
public static class ArenaHelper
{
    /// <summary>Players are all in one bracket, the season grouping's group 1.</summary>
    public const int GroupId = 1;

    /// <summary>The season running at <paramref name="nowMs"/>; null between seasons.</summary>
    public static EntityMPvpSeason? CurrentSeason(DarkMasterMemoryDatabase masterDb, long nowMs)
        => masterDb.EntityMPvpSeason
            .Where(s => !s.IsInvalid && s.SeasonStartDatetime <= nowMs && nowMs < s.SeasonEndDatetime)
            .OrderBy(s => s.SeasonStartDatetime)
            .FirstOrDefault();

    public static ArenaSeason ToArenaSeason(EntityMPvpSeason season) => new(season.PvpSeasonId, season.SeasonStartDatetime, season.SeasonEndDatetime);

    /// <summary>The grade for <paramref name="points"/>: the last one whose required points it reaches.</summary>
    public static EntityMPvpGradeGroup? Grade(DarkMasterMemoryDatabase masterDb, int gradeGroupId, int points)
        => masterDb.EntityMPvpGradeGroup
            .Where(g => g.PvpGradeGroupId == gradeGroupId && g.NecessaryPvpPoint <= points)
            .OrderBy(g => g.NecessaryPvpPoint)
            .LastOrDefault();

    /// <summary>The player's highest deck force, which sets their starting points in a new season.</summary>
    public static int MaxDeckPower(DarkUserMemoryDatabase userDb)
        => Math.Max(
            userDb.EntityIUserDeckTypeNote.Select(n => n.MaxDeckPower).DefaultIfEmpty(0).Max(),
            userDb.EntityIUserDeck.Select(d => d.Power).DefaultIfEmpty(0).Max());

    /// <summary>
    /// The player's standing in <paramref name="season"/>, entering them if this is their first visit. Also refreshes
    /// what others see of their defense deck, ends a win streak left over from an earlier season and copies their
    /// weekly results into their own rows.
    /// </summary>
    public static ArenaStanding Enter(UserDataStore store, DarkMasterMemoryDatabase masterDb, DarkUserMemoryDatabase userDb, EntityMPvpSeason season, long nowMs)
    {
        ArenaSeason arenaSeason = ToArenaSeason(season);
        long playerId = userDb.EntityIUser.FirstOrDefault()?.PlayerId ?? 0;
        ArenaDefense defense = ArenaDecks.Summary(userDb, masterDb, DeckType.PVP, ArenaDecks.DefenseDeckNumber(userDb));
        ArenaStanding standing = store.Arena.Enter(
            arenaSeason, userDb.UserId, playerId, ArenaPoints.InitialPoints(MaxDeckPower(userDb)), defense, nowMs,
            () => ArenaBots.Create(masterDb, arenaSeason), out _);

        // The client adds the streak buff without checking dates, so a streak from an earlier season has to end here.
        EntityIUserPvpStatus status = userDb.EntityIUserPvpStatus.GetOrCreate(userDb.UserId);
        if (status.WinStreakCount != 0 && status.WinStreakCountUpdateDatetime < season.SeasonStartDatetime)
        {
            status.WinStreakCount = 0;
            status.WinStreakCountUpdateDatetime = nowMs;
        }

        SyncWeeks(store, userDb, nowMs);
        return standing;
    }

    /// <summary>Copies the player's weekly results (frozen ones and the week running) into their <c>EntityIUserPvpWeeklyResult</c> rows.</summary>
    public static void SyncWeeks(UserDataStore store, DarkUserMemoryDatabase userDb, long nowMs)
    {
        foreach (ArenaWeek week in store.Arena.Weeks(userDb.UserId, nowMs))
        {
            EntityIUserPvpWeeklyResult? row = userDb.EntityIUserPvpWeeklyResult.FirstOrDefault(r => r.PvpWeeklyVersion == week.WeekStart);
            if (row == null)
            {
                row = new EntityIUserPvpWeeklyResult { UserId = userDb.UserId, PvpWeeklyVersion = week.WeekStart };
                userDb.EntityIUserPvpWeeklyResult.Add(row);
            }
            else if (row.PvpSeasonId == week.SeasonId && row.FinalPoint == week.FinalPoint && row.FinalRank == week.FinalRank)
            {
                continue;
            }

            row.PvpSeasonId = week.SeasonId;
            row.GroupId = GroupId;
            row.FinalPoint = week.FinalPoint;
            row.FinalRank = week.FinalRank;
        }
    }
}
