using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using Microsoft.EntityFrameworkCore;
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
}
