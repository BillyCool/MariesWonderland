using Art.Framework.ApiNetwork.Grpc.Api.User;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace MariesWonderland.Services;

public class UserService : Art.Framework.ApiNetwork.Grpc.Api.User.UserService.UserServiceBase
{
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
        return Task.FromResult(new AuthUserResponse
        {
            ExpireDatetime = Timestamp.FromDateTime(DateTime.UtcNow.AddDays(30)),
            UserId = 1234567890123450000,
            SessionKey = "1234567890",
            Signature = "V2UnbGxQbGF5QWdhaW5Tb21lZGF5TXJNb25zdGVyIQ=="
        });
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
        return Task.FromResult(new RegisterUserResponse
        {
            UserId = 1234567890123450000,
            Signature = "V2UnbGxQbGF5QWdhaW5Tb21lZGF5TXJNb25zdGVyIQ=="
        });
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
        return Task.FromResult(new SetUserNameResponse());
    }

    public override Task<SetUserSettingResponse> SetUserSetting(SetUserSettingRequest request, ServerCallContext context)
    {
        return Task.FromResult(new SetUserSettingResponse());
    }

    public override Task<TransferUserResponse> TransferUser(TransferUserRequest request, ServerCallContext context)
    {
        return Task.FromResult(new TransferUserResponse
        {
            UserId = 1234567890123450000,
            Signature = "V2UnbGxQbGF5QWdhaW5Tb21lZGF5TXJNb25zdGVyIQ=="
        });
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
