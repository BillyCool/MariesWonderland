using MariesWonderland.Constants;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using System.Text.Json;

namespace MariesWonderland.Data;

/// <summary>
/// Manages the map of userId → <see cref="DarkUserMemoryDatabase"/>. Handles user registration,
/// session management, and persistence to/from JSON files on disk.
/// </summary>
public class UserDataStore(DarkMasterMemoryDatabase masterDb)
{
    private readonly DarkMasterMemoryDatabase _masterDb = masterDb;
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
        var db = new DarkUserMemoryDatabase { UserId = userId };
        SeedInitialUserData(db);
        _users[userId] = db;
        return (userId, true);
    }

    /// <summary>
    /// Creates a new authenticated session for the user with the given TTL.
    /// </summary>
    public UserSession CreateSession(long userId, TimeSpan ttl)
    {
        var key = $"session_{userId}_{Guid.NewGuid():N}";
        var session = new UserSession(key, userId, DateTime.UtcNow.Add(ttl));
        _sessions[key] = session;
        return session;
    }

    /// <summary>
    /// Resolves a session key to a userId if the session exists and has not expired.
    /// </summary>
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

    /// <summary>
    /// Returns the user's in-memory database, creating an empty one if not found.
    /// </summary>
    public DarkUserMemoryDatabase GetOrCreate(long userId)
    {
        if (!_users.TryGetValue(userId, out var db))
        {
            db = new DarkUserMemoryDatabase { UserId = userId };
            _users[userId] = db;
        }
        return db;
    }

    /// <summary>
    /// Registers a pre-seeded database under a UUID. If the UUID is already mapped,
    /// returns the existing userId without overwriting. Otherwise stores the seeded
    /// database and maps the UUID to the userId found inside it.
    /// </summary>
    public long SeedUserFromDatabase(string uuid, DarkUserMemoryDatabase seededDb)
    {
        if (_uuidToUserId.TryGetValue(uuid, out var existingId))
            return existingId;

        long userId = seededDb.EntityIUser.FirstOrDefault()?.UserId ?? GenerateUserId();
        _uuidToUserId[uuid] = userId;
        _users[userId] = seededDb;
        return userId;
    }

    /// <summary>
    /// Attempts to retrieve a user's database without creating one.
    /// </summary>
    public bool TryGet(long userId, out DarkUserMemoryDatabase db)
        => _users.TryGetValue(userId, out db!);

    /// <summary>
    /// Finds the user database whose EntityIUser record matches the given playerId.
    /// Returns false if no user has that playerId.
    /// </summary>
    public bool TryGetByPlayerId(long playerId, out DarkUserMemoryDatabase db)
    {
        foreach (var (_, userDb) in _users)
        {
            if (userDb.EntityIUser.Any(u => u.PlayerId == playerId))
            {
                db = userDb;
                return true;
            }
        }
        db = null!;
        return false;
    }

    /// <summary>
    /// Stores a user database, replacing any existing one for that userId.
    /// </summary>
    public void Set(long userId, DarkUserMemoryDatabase db)
    {
        db.UserId = userId;
        _users[userId] = db;
    }

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
        {
            db.UserId = userId;
            _users[userId] = db;
        }

        foreach (var (uuid, userId) in snapshot.UuidToUserId)
            _uuidToUserId[uuid] = userId;

        foreach (UserSession session in snapshot.Sessions)
            _sessions[session.SessionKey] = session;

        return _users.Count;
    }

    /// <summary>
    /// Generates a random 19-digit user ID.
    /// </summary>
    private static long GenerateUserId()
    {
        // Random 19-digit positive long (range: 1e18 to 2e18)
        return Random.Shared.NextInt64(GameConstants.MinUserId, GameConstants.MaxUserId);
    }

    /// <summary>
    /// Generates a random 12-digit player ID.
    /// </summary>
    private static long GeneratePlayerId()
    {
        // Random 12-digit positive long (range: 1e12 to 2e12)
        return Random.Shared.NextInt64(GameConstants.MinPlayerId, GameConstants.MaxPlayerId);
    }

    /// <summary>
    /// Populates a new user database with default records (profile, status, starting weapons, etc.).
    /// </summary>
    private void SeedInitialUserData(DarkUserMemoryDatabase db)
    {
        var nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        db.EntityIUser.Add(new EntityIUser
        {
            UserId = db.UserId,
            PlayerId = GeneratePlayerId(),
            OsType = 2,
            PlatformType = PlatformType.GOOGLE_PLAY_STORE,
            UserRestrictionType = 0,
            RegisterDatetime = nowMs,
            GameStartDatetime = nowMs
        });

        db.EntityIUserSetting.Add(new EntityIUserSetting
        {
            UserId = db.UserId,
            IsNotifyPurchaseAlert = false
        });

        db.EntityIUserStatus.Add(new EntityIUserStatus
        {
            UserId = db.UserId,
            Level = 1,
            Exp = 0,
            StaminaMilliValue = 60000,
            StaminaUpdateDatetime = nowMs
        });

        db.EntityIUserProfile.Add(new EntityIUserProfile
        {
            UserId = db.UserId,
            Name = string.Empty,
            NameUpdateDatetime = nowMs,
            Message = string.Empty,
            MessageUpdateDatetime = nowMs,
            FavoriteCostumeId = 0,
            FavoriteCostumeIdUpdateDatetime = nowMs
        });

        db.EntityIUserLogin.Add(new EntityIUserLogin
        {
            UserId = db.UserId,
            TotalLoginCount = 1,
            ContinualLoginCount = 1,
            MaxContinualLoginCount = 1,
            LastLoginDatetime = nowMs,
            LastComebackLoginDatetime = 0
        });

        db.EntityIUserLoginBonus.Add(new EntityIUserLoginBonus
        {
            UserId = db.UserId,
            LoginBonusId = 1,
            CurrentPageNumber = 1,
            CurrentStampNumber = 0,
            LatestRewardReceiveDatetime = 0
        });

        db.EntityIUserTutorialProgress.Add(new EntityIUserTutorialProgress
        {
            UserId = db.UserId,
            TutorialType = TutorialType.GAME_START,
            ProgressPhase = 0,
            ChoiceId = 0
        });

        foreach (int weaponId in GameConstants.StartingWeaponIds)
        {
            string uuid = Guid.NewGuid().ToString();

            db.EntityIUserWeapon.Add(new EntityIUserWeapon
            {
                UserId = db.UserId,
                UserWeaponUuid = uuid,
                WeaponId = weaponId,
                Level = 1,
                Exp = 0,
                LimitBreakCount = 0,
                IsProtected = true,
                AcquisitionDatetime = nowMs
            });

            db.EntityIUserWeaponNote.Add(new EntityIUserWeaponNote
            {
                UserId = db.UserId,
                WeaponId = weaponId,
                MaxLevel = 1,
                MaxLimitBreakCount = 0,
                FirstAcquisitionDatetime = nowMs
            });

            db.EntityIUserWeaponStory.Add(new EntityIUserWeaponStory
            {
                UserId = db.UserId,
                WeaponId = weaponId,
                ReleasedMaxStoryIndex = 1
            });

            EntityMWeapon? masterWeapon = _masterDb.EntityMWeapon.FirstOrDefault(w => w.WeaponId == weaponId);
            if (masterWeapon != null)
            {
                foreach (EntityMWeaponAbilityGroup ag in _masterDb.EntityMWeaponAbilityGroup.Where(g => g.WeaponAbilityGroupId == masterWeapon.WeaponAbilityGroupId))
                    db.EntityIUserWeaponAbility.Add(new EntityIUserWeaponAbility { UserId = db.UserId, UserWeaponUuid = uuid, SlotNumber = ag.SlotNumber, Level = 1 });

                foreach (EntityMWeaponSkillGroup sg in _masterDb.EntityMWeaponSkillGroup.Where(g => g.WeaponSkillGroupId == masterWeapon.WeaponSkillGroupId))
                    db.EntityIUserWeaponSkill.Add(new EntityIUserWeaponSkill { UserId = db.UserId, UserWeaponUuid = uuid, SlotNumber = sg.SlotNumber, Level = 1 });
            }
        }
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

