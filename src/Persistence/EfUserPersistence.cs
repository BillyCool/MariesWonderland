using MariesWonderland.Arena;
using MariesWonderland.Data;
using MariesWonderland.Friends;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Collections;
using System.Reflection;

namespace MariesWonderland.Persistence;

/// <summary>
/// <see cref="IUserPersistence"/> on an EF Core <see cref="UserDbContext"/>. Every call uses a fresh context, so
/// calls for different users can run in parallel; the store only calls it for a user while holding that user's lock.
/// </summary>
public sealed class EfUserPersistence(Func<UserDbContext> createContext) : IUserPersistence
{
    /// <summary>Per-table operations, built once per row type.</summary>
    private sealed record TableOps(
        UserTable Table,
        Func<UserDbContext, long, IList> Load,
        Action<UserDbContext, long> DeleteAll);

    private static readonly IReadOnlyList<TableOps> Tables = [.. UserTables.All.Select(table =>
        (TableOps)typeof(EfUserPersistence).GetMethod(nameof(CreateOps), BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(table.RowType).Invoke(null, [table])!)];

    private static TableOps CreateOps<T>(UserTable table) where T : class, IUserEntity
        => new(
            table,
            (db, userId) => db.Set<T>().AsNoTracking().Where(r => r.UserId == userId).ToList(),
            (db, userId) => db.Set<T>().Where(r => r.UserId == userId).ExecuteDelete());

    /// <summary>Creates the database or brings its schema up to date.</summary>
    public void Migrate()
    {
        using UserDbContext db = createContext();
        db.Database.Migrate();
    }

    public DarkUserMemoryDatabase? LoadUser(long userId)
    {
        using UserDbContext db = createContext();
        DarkUserMemoryDatabase userDb = new() { UserId = userId };
        bool any = false;
        foreach (TableOps ops in Tables)
        {
            IList rows = ops.Load(db, userId);
            any |= rows.Count > 0;
            ops.Table.Property.SetValue(userDb, rows);
        }
        return any ? userDb : null;
    }

    public void SaveChanges(long userId, UserChangeSet changes)
    {
        if (changes.IsEmpty)
        {
            return;
        }

        using UserDbContext db = createContext();
        foreach (TableChange change in changes.Tables)
        {
            foreach (object row in change.Deleted)
            {
                db.Remove(row);
            }
            foreach (object row in change.Modified)
            {
                db.Update(row);
            }
            foreach (object row in change.Added)
            {
                db.Add(row);
            }
        }
        db.SaveChanges();
    }

    public void ReplaceUser(DarkUserMemoryDatabase userDb)
    {
        using UserDbContext db = createContext();
        using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction = db.Database.BeginTransaction();
        foreach (TableOps ops in Tables)
        {
            ops.DeleteAll(db, userDb.UserId);
        }

        // The whole user as one change set against nothing: every row (first of a repeated key) is added.
        UserChangeSet all = UserDataDiffBuilder.Changes([], UserDataDiffBuilder.Snapshot(userDb));
        foreach (TableChange change in all.Tables)
        {
            foreach (object row in change.Added)
            {
                db.Add(row);
            }
        }
        db.SaveChanges();
        transaction.Commit();
    }

    public void AddBattleDetail(EntitySBattleDetail detail)
    {
        using UserDbContext db = createContext();
        db.Add(detail);
        db.SaveChanges();
    }

    public IReadOnlyList<EntitySBattleDetail> BattleDetails(long userId)
    {
        using UserDbContext db = createContext();
        return [.. db.Set<EntitySBattleDetail>().AsNoTracking().Where(d => d.UserId == userId).OrderBy(d => d.RecordedAt)];
    }

    public IReadOnlyList<EntitySBattleDetail> BattleDetails(long userId, int questId, DateTime sinceUtc)
    {
        using UserDbContext db = createContext();
        return [.. db.Set<EntitySBattleDetail>().AsNoTracking().Where(d => d.UserId == userId && d.QuestId == questId && d.RecordedAt >= sinceUtc).OrderBy(d => d.RecordedAt)];
    }

    public IReadOnlyList<long> UserIds()
    {
        using UserDbContext db = createContext();
        return [.. db.Set<EntityIUser>().AsNoTracking().Select(u => u.UserId)];
    }

    public long? FindUserIdByPlayerId(long playerId)
    {
        using UserDbContext db = createContext();
        return db.Set<EntityIUser>().AsNoTracking().Where(u => u.PlayerId == playerId).Select(u => (long?)u.UserId).FirstOrDefault();
    }

    public long? FindUserIdByDevice(string uuid)
    {
        using UserDbContext db = createContext();
        return db.Devices.AsNoTracking().Where(d => d.Uuid == uuid).Select(d => (long?)d.UserId).FirstOrDefault();
    }

    public void SaveDevice(string uuid, long userId)
    {
        using UserDbContext db = createContext();
        DeviceAccount? device = db.Devices.AsTracking().FirstOrDefault(d => d.Uuid == uuid);
        if (device == null)
        {
            db.Devices.Add(new DeviceAccount { Uuid = uuid, UserId = userId });
        }
        else
        {
            device.UserId = userId;
        }
        db.SaveChanges();
    }

    public IReadOnlyDictionary<string, long> Devices()
    {
        using UserDbContext db = createContext();
        return db.Devices.AsNoTracking().ToDictionary(d => d.Uuid, d => d.UserId);
    }

    public IReadOnlyList<string> DevicesOf(long userId)
    {
        using UserDbContext db = createContext();
        return [.. db.Devices.AsNoTracking().Where(d => d.UserId == userId).Select(d => d.Uuid)];
    }

    public int RemoveDevicesOf(long userId)
    {
        using UserDbContext db = createContext();
        return db.Devices.Where(d => d.UserId == userId).ExecuteDelete();
    }

    public IReadOnlyList<UserSession> SessionsOf(long userId)
    {
        using UserDbContext db = createContext();
        return [.. db.Sessions.AsNoTracking().Where(s => s.UserId == userId).AsEnumerable()
            .Select(s => new UserSession(s.SessionKey, s.UserId, DateTime.SpecifyKind(s.ExpiresAt, DateTimeKind.Utc)))];
    }

    public int RemoveSessionsOf(long userId)
    {
        using UserDbContext db = createContext();
        return db.Sessions.Where(s => s.UserId == userId).ExecuteDelete();
    }

    public UserSession? FindSession(string sessionKey)
    {
        using UserDbContext db = createContext();
        StoredSession? session = db.Sessions.AsNoTracking().FirstOrDefault(s => s.SessionKey == sessionKey);
        return session == null ? null : new UserSession(session.SessionKey, session.UserId, DateTime.SpecifyKind(session.ExpiresAt, DateTimeKind.Utc));
    }

    public void SaveSession(UserSession session, DateTime now)
    {
        using UserDbContext db = createContext();
        db.Sessions.Where(s => s.ExpiresAt <= now).ExecuteDelete();
        db.Sessions.Add(new StoredSession { SessionKey = session.SessionKey, UserId = session.UserId, ExpiresAt = session.ExpiresAt });
        db.SaveChanges();
    }

    public IReadOnlyList<UserSession> Sessions()
    {
        using UserDbContext db = createContext();
        return [.. db.Sessions.AsNoTracking().AsEnumerable()
            .Select(s => new UserSession(s.SessionKey, s.UserId, DateTime.SpecifyKind(s.ExpiresAt, DateTimeKind.Utc)))];
    }

    public IReadOnlyDictionary<string, int> SharedPrizeClaims()
    {
        using UserDbContext db = createContext();
        return db.SharedPrizeClaims.AsNoTracking().ToDictionary(c => c.PrizeKey, c => c.Claimed);
    }

    public void SaveSharedPrizeClaim(string prizeKey, int claimed)
    {
        using UserDbContext db = createContext();
        SharedPrizeClaim? claim = db.SharedPrizeClaims.AsTracking().FirstOrDefault(c => c.PrizeKey == prizeKey);
        if (claim == null)
        {
            db.SharedPrizeClaims.Add(new SharedPrizeClaim { PrizeKey = prizeKey, Claimed = claimed });
        }
        else
        {
            claim.Claimed = claimed;
        }
        db.SaveChanges();
    }

    public FriendState LoadFriends()
    {
        using UserDbContext db = createContext();
        return new FriendState([.. db.FriendLinks.AsNoTracking()], [.. db.FriendRequests.AsNoTracking()], [.. db.FriendCheers.AsNoTracking()]);
    }

    public void SaveFriendChanges(FriendChanges changes)
    {
        if (changes.IsEmpty)
        {
            return;
        }

        using UserDbContext db = createContext();
        using IDbContextTransaction transaction = db.Database.BeginTransaction();

        foreach (FriendLink link in changes.RemovedLinks)
        {
            db.FriendLinks.Where(l => l.UserId == link.UserId && l.FriendUserId == link.FriendUserId).ExecuteDelete();
        }
        foreach (FriendRequest request in changes.RemovedRequests)
        {
            db.FriendRequests.Where(r => r.FromUserId == request.FromUserId && r.ToUserId == request.ToUserId).ExecuteDelete();
        }
        foreach (FriendCheer cheer in changes.RemovedCheers)
        {
            db.FriendCheers.Where(c => c.FromUserId == cheer.FromUserId && c.ToUserId == cheer.ToUserId).ExecuteDelete();
        }

        // A row can be saved twice in one operation (say a request resolved by both sides); the last copy wins.
        foreach (FriendLink link in changes.SavedLinks.GroupBy(l => (l.UserId, l.FriendUserId)).Select(g => g.Last()))
        {
            FriendLink? row = db.FriendLinks.AsTracking().FirstOrDefault(l => l.UserId == link.UserId && l.FriendUserId == link.FriendUserId);
            if (row == null)
            {
                db.FriendLinks.Add(new FriendLink { UserId = link.UserId, FriendUserId = link.FriendUserId, CreatedAt = link.CreatedAt });
            }
            else
            {
                row.CreatedAt = link.CreatedAt;
            }
        }
        foreach (FriendRequest request in changes.SavedRequests.GroupBy(r => (r.FromUserId, r.ToUserId)).Select(g => g.Last()))
        {
            FriendRequest? row = db.FriendRequests.AsTracking().FirstOrDefault(r => r.FromUserId == request.FromUserId && r.ToUserId == request.ToUserId);
            if (row == null)
            {
                db.FriendRequests.Add(new FriendRequest { FromUserId = request.FromUserId, ToUserId = request.ToUserId, SentAt = request.SentAt, Resolved = request.Resolved });
            }
            else
            {
                row.SentAt = request.SentAt;
                row.Resolved = request.Resolved;
            }
        }
        foreach (FriendCheer cheer in changes.SavedCheers.GroupBy(c => (c.FromUserId, c.ToUserId)).Select(g => g.Last()))
        {
            FriendCheer? row = db.FriendCheers.AsTracking().FirstOrDefault(c => c.FromUserId == cheer.FromUserId && c.ToUserId == cheer.ToUserId);
            if (row == null)
            {
                db.FriendCheers.Add(new FriendCheer { FromUserId = cheer.FromUserId, ToUserId = cheer.ToUserId, SentAt = cheer.SentAt, ReceivedAt = cheer.ReceivedAt });
            }
            else
            {
                row.SentAt = cheer.SentAt;
                row.ReceivedAt = cheer.ReceivedAt;
            }
        }

        db.SaveChanges();
        transaction.Commit();
    }

    public ArenaState LoadArena()
    {
        using UserDbContext db = createContext();
        return new ArenaState([.. db.ArenaEntries.AsNoTracking()], [.. db.ArenaBattles.AsNoTracking()], [.. db.ArenaWeeks.AsNoTracking()]);
    }

    public void SaveArenaChanges(ArenaChanges changes)
    {
        if (changes.IsEmpty)
        {
            return;
        }

        using UserDbContext db = createContext();
        using IDbContextTransaction transaction = db.Database.BeginTransaction();

        foreach (int seasonId in changes.ClearedSeasons)
        {
            db.ArenaEntries.Where(e => e.SeasonId == seasonId && e.IsBot).ExecuteDelete();
            db.ArenaBattles.Where(b => b.SeasonId == seasonId).ExecuteDelete();
        }
        if (changes.RemovedBattleIds.Count > 0)
        {
            long[] removed = [.. changes.RemovedBattleIds];
            db.ArenaBattles.Where(b => removed.Contains(b.BattleId)).ExecuteDelete();
        }

        // A row can be saved twice in one operation; the last copy wins. Existing rows are read in one query per season
        // (a new season saves all its bots at once).
        foreach (IGrouping<int, ArenaEntry> season in changes.SavedEntries
            .GroupBy(e => (e.SeasonId, e.UserId)).Select(g => g.Last())
            .GroupBy(e => e.SeasonId))
        {
            long[] userIds = [.. season.Select(e => e.UserId)];
            Dictionary<long, ArenaEntry> existing = db.ArenaEntries.AsTracking()
                .Where(e => e.SeasonId == season.Key && userIds.Contains(e.UserId))
                .ToDictionary(e => e.UserId);
            foreach (ArenaEntry entry in season)
            {
                CopyInto(db, existing.GetValueOrDefault(entry.UserId), entry);
            }
        }

        long[] battleIds = [.. changes.SavedBattles.Select(b => b.BattleId).Distinct()];
        Dictionary<long, ArenaBattle> battles = db.ArenaBattles.AsTracking().Where(b => battleIds.Contains(b.BattleId)).ToDictionary(b => b.BattleId);
        foreach (ArenaBattle battle in changes.SavedBattles.GroupBy(b => b.BattleId).Select(g => g.Last()))
        {
            CopyInto(db, battles.GetValueOrDefault(battle.BattleId), battle);
        }

        foreach (ArenaWeek week in changes.SavedWeeks.GroupBy(w => (w.UserId, w.WeekStart)).Select(g => g.Last()))
        {
            CopyInto(db, db.ArenaWeeks.AsTracking().FirstOrDefault(w => w.UserId == week.UserId && w.WeekStart == week.WeekStart), week);
        }

        db.SaveChanges();
        transaction.Commit();
    }

    /// <summary>Writes an in-memory row's values into its saved row, or adds a copy of it (the store keeps its own objects).</summary>
    private static void CopyInto<T>(UserDbContext db, T? row, T values) where T : class, new()
    {
        if (row != null)
        {
            db.Entry(row).CurrentValues.SetValues(values);
            return;
        }

        // Keys are set before the row is tracked: a tracked row's key can't change.
        T added = new();
        db.Entry(added).CurrentValues.SetValues(values);
        db.Add(added);
    }

    public IReadOnlyList<PlayerCard> PlayerCards(IReadOnlyCollection<long>? userIds)
    {
        using UserDbContext db = createContext();
        long[]? ids = userIds?.ToArray();
        Dictionary<long, EntityIUserProfile> profiles = OfUsers(db.Set<EntityIUserProfile>(), ids).ToDictionary(p => p.UserId);
        Dictionary<long, int> levels = OfUsers(db.Set<EntityIUserStatus>(), ids).ToDictionary(s => s.UserId, s => s.Level);
        Dictionary<long, long> logins = OfUsers(db.Set<EntityIUserLogin>(), ids).ToDictionary(l => l.UserId, l => l.LastLoginDatetime);
        Dictionary<long, int> powers = OfUsers(db.Set<EntityIUserDeckTypeNote>(), ids).Where(n => n.DeckType == DeckType.QUEST)
            .ToDictionary(n => n.UserId, n => n.MaxDeckPower);

        return [.. OfUsers(db.Set<EntityIUser>(), ids).AsEnumerable().Select(u => new PlayerCard(
            u.UserId,
            u.PlayerId,
            profiles.GetValueOrDefault(u.UserId)?.Name ?? string.Empty,
            levels.GetValueOrDefault(u.UserId),
            profiles.GetValueOrDefault(u.UserId)?.FavoriteCostumeId ?? 0,
            powers.GetValueOrDefault(u.UserId),
            logins.GetValueOrDefault(u.UserId)))];
    }

    /// <summary>The rows of these users, or of everyone when <paramref name="userIds"/> is null.</summary>
    private static IQueryable<T> OfUsers<T>(IQueryable<T> rows, long[]? userIds) where T : class, IUserEntity
        => userIds == null ? rows.AsNoTracking() : rows.AsNoTracking().Where(r => userIds.Contains(r.UserId));
}
