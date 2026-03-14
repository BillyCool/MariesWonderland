using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using System.Text.Json;

namespace MariesWonderland.Data;

public class UserDataStore
{
    private readonly Dictionary<long, DarkUserMemoryDatabase> _users = [];
    private readonly Dictionary<string, long> _uuidToUserId = [];
    private readonly Dictionary<string, UserSession> _sessions = [];

    /// <summary>
    /// Look up or create a user by UUID. Returns the userId and whether the user is new.
    /// For new users, seeds the initial data into their database.
    /// </summary>
    public (long UserId, bool IsNew) RegisterOrGetUser(string uuid)
    {
        if (_uuidToUserId.TryGetValue(uuid, out var existingId))
            return (existingId, false);

        var userId = GenerateUserId();
        _uuidToUserId[uuid] = userId;
        var db = new DarkUserMemoryDatabase();
        SeedInitialUserData(db, userId);
        _users[userId] = db;
        return (userId, true);
    }

    public UserSession CreateSession(long userId, TimeSpan ttl)
    {
        var key = $"session_{userId}_{Guid.NewGuid():N}";
        var session = new UserSession(key, userId, DateTime.UtcNow.Add(ttl));
        _sessions[key] = session;
        return session;
    }

    public bool TryResolveSession(string sessionKey, out long userId)
    {
        if (_sessions.TryGetValue(sessionKey, out var session) && session.ExpiresAt > DateTime.UtcNow)
        {
            userId = session.UserId;
            return true;
        }
        userId = 0;
        return false;
    }

    public DarkUserMemoryDatabase GetOrCreate(long userId)
    {
        if (!_users.TryGetValue(userId, out var db))
        {
            db = new DarkUserMemoryDatabase();
            _users[userId] = db;
        }
        return db;
    }

    public bool TryGet(long userId, out DarkUserMemoryDatabase db)
        => _users.TryGetValue(userId, out db!);

    public void Set(long userId, DarkUserMemoryDatabase db)
        => _users[userId] = db;

    public IReadOnlyDictionary<long, DarkUserMemoryDatabase> All => _users;

    /// <summary>
    /// Serialize all user data to a JSON file on disk.
    /// </summary>
    public void Save(string filePath)
    {
        UserDataSnapshot snapshot = new()
        {
            Users = _users,
            UuidToUserId = _uuidToUserId,
            Sessions = _sessions.Values.ToList()
        };

        JsonSerializerOptions options = new()
        {
            WriteIndented = true
        };

        string json = JsonSerializer.Serialize(snapshot, options);
        File.WriteAllText(filePath, json);
    }

    /// <summary>
    /// Deserialize user data from a JSON file on disk, replacing the current in-memory state.
    /// Returns the number of users loaded.
    /// </summary>
    public int Load(string filePath)
    {
        if (!File.Exists(filePath))
            return 0;

        string json = File.ReadAllText(filePath);
        UserDataSnapshot? snapshot = JsonSerializer.Deserialize<UserDataSnapshot>(json);

        if (snapshot is null)
            return 0;

        _users.Clear();
        _uuidToUserId.Clear();
        _sessions.Clear();

        foreach (var (userId, db) in snapshot.Users)
            _users[userId] = db;

        foreach (var (uuid, userId) in snapshot.UuidToUserId)
            _uuidToUserId[uuid] = userId;

        foreach (UserSession session in snapshot.Sessions)
            _sessions[session.SessionKey] = session;

        return _users.Count;
    }

    private static long GenerateUserId()
    {
        // Random 19-digit positive long (range: 1e18 to long.MaxValue)
        return Random.Shared.NextInt64(1_000_000_000_000_000_000L, long.MaxValue);
    }

    private static void SeedInitialUserData(DarkUserMemoryDatabase db, long userId)
    {
        var nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        db.EntityIUser.Add(new EntityIUser
        {
            UserId = userId,
            PlayerId = userId,
            OsType = 2,
            PlatformType = PlatformType.GOOGLE_PLAY_STORE,
            UserRestrictionType = 0,
            RegisterDatetime = nowMs,
            GameStartDatetime = nowMs,
            LatestVersion = 0
        });

        db.EntityIUserSetting.Add(new EntityIUserSetting
        {
            UserId = userId,
            IsNotifyPurchaseAlert = false,
            LatestVersion = 0
        });

        db.EntityIUserStatus.Add(new EntityIUserStatus
        {
            UserId = userId,
            Level = 1,
            Exp = 0,
            StaminaMilliValue = 60000,
            StaminaUpdateDatetime = nowMs,
            LatestVersion = 0
        });

        db.EntityIUserProfile.Add(new EntityIUserProfile
        {
            UserId = userId,
            Name = string.Empty,
            NameUpdateDatetime = nowMs,
            Message = string.Empty,
            MessageUpdateDatetime = nowMs,
            FavoriteCostumeId = 0,
            FavoriteCostumeIdUpdateDatetime = nowMs,
            LatestVersion = 0
        });

        db.EntityIUserLogin.Add(new EntityIUserLogin
        {
            UserId = userId,
            TotalLoginCount = 1,
            ContinualLoginCount = 1,
            MaxContinualLoginCount = 1,
            LastLoginDatetime = nowMs,
            LastComebackLoginDatetime = 0,
            LatestVersion = 0
        });

        db.EntityIUserLoginBonus.Add(new EntityIUserLoginBonus
        {
            UserId = userId,
            LoginBonusId = 1,
            CurrentPageNumber = 1,
            CurrentStampNumber = 0,
            LatestRewardReceiveDatetime = 0,
            LatestVersion = 0
        });
    }
}

/// <summary>
/// Serializable snapshot of all user data for persistence.
/// </summary>
file record UserDataSnapshot
{
    public Dictionary<long, DarkUserMemoryDatabase> Users { get; init; } = [];
    public Dictionary<string, long> UuidToUserId { get; init; } = [];
    public List<UserSession> Sessions { get; init; } = [];
}

