using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Models.Entities;
using MariesWonderland.Proto.Data;
using MariesWonderland.Proto.User;

namespace MariesWonderland.Services;

public class UserService(UserDataStore store) : MariesWonderland.Proto.User.UserService.UserServiceBase
{
    private readonly UserDataStore _store = store;

    public override Task<GetAndroidArgsResponse> GetAndroidArgs(GetAndroidArgsRequest request, ServerCallContext context)
    {
        return Task.FromResult(new GetAndroidArgsResponse
        {
            ApiKey = "1234567890",
            Nonce = "Mama"
        });
    }

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
        var diffData = UserDataDiffBuilder.FullDiff(userDb);

        var response = new AuthUserResponse
        {
            SessionKey = session.SessionKey,
            ExpireDatetime = Timestamp.FromDateTime(session.ExpiresAt),
            Signature = request.Signature,
            UserId = userId
        };
        foreach (var (k, v) in diffData) response.DiffUserData[k] = v;

        return Task.FromResult(response);
    }

    public override Task<CheckTransferSettingResponse> CheckTransferSetting(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new CheckTransferSettingResponse());
    }

    public override Task<GameStartResponse> GameStart(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new GameStartResponse());
    }

    public override Task<GetBackupTokenResponse> GetBackupToken(GetBackupTokenRequest request, ServerCallContext context)
    {
        return Task.FromResult(new GetBackupTokenResponse
        {
            BackupToken = "1234567890"
        });
    }

    public override Task<GetBirthYearMonthResponse> GetBirthYearMonth(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new GetBirthYearMonthResponse());
    }

    public override Task<GetChargeMoneyResponse> GetChargeMoney(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new GetChargeMoneyResponse());
    }

    public override Task<GetUserGamePlayNoteResponse> GetUserGamePlayNote(GetUserGamePlayNoteRequest request, ServerCallContext context)
    {
        return Task.FromResult(new GetUserGamePlayNoteResponse());
    }

    public override Task<GetUserProfileResponse> GetUserProfile(GetUserProfileRequest request, ServerCallContext context)
    {
        return Task.FromResult(new GetUserProfileResponse());
    }

    public override Task<RegisterUserResponse> RegisterUser(RegisterUserRequest request, ServerCallContext context)
    {
        // RegisterUser is the very first API called on a fresh install. It registers the device UUID
        // and assigns a permanent userId (random 19-digit number). Subsequent launches call Auth instead.
        var (userId, _) = _store.RegisterOrGetUser(request.Uuid);
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);
        Dictionary<string, DiffData> diffData = UserDataDiffBuilder.FullDiff(userDb);

        RegisterUserResponse response = new()
        {
            UserId = userId,
            Signature = $"sig_{userId}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}"
        };
        foreach (var (k, v) in diffData) response.DiffUserData[k] = v;

        return Task.FromResult(response);
    }

    public override Task<SetAppleAccountResponse> SetAppleAccount(SetAppleAccountRequest request, ServerCallContext context)
    {
        return Task.FromResult(new SetAppleAccountResponse());
    }

    public override Task<SetBirthYearMonthResponse> SetBirthYearMonth(SetBirthYearMonthRequest request, ServerCallContext context)
    {
        return Task.FromResult(new SetBirthYearMonthResponse());
    }

    public override Task<SetFacebookAccountResponse> SetFacebookAccount(SetFacebookAccountRequest request, ServerCallContext context)
    {
        return Task.FromResult(new SetFacebookAccountResponse());
    }

    public override Task<SetUserFavoriteCostumeIdResponse> SetUserFavoriteCostumeId(SetUserFavoriteCostumeIdRequest request, ServerCallContext context)
    {
        return Task.FromResult(new SetUserFavoriteCostumeIdResponse());
    }

    public override Task<SetUserMessageResponse> SetUserMessage(SetUserMessageRequest request, ServerCallContext context)
    {
        return Task.FromResult(new SetUserMessageResponse());
    }

    public override Task<SetUserNameResponse> SetUserName(SetUserNameRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        Dictionary<string, string> before = UserDataDiffBuilder.Snapshot(userDb);

        EntityIUserProfile profile = userDb.EntityIUserProfile.FirstOrDefault(p => p.UserId == userId)
            ?? AddEntity(userDb.EntityIUserProfile, new EntityIUserProfile { UserId = userId });

        profile.Name = request.Name;
        profile.NameUpdateDatetime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        SetUserNameResponse response = new();
        foreach (var (k, v) in UserDataDiffBuilder.Delta(before, userDb)) response.DiffUserData[k] = v;

        return Task.FromResult(response);
    }

    /// <summary>Adds an entity to a list and returns it (convenience for inline new-entity seeding).</summary>
    private static T AddEntity<T>(List<T> list, T entity)
    {
        list.Add(entity);
        return entity;
    }

    public override Task<SetUserSettingResponse> SetUserSetting(SetUserSettingRequest request, ServerCallContext context)
    {
        return Task.FromResult(new SetUserSettingResponse());
    }

    public override Task<TransferUserResponse> TransferUser(TransferUserRequest request, ServerCallContext context)
    {
        return Task.FromResult(new TransferUserResponse());
    }

    public override Task<TransferUserByAppleResponse> TransferUserByApple(TransferUserByAppleRequest request, ServerCallContext context)
    {
        return Task.FromResult(new TransferUserByAppleResponse());
    }

    public override Task<TransferUserByFacebookResponse> TransferUserByFacebook(TransferUserByFacebookRequest request, ServerCallContext context)
    {
        return Task.FromResult(new TransferUserByFacebookResponse());
    }

    public override Task<UnsetFacebookAccountResponse> UnsetFacebookAccount(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new UnsetFacebookAccountResponse());
    }
}
