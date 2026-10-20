using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Helpers;
using MariesWonderland.Missions;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.User;

namespace MariesWonderland.Services;

public class UserService(UserDataStore store, UserDataSeeder seeder, DarkMasterMemoryDatabase masterDb, MissionTracker missions, TimeProvider timeProvider) : MariesWonderland.Proto.User.UserService.UserServiceBase
{
    private readonly TimeProvider _timeProvider = timeProvider;

    public const string AndroidApiKey = "1234567890";
    public const string AndroidNonce = "Mama";

    private readonly UserDataStore _store = store;
    private readonly UserDataSeeder _seeder = seeder;
    private readonly DarkMasterMemoryDatabase _masterDb = masterDb;
    private readonly MissionTracker _missions = missions;

    /// <summary>Returns Android-specific arguments (API key and nonce) for client initialization.</summary>
    public override Task<GetAndroidArgsResponse> GetAndroidArgs(GetAndroidArgsRequest request, ServerCallContext context)
    {
        return Task.FromResult(new GetAndroidArgsResponse
        {
            ApiKey = AndroidApiKey,
            Nonce = AndroidNonce
        });
    }

    /// <summary>Authenticates a user by UUID, creates/updates device records, and returns a session key.</summary>
    public override Task<AuthUserResponse> Auth(AuthUserRequest request, ServerCallContext context)
    {
        var (userId, isNew) = _store.RegisterOrGetUser(request.Uuid, _masterDb);
        var userDb = _store.GetOrCreate(userId);

        var nowMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();
        var deviceRecord = userDb.EntitySUserDevice.FirstOrDefault(d => d.UserId == userId);
        if (deviceRecord == null)
        {
            deviceRecord = new EntitySUserDevice { UserId = userId };
            userDb.EntitySUserDevice.Add(deviceRecord);
        }
        deviceRecord.Uuid = request.Uuid;
        deviceRecord.AdvertisingId = request.AdvertisingId;
        deviceRecord.IsTrackingEnabled = request.IsTrackingEnabled;
        deviceRecord.IdentifierForVendor = request.DeviceInherent?.IdentifierForVendor ?? "";
        deviceRecord.DeviceToken = request.DeviceInherent?.DeviceToken ?? "";
        deviceRecord.MacAddress = request.DeviceInherent?.MacAddress ?? "";
        deviceRecord.RegistrationId = request.DeviceInherent?.RegistrationId ?? "";
        deviceRecord.LastAuthAt = nowMs;
        if (isNew) deviceRecord.RegisteredAt = nowMs;

        var session = _store.CreateSession(userId, TimeSpan.FromHours(24));

        var response = new AuthUserResponse
        {
            SessionKey = session.SessionKey,
            ExpireDatetime = Timestamp.FromDateTime(session.ExpiresAt),
            Signature = request.Signature,
            UserId = userId
        };

        return Task.FromResult(response);
    }

