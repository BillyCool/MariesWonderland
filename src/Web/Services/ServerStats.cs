using MariesWonderland.Accounts;
using MariesWonderland.Configuration;
using MariesWonderland.Data;
using MariesWonderland.Helpers;
using MariesWonderland.Models.Entities;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace MariesWonderland.Web.Services;

public sealed record RunningContent(string Kind, string Name, DateTime Start, DateTime End);

public sealed record DashboardStats(
    int Accounts, int InMemory, int LiveSessions, int Active5, int Active15, int Active60, int Active24h,
    int NewToday, int PendingImports, RegistrationMode Mode,
    TimeSpan Uptime, long WorkingSetBytes, long ManagedBytes, int Gen2Collections, long DatabaseBytes,
    string MasterVersion, IReadOnlyList<(DateTime Minute, long Calls, long Failures)> Traffic,
    IReadOnlyList<RunningContent> Running);

/// <summary>Numbers for the admin dashboard.</summary>
public sealed class ServerStats(
    UserDataStore store,
    AccountStore accounts,
    ActivityTracker activity,
    SaveImportService imports,
    MasterDataHolder masterData,
    GameText text,
    TimeProvider timeProvider,
    IOptions<ServerOptions> options)
{
    public DashboardStats Collect()
    {
        DateTime now = timeProvider.GetUtcNow().UtcDateTime;
        Process process = Process.GetCurrentProcess();
        string dbPath = DataOptions.ResolvePath(options.Value.Data.SqliteDatabase);
        IReadOnlyDictionary<long, AccountRecord> records = accounts.All();

        return new DashboardStats(
            store.UserIds().Count,
            store.All.Count,
            store.LiveSessions().Count,
            activity.ActiveWithin(TimeSpan.FromMinutes(5)),
            activity.ActiveWithin(TimeSpan.FromMinutes(15)),
            activity.ActiveWithin(TimeSpan.FromHours(1)),
            activity.ActiveWithin(TimeSpan.FromDays(1)),
            records.Values.Count(r => r.CreatedAt >= now.Date),
            imports.PendingCount(),
            accounts.RegistrationMode,
            now - activity.StartedAt,
            process.WorkingSet64,
            GC.GetTotalMemory(false),
            GC.CollectionCount(2),
            File.Exists(dbPath) ? new FileInfo(dbPath).Length + (File.Exists(dbPath + "-wal") ? new FileInfo(dbPath + "-wal").Length : 0) : 0,
            masterData.Current.Version,
            activity.PerMinute(60),
            Running(now));
    }

    /// <summary>Seasons, events and shops open right now, from the master data dates.</summary>
    private List<RunningContent> Running(DateTime now)
    {
        DarkMasterMemoryDatabase db = masterData.Current.Database;
        long ms = new DateTimeOffset(now).ToUnixTimeMilliseconds();
        List<RunningContent> running = [];

        foreach (EntityMPvpSeason season in db.EntityMPvpSeason.Where(s => s.SeasonStartDatetime <= ms && s.SeasonEndDatetime > ms && !s.IsInvalid))
        {
            running.Add(new("Arena season", $"Season {season.PvpSeasonId}", Date(season.SeasonStartDatetime), Date(season.SeasonEndDatetime)));
        }
        foreach (EntityMBigHuntSchedule hunt in db.EntityMBigHuntSchedule.Where(s => s.ChallengeStartDatetime <= ms && s.ChallengeEndDatetime > ms))
        {
            running.Add(new("Subjugation", $"Schedule {hunt.BigHuntScheduleId}", Date(hunt.ChallengeStartDatetime), Date(hunt.ChallengeEndDatetime)));
        }
        foreach (EntityMEventQuestLabyrinthSeason season in db.EntityMEventQuestLabyrinthSeason.Where(s => s.StartDatetime <= ms && s.EndDatetime > ms))
        {
            EntityMEventQuestChapter? chapter = db.EntityMEventQuestChapter.Find(season.EventQuestChapterId);
            running.Add(new("Fate Board", $"{(chapter == null ? season.EventQuestChapterId.ToString() : text.EventChapter(chapter))}, season {season.SeasonNumber}", Date(season.StartDatetime), Date(season.EndDatetime)));
        }
        foreach (EntityMEventQuestChapter chapter in db.EntityMEventQuestChapter.Where(c => c.StartDatetime <= ms && c.EndDatetime > ms).OrderBy(c => c.EndDatetime).Take(40))
        {
            running.Add(new($"Event ({chapter.EventQuestType})", text.EventChapter(chapter), Date(chapter.StartDatetime), Date(chapter.EndDatetime)));
        }
        return running;
    }

    private static DateTime Date(long ms) => DateTimeOffset.FromUnixTimeMilliseconds(ms).UtcDateTime;
}
