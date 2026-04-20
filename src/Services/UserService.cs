using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Helpers;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.User;

namespace MariesWonderland.Services;

public class UserService(UserDataStore store, UserDataSeeder seeder) : MariesWonderland.Proto.User.UserService.UserServiceBase
{
    public const string AndroidApiKey = "1234567890";
    public const string AndroidNonce = "Mama";

    private readonly UserDataStore _store = store;
    private readonly UserDataSeeder _seeder = seeder;

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
        var (userId, isNew) = _store.RegisterOrGetUser(request.Uuid);
        var userDb = _store.GetOrCreate(userId);

        var nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
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

    /// <summary>Initializes the game session: sets game start time and ensures gem balance exists.</summary>
    public override Task<GameStartResponse> GameStart(Empty request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        EntityIUser user = userDb.EntityIUser.GetOrCreate(userId);
        user.GameStartDatetime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        // Initialize gem balance with 0/0 if not exists
        if (!userDb.EntityIUserGem.Any(g => g.UserId == userId))
        {
            userDb.EntityIUserGem.Add(new EntityIUserGem { UserId = userId, PaidGem = 0, FreeGem = 0 });
        }

        // TODO: Investigate if these singleton tables need to be pre-initialized at registration.
        // Uncomment to enable initialization:
        // userDb.EntityIUserBigHuntProgressStatus.AddNew(new EntityIUserBigHuntProgressStatus { UserId = userId });
        // userDb.EntityIUserEventQuestGuerrillaFreeOpen.AddNew(new EntityIUserEventQuestGuerrillaFreeOpen { UserId = userId });
        // userDb.EntityIUserEventQuestProgressStatus.AddNew(new EntityIUserEventQuestProgressStatus { UserId = userId });
        // userDb.EntityIUserExplore.AddNew(new EntityIUserExplore { UserId = userId });
        // userDb.EntityIUserExtraQuestProgressStatus.AddNew(new EntityIUserExtraQuestProgressStatus { UserId = userId });
        // userDb.EntityIUserMainQuestFlowStatus.AddNew(new EntityIUserMainQuestFlowStatus { UserId = userId });
        // userDb.EntityIUserMainQuestMainFlowStatus.AddNew(new EntityIUserMainQuestMainFlowStatus { UserId = userId });
        // userDb.EntityIUserMainQuestProgressStatus.AddNew(new EntityIUserMainQuestProgressStatus { UserId = userId });
        // userDb.EntityIUserMainQuestReplayFlowStatus.AddNew(new EntityIUserMainQuestReplayFlowStatus { UserId = userId });
        // userDb.EntityIUserMainQuestSeasonRoute.AddNew(new EntityIUserMainQuestSeasonRoute { UserId = userId });
        // userDb.EntityIUserPortalCageStatus.AddNew(new EntityIUserPortalCageStatus { UserId = userId });
        // userDb.EntityIUserShopReplaceable.AddNew(new EntityIUserShopReplaceable { UserId = userId });
        // userDb.EntityIUserSideStoryQuestSceneProgressStatus.AddNew(new EntityIUserSideStoryQuestSceneProgressStatus { UserId = userId });

        // TODO: Initialize first mission record (missionId=1, IN_PROGRESS) at registration.
        // userDb.EntityIUserMission.AddNew(new EntityIUserMission
        // {
        //     UserId = userId,
        //     MissionId = 1,
        //     MissionProgressStatusType = MissionProgressStatusType.IN_PROGRESS
        // });

        return Task.FromResult(new GameStartResponse());
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
        var (userId, _) = _store.RegisterOrGetUser(request.Uuid);

        RegisterUserResponse response = new()
        {
            UserId = userId,
            Signature = $"sig_{userId}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}"
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
        profile.FavoriteCostumeIdUpdateDatetime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        return Task.FromResult(new SetUserFavoriteCostumeIdResponse());
    }

    /// <summary>Updates the user's profile message.</summary>
    public override Task<SetUserMessageResponse> SetUserMessage(SetUserMessageRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        EntityIUserProfile profile = userDb.EntityIUserProfile.GetOrCreate(userId);

        profile.Message = request.Message;
        profile.MessageUpdateDatetime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        return Task.FromResult(new SetUserMessageResponse());
    }

    /// <summary>Updates the user's display name.</summary>
    public override Task<SetUserNameResponse> SetUserName(SetUserNameRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        EntityIUserProfile profile = userDb.EntityIUserProfile.GetOrCreate(userId);

        profile.Name = request.Name;
        profile.NameUpdateDatetime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

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
            Signature = $"sig_{userId}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}"
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