    /// <summary>Stub for transfer setting check; returns empty response.</summary>
    public override Task<CheckTransferSettingResponse> CheckTransferSetting(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new CheckTransferSettingResponse());
    }

    /// <summary>
    /// Starts the game session: records the game start time, brings missions up to date and, on the first start of
    /// a game day, counts the login (login record and login missions).
    /// </summary>
    public override Task<GameStartResponse> GameStart(Empty request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        long nowMs = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();
        EntityIUser user = userDb.EntityIUser.GetOrCreate(userId);
        bool isFirstStartToday = GameDayHelper.IsBeforeToday(user.GameStartDatetime, nowMs);
        user.GameStartDatetime = nowMs;

        // Missions that became available since the last session, daily restarts, state conditions.
        _missions.Refresh(userDb, nowMs);

        if (isFirstStartToday)
        {
            RecordDailyLogin(userDb, userId, nowMs);
            _missions.Record(userDb, MissionClearConditionType.USER_LOGIN_BY_COUNT, 1, nowMs);
            _missions.Record(userDb, MissionClearConditionType.USER_LOGIN_BY_COUNT_FROM_UNLOCK, 1, nowMs);
        }

        return Task.FromResult(new GameStartResponse());
    }

    /// <summary>
    /// Updates the login record for a game day's first start: total days, the run of consecutive days and its best.
    /// A record already stamped today (registration does that) is left as it is.
    /// </summary>
    private static void RecordDailyLogin(DarkUserMemoryDatabase userDb, long userId, long nowMs)
    {
        EntityIUserLogin login = userDb.EntityIUserLogin.GetOrCreate(userId);
        long today = GameDayHelper.StartOfDay(nowMs);
        if (login.LastLoginDatetime >= today)
        {
            return;
        }

        long yesterday = today - (long)TimeSpan.FromDays(1).TotalMilliseconds;
        login.ContinualLoginCount = login.LastLoginDatetime >= yesterday ? login.ContinualLoginCount + 1 : 1;
        login.MaxContinualLoginCount = Math.Max(login.MaxContinualLoginCount, login.ContinualLoginCount);
        login.TotalLoginCount++;
        login.LastLoginDatetime = nowMs;
        login.LatestVersion = nowMs;
    }

    /// <summary>Returns the user's backup token for account recovery.</summary>
    public override Task<GetBackupTokenResponse> GetBackupToken(GetBackupTokenRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        EntitySUser? sUser = userDb.EntitySUser.FirstOrDefault(u => u.UserId == userId);
        string token = sUser?.BackupToken ?? "";

        if (string.IsNullOrEmpty(token))
        {
            token = "mock-backup-token";
        }

        return Task.FromResult(new GetBackupTokenResponse
        {
            BackupToken = token
        });
    }

    /// <summary>Returns the user's birth year and month.</summary>
    public override Task<GetBirthYearMonthResponse> GetBirthYearMonth(Empty request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        EntityIUser? user = userDb.EntityIUser.FirstOrDefault(u => u.UserId == userId);

        return Task.FromResult(new GetBirthYearMonthResponse
        {
            BirthYear = user?.BirthYear ?? 2000,
            BirthMonth = user?.BirthMonth ?? 1
        });
    }

    /// <summary>Returns the user's charge money amount for the current month.</summary>
    public override Task<GetChargeMoneyResponse> GetChargeMoney(Empty request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        EntitySUser? sUser = userDb.EntitySUser.FirstOrDefault(u => u.UserId == userId);

        return Task.FromResult(new GetChargeMoneyResponse
        {
            ChargeMoneyThisMonth = sUser?.ChargeMoneyThisMonth ?? 0
        });
    }

    /// <summary>Stub for game play note retrieval; returns empty response.</summary>
    public override Task<GetUserGamePlayNoteResponse> GetUserGamePlayNote(GetUserGamePlayNoteRequest request, ServerCallContext context)
    {
        return Task.FromResult(new GetUserGamePlayNoteResponse());
    }

    /// <summary>Returns a player's profile including name, level, favorite costume, and lead deck character.</summary>
    public override Task<GetUserProfileResponse> GetUserProfile(GetUserProfileRequest request, ServerCallContext context)
    {
        long callerUserId = context.GetUserId();

        if (!_store.TryGetByPlayerId(request.PlayerId, out DarkUserMemoryDatabase targetDb))
            return Task.FromResult(new GetUserProfileResponse());

        EntityIUserProfile? profile = targetDb.EntityIUserProfile.FirstOrDefault(p => p.UserId == targetDb.UserId);
        EntityIUserStatus? status = targetDb.EntityIUserStatus.FirstOrDefault(s => s.UserId == targetDb.UserId);
        bool isOwnProfile = targetDb.UserId == callerUserId;
        int maxDeckPower = 0;

        List<ProfileDeckCharacter> deckCharacters = [];

        EntityIUserDeck? deck = targetDb.EntityIUserDeck.FirstOrDefault(d =>
            d.DeckType == DeckType.QUEST && d.UserDeckNumber == 1);

        if (deck != null && !string.IsNullOrEmpty(deck.UserDeckCharacterUuid01))
        {
            EntityIUserDeckCharacter? dc = targetDb.EntityIUserDeckCharacter
                .FirstOrDefault(c => c.UserDeckCharacterUuid == deck.UserDeckCharacterUuid01);

            if (dc != null)
            {
                maxDeckPower = dc.Power;
                int costumeId = targetDb.EntityIUserCostume
                    .FirstOrDefault(c => c.UserCostumeUuid == dc.UserCostumeUuid)?.CostumeId ?? 0;

                EntityIUserWeapon? weapon = targetDb.EntityIUserWeapon
                    .FirstOrDefault(w => w.UserWeaponUuid == dc.MainUserWeaponUuid);

                deckCharacters.Add(new ProfileDeckCharacter
                {
                    CostumeId = costumeId,
                    MainWeaponId = weapon?.WeaponId ?? 0,
                    MainWeaponLevel = weapon?.Level ?? 0
                });
            }
        }

        return Task.FromResult(new GetUserProfileResponse
        {
            Level = status?.Level ?? 0,
            Name = profile?.Name ?? string.Empty,
            FavoriteCostumeId = profile?.FavoriteCostumeId ?? 0,
            Message = profile?.Message ?? string.Empty,
            IsFriend = false,
            LatestUsedDeck = new ProfileDeck
            {
                Power = maxDeckPower,
                DeckCharacter = { deckCharacters }
            },
            PvpInfo = new ProfilePvpInfo()
            {
                MaxSeasonRank = targetDb.EntityIUserPvpWeeklyResult
                    .GroupBy(x => x.PvpSeasonId)
                    .DefaultIfEmpty()
                    .Min(x => x?.OrderBy(y => y.PvpWeeklyVersion).Select(y => y.FinalRank).LastOrDefault() ?? 0),
            },
            GamePlayHistory = new GamePlayHistory
            {
                HistoryItem = { },
                HistoryCategoryGraphItem = { }
            }
        });
    }

    /// <summary>Registers a new device UUID and assigns a permanent user ID.</summary>
    public override Task<RegisterUserResponse> RegisterUser(RegisterUserRequest request, ServerCallContext context)
    {
        // RegisterUser is the very first API called on a fresh install. It registers the device UUID
        // and assigns a permanent userId (random 19-digit number). Subsequent launches call Auth instead.
        var (userId, _) = _store.RegisterOrGetUser(request.Uuid, _masterDb);

        RegisterUserResponse response = new()
        {
            UserId = userId,
            Signature = $"sig_{userId}_{_timeProvider.GetUtcNow().ToUnixTimeSeconds()}"
        };

        return Task.FromResult(response);
    }

    /// <summary>Stub for Apple account linking; returns empty response.</summary>
    public override Task<SetAppleAccountResponse> SetAppleAccount(SetAppleAccountRequest request, ServerCallContext context)
    {
        return Task.FromResult(new SetAppleAccountResponse());
    }

    /// <summary>Updates the user's birth year and month.</summary>
    public override Task<SetBirthYearMonthResponse> SetBirthYearMonth(SetBirthYearMonthRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        EntityIUser user = userDb.EntityIUser.GetOrCreate(userId);

        user.BirthYear = request.BirthYear;
        user.BirthMonth = request.BirthMonth;

        return Task.FromResult(new SetBirthYearMonthResponse());
    }

    /// <summary>Stub for Facebook account linking; returns empty response.</summary>
    public override Task<SetFacebookAccountResponse> SetFacebookAccount(SetFacebookAccountRequest request, ServerCallContext context)
    {
        return Task.FromResult(new SetFacebookAccountResponse());
    }

    /// <summary>Updates the user's favorite costume displayed on their profile.</summary>
    public override Task<SetUserFavoriteCostumeIdResponse> SetUserFavoriteCostumeId(SetUserFavoriteCostumeIdRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        EntityIUserProfile profile = userDb.EntityIUserProfile.GetOrCreate(userId);

        profile.FavoriteCostumeId = request.FavoriteCostumeId;
        profile.FavoriteCostumeIdUpdateDatetime = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();

        return Task.FromResult(new SetUserFavoriteCostumeIdResponse());
    }

    /// <summary>Updates the user's profile message.</summary>
    public override Task<SetUserMessageResponse> SetUserMessage(SetUserMessageRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        EntityIUserProfile profile = userDb.EntityIUserProfile.GetOrCreate(userId);

        profile.Message = request.Message;
        profile.MessageUpdateDatetime = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();

        return Task.FromResult(new SetUserMessageResponse());
    }

    /// <summary>Updates the user's display name.</summary>
    public override Task<SetUserNameResponse> SetUserName(SetUserNameRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        EntityIUserProfile profile = userDb.EntityIUserProfile.GetOrCreate(userId);

        profile.Name = request.Name;
        profile.NameUpdateDatetime = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();

        return Task.FromResult(new SetUserNameResponse());
    }

    /// <summary>Updates the user's notification preferences.</summary>
    public override Task<SetUserSettingResponse> SetUserSetting(SetUserSettingRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        EntityIUserSetting setting = userDb.EntityIUserSetting.GetOrCreate(userId);

        setting.IsNotifyPurchaseAlert = request.IsNotifyPurchaseAlert;

        return Task.FromResult(new SetUserSettingResponse());
    }

    /// <summary>Transfers account data from seed files, creating a new user with pre-seeded database.</summary>
    public override Task<TransferUserResponse> TransferUser(TransferUserRequest request, ServerCallContext context)
    {
        DarkUserMemoryDatabase seededDb = _seeder.LoadFromFiles();
        long userId = _store.SeedUserFromDatabase(request.Uuid, seededDb);

        TransferUserResponse response = new()
        {
            UserId = userId,
            Signature = $"sig_{userId}_{_timeProvider.GetUtcNow().ToUnixTimeSeconds()}"
        };
        return Task.FromResult(response);
    }

    /// <summary>Stub for Apple-based account transfer; returns empty response.</summary>
    public override Task<TransferUserByAppleResponse> TransferUserByApple(TransferUserByAppleRequest request, ServerCallContext context)
    {
        return Task.FromResult(new TransferUserByAppleResponse());
    }

    /// <summary>Stub for Facebook-based account transfer; returns empty response.</summary>
    public override Task<TransferUserByFacebookResponse> TransferUserByFacebook(TransferUserByFacebookRequest request, ServerCallContext context)
    {
        return Task.FromResult(new TransferUserByFacebookResponse());
    }

    /// <summary>Stub for unlinking Facebook account; returns empty response.</summary>
    public override Task<UnsetFacebookAccountResponse> UnsetFacebookAccount(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new UnsetFacebookAccountResponse());
    }
}
