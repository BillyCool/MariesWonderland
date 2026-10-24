using MariesWonderland.Constants;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Persistence;
using System.Collections.Concurrent;
using System.Text.Json;

namespace MariesWonderland.Data;

/// <summary>
/// Manages the map of userId → <see cref="DarkUserMemoryDatabase"/>. Handles user registration and sessions.
/// Users are worked on in memory. With a <see cref="IUserPersistence"/> (the server), every change is also
/// written to the user database: requests through <see cref="SaveChanges(long, Dictionary{string, TableSnapshot}, Dictionary{string, TableSnapshot}, DarkUserMemoryDatabase)"/>,
/// new and replaced users whole. Users not in memory are loaded on first use, and idle ones can be dropped from
/// memory (<see cref="EvictIdle"/>). Without one (tests), everything stays in memory.
/// The maps are safe to use from parallel requests. A user's own database isn't: requests reach it under that
/// user's lock (<see cref="Locks"/>).
/// </summary>
public class UserDataStore(TimeProvider timeProvider, IUserPersistence? persistence = null, ILogger<UserDataStore>? logger = null)
{
    /// <summary>How long code waits for another user's lock (or a user's lock outside a request) before giving up.</summary>
    public static readonly TimeSpan LockTimeout = TimeSpan.FromSeconds(10);

    private readonly ConcurrentDictionary<long, DarkUserMemoryDatabase> _users = new();
    private readonly ConcurrentDictionary<long, long> _lastUsedTicks = new();
    private readonly ConcurrentDictionary<string, long> _uuidToUserId = new();
    private readonly ConcurrentDictionary<string, UserSession> _sessions = new();
    private readonly ConcurrentDictionary<long, List<EntitySBattleDetail>> _battleLogs = new();
    private readonly Dictionary<string, int> _sharedPrizeClaims = new(persistence?.SharedPrizeClaims() ?? new Dictionary<string, int>());
    private readonly Lock _sharedPrizeLock = new();
    private readonly Lock _registrationLock = new();
    private readonly TimeProvider _timeProvider = timeProvider;
    private readonly IUserPersistence? _persistence = persistence;
    private readonly ILogger<UserDataStore>? _logger = logger;

    /// <summary>The per-user locks requests run under.</summary>
    public UserLocks Locks { get; } = new();

