using MariesWonderland.Constants;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using System.Collections.Concurrent;
using System.Text.Json;

namespace MariesWonderland.Data;

/// <summary>
/// Manages the map of userId → <see cref="DarkUserMemoryDatabase"/>. Handles user registration,
/// session management, and persistence to/from JSON files on disk.
/// The maps are safe to use from parallel requests. A user's own database isn't: requests reach it under that
/// user's lock (<see cref="Locks"/>).
/// </summary>
public class UserDataStore(TimeProvider timeProvider)
{
    /// <summary>How long code waits for another user's lock (or a user's lock outside a request) before giving up.</summary>
    public static readonly TimeSpan LockTimeout = TimeSpan.FromSeconds(10);

    private readonly ConcurrentDictionary<long, DarkUserMemoryDatabase> _users = new();
    private readonly ConcurrentDictionary<string, long> _uuidToUserId = new();
    private readonly ConcurrentDictionary<string, UserSession> _sessions = new();
    private readonly Dictionary<string, int> _sharedPrizeClaims = [];
    private readonly Lock _sharedPrizeLock = new();
    private readonly Lock _registrationLock = new();
    private readonly TimeProvider _timeProvider = timeProvider;

    /// <summary>The per-user locks requests run under.</summary>
    public UserLocks Locks { get; } = new();

    /// <summary>How many of a shared limited prize (e.g. Mama's Dream gems) every player on the server has won so far.</summary>
    public int SharedPrizeClaimed(int gachaId, int prizeIndex)
    {
        lock (_sharedPrizeLock)
        {
            return _sharedPrizeClaims.GetValueOrDefault(SharedPrizeKey(gachaId, prizeIndex));
        }
    }

    /// <summary>Claims one of a shared limited prize; false when its <paramref name="stock"/> is already used up.</summary>
    public bool TryClaimSharedPrize(int gachaId, int prizeIndex, int stock)
    {
        lock (_sharedPrizeLock)
        {
            string key = SharedPrizeKey(gachaId, prizeIndex);
            int claimed = _sharedPrizeClaims.GetValueOrDefault(key);
            if (claimed >= stock)
            {
                return false;
            }

            _sharedPrizeClaims[key] = claimed + 1;
            return true;
        }
    }

    private static string SharedPrizeKey(int gachaId, int prizeIndex) => $"{gachaId}:{prizeIndex}";

    /// <summary>
    /// Look up or create a user by UUID. Returns the userId and whether the user is new.
    /// For new users, seeds the initial data into their database.
    /// </summary>
    public (long UserId, bool IsNew) RegisterOrGetUser(string uuid, DarkMasterMemoryDatabase masterDb)
    {
        if (_uuidToUserId.TryGetValue(uuid, out long existingId))
            return (existingId, false);

        lock (_registrationLock)
        {
            if (_uuidToUserId.TryGetValue(uuid, out existingId))
                return (existingId, false);

            long userId = NewUserId();
            DarkUserMemoryDatabase db = new() { UserId = userId };
            SeedInitialUserData(db, masterDb);
            _users[userId] = db;
            _uuidToUserId[uuid] = userId;
            return (userId, true);
        }
    }

    /// <summary>
    /// Creates a new authenticated session for the user with the given TTL.
    /// </summary>
    public UserSession CreateSession(long userId, TimeSpan ttl)
    {
        DateTime now = _timeProvider.GetUtcNow().UtcDateTime;
        foreach ((string expiredKey, UserSession expired) in _sessions)
        {
            if (expired.ExpiresAt <= now)
                _sessions.TryRemove(expiredKey, out _);
        }

        string key = $"session_{userId}_{Guid.NewGuid():N}";
        UserSession session = new(key, userId, now.Add(ttl));
        _sessions[key] = session;
        return session;
    }

