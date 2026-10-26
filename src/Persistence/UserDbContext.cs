using MariesWonderland.Data;
using MariesWonderland.Friends;
using MariesWonderland.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Reflection;
using System.Text.Json;

namespace MariesWonderland.Persistence;

/// <summary>
/// The user database: one table per user table of <see cref="DarkUserMemoryDatabase"/> (named like the client's
/// tables, e.g. <c>IUserWeapon</c>, server-only ones like <c>SUserGift</c>), keyed by the entity's
/// <c>[PrimaryKey]</c>, plus the battle log and the device, session, shared-prize and friend tables. Values that aren't plain columns (lists,
/// dictionaries of a row) are stored as JSON. Provider-neutral; each provider derives its own context so it gets
/// its own migrations.
/// </summary>
public abstract class UserDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<DeviceAccount> Devices => Set<DeviceAccount>();
    public DbSet<StoredSession> Sessions => Set<StoredSession>();
    public DbSet<SharedPrizeClaim> SharedPrizeClaims => Set<SharedPrizeClaim>();
    public DbSet<FriendLink> FriendLinks => Set<FriendLink>();
    public DbSet<FriendRequest> FriendRequests => Set<FriendRequest>();
    public DbSet<FriendCheer> FriendCheers => Set<FriendCheer>();

    private static readonly MethodInfo ConfigureJsonMethod = typeof(UserDbContext).GetMethod(nameof(ConfigureJson), BindingFlags.NonPublic | BindingFlags.Static)!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        foreach (UserTable table in UserTables.All)
        {
            ConfigureRowTable(modelBuilder, table.RowType, table.Name);
        }

        // The battle log only ever grows and is never part of the in-memory user: rows are added as battles end.
        ConfigureRowTable(modelBuilder, typeof(EntitySBattleDetail), "SBattleDetail");

        modelBuilder.Entity<DeviceAccount>(entity =>
        {
            entity.ToTable("Devices");
            entity.HasKey(d => d.Uuid);
            entity.HasIndex(d => d.UserId);
        });

        modelBuilder.Entity<StoredSession>(entity =>
        {
            entity.ToTable("Sessions");
            entity.HasKey(s => s.SessionKey);
            entity.HasIndex(s => s.ExpiresAt);
        });

        modelBuilder.Entity<SharedPrizeClaim>(entity =>
        {
            entity.ToTable("SharedPrizeClaims");
            entity.HasKey(c => c.PrizeKey);
        });

        // Friends belong to two players, so they're server-wide tables rather than user tables.
        modelBuilder.Entity<FriendLink>(entity =>
        {
            entity.ToTable("FriendLinks");
            entity.HasKey(l => new { l.UserId, l.FriendUserId });
            entity.HasIndex(l => l.FriendUserId);
        });

        modelBuilder.Entity<FriendRequest>(entity =>
        {
            entity.ToTable("FriendRequests");
            entity.HasKey(r => new { r.FromUserId, r.ToUserId });
            entity.HasIndex(r => r.ToUserId);
        });

        modelBuilder.Entity<FriendCheer>(entity =>
        {
            entity.ToTable("FriendCheers");
            entity.HasKey(c => new { c.FromUserId, c.ToUserId });
            entity.HasIndex(c => c.ToUserId);
        });

        // Player-id lookups (friends, profiles) for users that aren't in memory.
        modelBuilder.Entity<EntityIUser>().HasIndex(u => u.PlayerId);
    }

    /// <summary>Maps a row type to its table, keyed by its <c>[PrimaryKey]</c>, with non-column values as JSON.</summary>
    private static void ConfigureRowTable(ModelBuilder modelBuilder, Type rowType, string name)
    {
        EntityTypeBuilder entity = modelBuilder.Entity(rowType);
        entity.ToTable(name);

        // Keys always come from the game data (user ids, uuids, master ids); the database never makes them up.
        foreach (PropertyInfo key in typeof(EntityKey<>).MakeGenericType(rowType)
            .GetProperty(nameof(EntityKey<object>.Properties))!.GetValue(null) as IReadOnlyList<PropertyInfo> ?? [])
        {
            entity.Property(key.Name).ValueGeneratedNever();
        }

        foreach (PropertyInfo prop in rowType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (prop.CanWrite && !IsColumnType(prop.PropertyType))
            {
                ConfigureJsonMethod.MakeGenericMethod(prop.PropertyType).Invoke(null, [entity, prop.Name]);
            }
        }
    }

    /// <summary>Whether a property type maps to a plain column; anything else is stored as JSON.</summary>
    private static bool IsColumnType(Type type)
    {
        Type t = Nullable.GetUnderlyingType(type) ?? type;
        return t.IsPrimitive || t.IsEnum || t == typeof(string) || t == typeof(decimal) || t == typeof(DateTime)
            || t == typeof(DateTimeOffset) || t == typeof(TimeSpan) || t == typeof(Guid) || t == typeof(byte[]);
    }

    private static void ConfigureJson<T>(EntityTypeBuilder entity, string propertyName)
    {
        ValueConverter<T, string> converter = new(
            v => JsonSerializer.Serialize(v, JsonOptions),
            v => JsonSerializer.Deserialize<T>(v, JsonOptions)!);
        ValueComparer<T> comparer = new(
            (a, b) => JsonSerializer.Serialize(a, JsonOptions) == JsonSerializer.Serialize(b, JsonOptions),
            v => JsonSerializer.Serialize(v, JsonOptions).GetHashCode(),
            v => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(v, JsonOptions), JsonOptions)!);

        entity.Property<T>(propertyName).HasConversion(converter, comparer);
    }

    private static readonly JsonSerializerOptions JsonOptions = new();
}

/// <summary>A device (install) id and the user it signs in as.</summary>
public sealed class DeviceAccount
{
    public string Uuid { get; set; } = "";
    public long UserId { get; set; }
}

/// <summary>A login session issued by <c>Auth</c>.</summary>
public sealed class StoredSession
{
    public string SessionKey { get; set; } = "";
    public long UserId { get; set; }
    public DateTime ExpiresAt { get; set; }
}

/// <summary>How many of a shared limited summon prize every player on the server has won ("gachaId:prizeIndex").</summary>
public sealed class SharedPrizeClaim
{
    public string PrizeKey { get; set; } = "";
    public int Claimed { get; set; }
}