    /// <summary>Whether changes are written to a user database.</summary>
    public bool IsPersistent => _persistence != null;

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
            _persistence?.SaveSharedPrizeClaim(key, claimed + 1);
            return true;
        }
    }

    private static string SharedPrizeKey(int gachaId, int prizeIndex) => $"{gachaId}:{prizeIndex}";

    /// <summary>
    /// Appends a finished battle to the user's battle log. The log isn't part of the in-memory user: it goes
    /// straight to the user database (or a memory-only list without one). A failed write is logged, not thrown.
    /// </summary>
    public void RecordBattle(EntitySBattleDetail detail)
    {
        if (_persistence == null)
        {
            List<EntitySBattleDetail> log = _battleLogs.GetOrAdd(detail.UserId, _ => []);
            lock (log)
            {
                log.Add(detail);
            }
            return;
        }

        try
        {
            _persistence.AddBattleDetail(detail);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Recording a battle of user {UserId} failed", detail.UserId);
        }
    }

    /// <summary>The user's battle log, oldest first.</summary>
    public IReadOnlyList<EntitySBattleDetail> BattleDetails(long userId)
    {
        if (_persistence != null)
            return _persistence.BattleDetails(userId);

        if (!_battleLogs.TryGetValue(userId, out List<EntitySBattleDetail>? log))
            return [];

        lock (log)
        {
            return [.. log];
        }
    }

    /// <summary>The battles of one quest recorded at or after <paramref name="sinceUtc"/>, oldest first.</summary>
    public IReadOnlyList<EntitySBattleDetail> BattleDetails(long userId, int questId, DateTime sinceUtc)
    {
        if (_persistence != null)
            return _persistence.BattleDetails(userId, questId, sinceUtc);

        if (!_battleLogs.TryGetValue(userId, out List<EntitySBattleDetail>? log))
            return [];

        lock (log)
        {
            return [.. log.Where(d => d.QuestId == questId && d.RecordedAt >= sinceUtc)];
        }
    }

    /// <summary>
    /// Look up or create a user by UUID. Returns the userId and whether the user is new.
    /// For new users, seeds the initial data into their database.
    /// </summary>
    public (long UserId, bool IsNew) RegisterOrGetUser(string uuid, DarkMasterMemoryDatabase masterDb)
    {
        if (TryFindDevice(uuid, out long existingId))
            return (existingId, false);

        lock (_registrationLock)
        {
            if (TryFindDevice(uuid, out existingId))
                return (existingId, false);

            long userId = NewUserId();
            DarkUserMemoryDatabase db = new() { UserId = userId };
            SeedInitialUserData(db, masterDb);
            _persistence?.ReplaceUser(db);
            _persistence?.SaveDevice(uuid, userId);
            _users[userId] = db;
            Touch(userId);
            _uuidToUserId[uuid] = userId;
            return (userId, true);
        }
    }

    private bool TryFindDevice(string uuid, out long userId)
    {
        if (_uuidToUserId.TryGetValue(uuid, out userId))
            return true;

        long? saved = _persistence?.FindUserIdByDevice(uuid);
        if (saved == null)
            return false;

        userId = saved.Value;
        _uuidToUserId[uuid] = userId;
        return true;
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
        _persistence?.SaveSession(session, now);
        _sessions[key] = session;
        return session;
    }

    /// <summary>
    /// Resolves a session key to a userId if the session exists and has not expired.
    /// </summary>
    public bool TryResolveSession(string sessionKey, out long userId)
    {
        if (!_sessions.TryGetValue(sessionKey, out UserSession? session))
        {
            session = _persistence?.FindSession(sessionKey);
            if (session != null)
                _sessions[sessionKey] = session;
        }

        if (session != null && session.ExpiresAt > _timeProvider.GetUtcNow().UtcDateTime)
        {
            userId = session.UserId;
            return true;
        }
        userId = 0;
        return false;
    }

    /// <summary>
    /// Returns the user's database (from memory, else the user database), creating an empty one if not found.
    /// </summary>
    public DarkUserMemoryDatabase GetOrCreate(long userId)
    {
        Touch(userId);
        return _users.GetOrAdd(userId, id => _persistence?.LoadUser(id) ?? new DarkUserMemoryDatabase { UserId = id });
    }

    /// <summary>
    /// Registers a pre-seeded database under a UUID. If the UUID is already mapped,
    /// returns the existing userId without overwriting. Otherwise stores the seeded
    /// database and maps the UUID to the userId found inside it.
    /// </summary>
    public long SeedUserFromDatabase(string uuid, DarkUserMemoryDatabase seededDb)
    {
        lock (_registrationLock)
        {
            if (TryFindDevice(uuid, out long existingId))
                return existingId;

            long userId = seededDb.EntityIUser.FirstOrDefault()?.UserId ?? NewUserId();
            seededDb.UserId = userId;
            _persistence?.ReplaceUser(seededDb);
            _persistence?.SaveDevice(uuid, userId);
            _users[userId] = seededDb;
            Touch(userId);
            _uuidToUserId[uuid] = userId;
            return userId;
        }
    }

    /// <summary>
    /// Attempts to retrieve a user's database (from memory, else the user database) without creating one.
    /// </summary>
    public bool TryGet(long userId, out DarkUserMemoryDatabase db)
    {
        if (!_users.TryGetValue(userId, out db!))
        {
            DarkUserMemoryDatabase? saved = _persistence?.LoadUser(userId);
            if (saved == null)
                return false;

            db = _users.GetOrAdd(userId, saved);
        }

        Touch(userId);
        return true;
    }

    /// <summary>
    /// Finds the user database whose EntityIUser record matches the given playerId.
    /// Returns false if no user has that playerId.
    /// </summary>
    public bool TryGetByPlayerId(long playerId, out DarkUserMemoryDatabase db)
    {
        foreach ((long userId, DarkUserMemoryDatabase userDb) in _users)
        {
            if (userDb.EntityIUser.Any(u => u.PlayerId == playerId))
            {
                Touch(userId);
                db = userDb;
                return true;
            }
        }

        long? saved = _persistence?.FindUserIdByPlayerId(playerId);
        if (saved != null && TryGet(saved.Value, out db))
            return true;

        db = null!;
        return false;
    }

    /// <summary>
    /// Stores a user database, replacing any existing one for that userId (in the user database too).
    /// </summary>
    public void Set(long userId, DarkUserMemoryDatabase db)
    {
        db.UserId = userId;
        _persistence?.ReplaceUser(db);
        _users[userId] = db;
        Touch(userId);
    }

    /// <summary>The users in memory now.</summary>
    public IReadOnlyDictionary<long, DarkUserMemoryDatabase> All => _users;

    /// <summary>
    /// Writes what a request changed in the user's database, given snapshots from before and after it. Call it
    /// while holding the user's lock. If the write fails (say the saved rows had drifted), the whole user is
    /// written again; if that fails too, it's logged and the in-memory user stays as it is.
    /// </summary>
    public void SaveChanges(long userId, Dictionary<string, TableSnapshot> before, Dictionary<string, TableSnapshot> after, DarkUserMemoryDatabase userDb)
    {
        if (_persistence == null)
            return;

        UserChangeSet changes = UserDataDiffBuilder.Changes(before, after);
        if (changes.IsEmpty)
            return;

        try
        {
            _persistence.SaveChanges(userId, changes);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Saving the changes of user {UserId} failed; writing the whole user instead", userId);
            try
            {
                _persistence.ReplaceUser(userDb);
            }
            catch (Exception replaceEx)
            {
                _logger?.LogError(replaceEx, "Writing user {UserId} failed; their latest changes are only in memory", userId);
            }
        }
    }

    /// <summary>Like the other overload, snapshotting the user as they are now.</summary>
    public void SaveChanges(long userId, Dictionary<string, TableSnapshot> before, DarkUserMemoryDatabase userDb)
    {
        if (_persistence != null)
            SaveChanges(userId, before, UserDataDiffBuilder.Snapshot(userDb), userDb);
    }

    /// <summary>
    /// Drops users not used for <paramref name="idleFor"/> from memory (they're already saved). Users busy right now
    /// are skipped. Does nothing without a user database. Returns how many were dropped.
    /// </summary>
    public int EvictIdle(TimeSpan idleFor)
    {
        if (_persistence == null)
            return 0;

        long cutoff = _timeProvider.GetUtcNow().UtcTicks - idleFor.Ticks;
        int evicted = 0;
        foreach (long userId in _users.Keys)
        {
            if (_lastUsedTicks.GetValueOrDefault(userId) > cutoff)
                continue;

            using IDisposable? userLock = Locks.TryAcquire(userId, TimeSpan.Zero);
            if (userLock == null || _lastUsedTicks.GetValueOrDefault(userId) > cutoff)
                continue;

            if (_users.TryRemove(userId, out _))
            {
                _lastUsedTicks.TryRemove(userId, out _);
                evicted++;
            }
        }
        return evicted;
    }

    private void Touch(long userId) => _lastUsedTicks[userId] = _timeProvider.GetUtcNow().UtcTicks;

    /// <summary>The ids of every user: saved ones with a user database, else the ones in memory.</summary>
    public IReadOnlyList<long> UserIds() => _persistence?.UserIds() ?? [.. _users.Keys];

    /// <summary>
    /// The user's data as JSON, a save point for <see cref="RestoreUser"/>; null when there's no such user. Taken
    /// while holding the user's lock, so no request is changing them mid-write.
    /// </summary>
    public string? ExportUser(long userId)
    {
        using IDisposable userLock = Locks.TryAcquire(userId, LockTimeout)
            ?? throw new TimeoutException($"User {userId} stayed busy.");
        return TryGet(userId, out DarkUserMemoryDatabase db) ? JsonSerializer.Serialize(db, SavePointOptions) : null;
    }

    /// <summary>
    /// Puts a user back as a save point from <see cref="ExportUser"/> has them, in memory and in the user database.
    /// Their devices and sessions stay as they are. Returns the user's id.
    /// </summary>
    public long RestoreUser(string json)
    {
        DarkUserMemoryDatabase db = JsonSerializer.Deserialize<DarkUserMemoryDatabase>(json)
            ?? throw new InvalidDataException("The save point is empty.");
        long userId = db.EntityIUser.FirstOrDefault()?.UserId ?? db.UserId;
        if (userId == 0)
            throw new InvalidDataException("The save point has no user.");

        using IDisposable userLock = Locks.TryAcquire(userId, LockTimeout)
            ?? throw new TimeoutException($"User {userId} stayed busy.");
        Set(userId, db);
        return userId;
    }

    private static readonly JsonSerializerOptions SavePointOptions = new() { WriteIndented = true };

    /// <summary>
    /// Imports a whole-server JSON snapshot (the file the server kept before it had a user database), replacing the
    /// current in-memory state; with a user database, the file's users, devices, sessions and prize claims are
    /// written there too (users whole).
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
        _lastUsedTicks.Clear();
        _uuidToUserId.Clear();
        _sessions.Clear();

        DateTime now = _timeProvider.GetUtcNow().UtcDateTime;
        foreach ((long userId, DarkUserMemoryDatabase db) in snapshot.Users)
        {
            db.UserId = userId;
            _persistence?.ReplaceUser(db);
            _users[userId] = db;
            Touch(userId);
        }

        foreach ((string uuid, long userId) in snapshot.UuidToUserId)
        {
            _persistence?.SaveDevice(uuid, userId);
            _uuidToUserId[uuid] = userId;
        }

        foreach (UserSession session in snapshot.Sessions)
        {
            if (session.ExpiresAt > now)
                _persistence?.SaveSession(session, now);
            _sessions[session.SessionKey] = session;
        }

        lock (_sharedPrizeLock)
        {
            _sharedPrizeClaims.Clear();
            foreach ((string key, int claimed) in snapshot.SharedPrizeClaims)
            {
                _persistence?.SaveSharedPrizeClaim(key, claimed);
                _sharedPrizeClaims[key] = claimed;
            }
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
        while (_users.ContainsKey(userId) || _persistence?.LoadUser(userId) != null);
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
/// The whole-server JSON snapshot the server kept before it had a user database, imported on a first start.
/// </summary>
file record UserDataSnapshot
{
    public Dictionary<long, DarkUserMemoryDatabase> Users { get; init; } = [];
    public Dictionary<string, long> UuidToUserId { get; init; } = [];
    public List<UserSession> Sessions { get; init; } = [];

    /// <summary>Server-wide claims of shared limited summon prizes, keyed "gachaId:prizeIndex".</summary>
    public Dictionary<string, int> SharedPrizeClaims { get; init; } = [];
}