    /// <summary>
    /// Resolves a session key to a userId if the session exists and has not expired.
    /// </summary>
    public bool TryResolveSession(string sessionKey, out long userId)
    {
        if (_sessions.TryGetValue(sessionKey, out var session) && session.ExpiresAt > _timeProvider.GetUtcNow().UtcDateTime)
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
        => _users.GetOrAdd(userId, static id => new DarkUserMemoryDatabase { UserId = id });

    /// <summary>
    /// Registers a pre-seeded database under a UUID. If the UUID is already mapped,
    /// returns the existing userId without overwriting. Otherwise stores the seeded
    /// database and maps the UUID to the userId found inside it.
    /// </summary>
    public long SeedUserFromDatabase(string uuid, DarkUserMemoryDatabase seededDb)
    {
        lock (_registrationLock)
        {
            if (_uuidToUserId.TryGetValue(uuid, out long existingId))
                return existingId;

            long userId = seededDb.EntityIUser.FirstOrDefault()?.UserId ?? NewUserId();
            _users[userId] = seededDb;
            _uuidToUserId[uuid] = userId;
            return userId;
        }
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
        // Each user is written while holding their lock, so no request is changing them mid-write.
        List<IDisposable> held = [];
        try
        {
            foreach (long userId in _users.Keys.Order())
            {
                held.Add(Locks.TryAcquire(userId, LockTimeout)
                    ?? throw new TimeoutException($"User {userId} stayed busy; nothing was saved."));
            }

            UserDataSnapshot snapshot;
            lock (_sharedPrizeLock)
            {
                snapshot = new()
                {
                    Users = new Dictionary<long, DarkUserMemoryDatabase>(_users),
                    UuidToUserId = new Dictionary<string, long>(_uuidToUserId),
                    Sessions = [.. _sessions.Values],
                    SharedPrizeClaims = new Dictionary<string, int>(_sharedPrizeClaims)
                };
            }

            JsonSerializerOptions options = new()
            {
                WriteIndented = true
            };

            string json = JsonSerializer.Serialize(snapshot, options);
            File.WriteAllText(filePath, json);
        }
        finally
        {
            foreach (IDisposable handle in held)
            {
                handle.Dispose();
            }
        }
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

        lock (_sharedPrizeLock)
        {
            _sharedPrizeClaims.Clear();
            foreach (var (key, claimed) in snapshot.SharedPrizeClaims)
                _sharedPrizeClaims[key] = claimed;
        }

        return _users.Count;
    }

    /// <summary>A random user ID no other user has.</summary>
    private long NewUserId()
    {
        long userId;
        do
        {
            userId = GenerateUserId();
        }
        while (_users.ContainsKey(userId));
        return userId;
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
    private void SeedInitialUserData(DarkUserMemoryDatabase db, DarkMasterMemoryDatabase masterDb)
    {
        var nowMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();

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

        // One-row-per-user tables present on every real account, seeded with default values
        db.EntityIUserGem.Add(new EntityIUserGem { UserId = db.UserId });
        db.EntityIUserExplore.Add(new EntityIUserExplore { UserId = db.UserId });
        db.EntityIUserMainQuestFlowStatus.Add(new EntityIUserMainQuestFlowStatus { UserId = db.UserId });
        db.EntityIUserMainQuestMainFlowStatus.Add(new EntityIUserMainQuestMainFlowStatus { UserId = db.UserId });
        db.EntityIUserMainQuestProgressStatus.Add(new EntityIUserMainQuestProgressStatus { UserId = db.UserId });
        db.EntityIUserMainQuestReplayFlowStatus.Add(new EntityIUserMainQuestReplayFlowStatus { UserId = db.UserId });
        db.EntityIUserEventQuestProgressStatus.Add(new EntityIUserEventQuestProgressStatus { UserId = db.UserId });
        db.EntityIUserExtraQuestProgressStatus.Add(new EntityIUserExtraQuestProgressStatus { UserId = db.UserId });
        db.EntityIUserSideStoryQuestSceneProgressStatus.Add(new EntityIUserSideStoryQuestSceneProgressStatus { UserId = db.UserId });
        db.EntityIUserPortalCageStatus.Add(new EntityIUserPortalCageStatus { UserId = db.UserId });
        db.EntityIUserBigHuntProgressStatus.Add(new EntityIUserBigHuntProgressStatus { UserId = db.UserId });
        db.EntityIUserEventQuestGuerrillaFreeOpen.Add(new EntityIUserEventQuestGuerrillaFreeOpen { UserId = db.UserId });
        db.EntityIUserEventQuestDailyGroupCompleteReward.Add(new EntityIUserEventQuestDailyGroupCompleteReward { UserId = db.UserId });
        db.EntityIUserShopReplaceable.Add(new EntityIUserShopReplaceable { UserId = db.UserId });
        db.EntityIUserPvpStatus.Add(new EntityIUserPvpStatus { UserId = db.UserId, StaminaUpdateDatetime = nowMs });
        db.EntityIUserPvpDefenseDeck.Add(new EntityIUserPvpDefenseDeck { UserId = db.UserId });
        db.EntityIUserQuestAutoOrbit.Add(new EntityIUserQuestAutoOrbit { UserId = db.UserId });

        // TODO: Seed the first mission record (missionId=1, IN_PROGRESS) once missions are implemented.

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

            EntityMWeapon? masterWeapon = masterDb.EntityMWeapon.FirstOrDefault(w => w.WeaponId == weaponId);
            if (masterWeapon != null)
            {
                foreach (EntityMWeaponAbilityGroup ag in masterDb.EntityMWeaponAbilityGroup.Where(g => g.WeaponAbilityGroupId == masterWeapon.WeaponAbilityGroupId))
                    db.EntityIUserWeaponAbility.Add(new EntityIUserWeaponAbility { UserId = db.UserId, UserWeaponUuid = uuid, SlotNumber = ag.SlotNumber, Level = 1 });

                foreach (EntityMWeaponSkillGroup sg in masterDb.EntityMWeaponSkillGroup.Where(g => g.WeaponSkillGroupId == masterWeapon.WeaponSkillGroupId))
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

    /// <summary>Server-wide claims of shared limited summon prizes, keyed "gachaId:prizeIndex".</summary>
    public Dictionary<string, int> SharedPrizeClaims { get; init; } = [];
}

