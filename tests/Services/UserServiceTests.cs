using Google.Protobuf.WellKnownTypes;
using MariesWonderland.Configuration;
using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.User;
using MariesWonderland.Tests.Infrastructure;
using Microsoft.Extensions.Options;
using UserService = MariesWonderland.Services.UserService;

namespace MariesWonderland.Tests.Services;

public class UserServiceTests(MasterDatabaseFixture fixture) : ServiceTestBase(fixture), IClassFixture<MasterDatabaseFixture>
{
    private const long UserId = 1L;

    private static UserService CreateService(UserDataStore store)
    {
        var options = Options.Create(new ServerOptions());
        var seeder = new UserDataSeeder(options);
        return new UserService(store, seeder);
    }

    /// <summary>
    /// Verifies that SetUserName updates the profile name and sets the name update timestamp.
    /// </summary>
    [Fact]
    public async Task SetUserName_WithExistingProfile_UpdatesNameAndTimestamp()
    {
        var userDb = CreateUserDb();
        userDb.EntityIUserProfile.Add(new EntityIUserProfile
        {
            UserId = UserId,
            Name = "OldName"
        });
        var store = CreateStore(UserId, userDb, MasterDb);
        var service = CreateService(store);

        var response = await service.SetUserName(
            new SetUserNameRequest { Name = "NewName" },
            ContextFor(UserId));

        Assert.NotNull(response);
        Assert.Equal("NewName", userDb.EntityIUserProfile[0].Name);
        Assert.True(userDb.EntityIUserProfile[0].NameUpdateDatetime > 0);
    }

    /// <summary>
    /// Verifies that SetUserSetting updates the notification preference on the setting record.
    /// </summary>
    [Fact]
    public async Task SetUserSetting_WithExistingSetting_UpdatesNotifyPurchaseAlert()
    {
        var userDb = CreateUserDb();
        userDb.EntityIUserSetting.Add(new EntityIUserSetting
        {
            UserId = UserId,
            IsNotifyPurchaseAlert = false
        });
        var store = CreateStore(UserId, userDb, MasterDb);
        var service = CreateService(store);

        var response = await service.SetUserSetting(
            new SetUserSettingRequest { IsNotifyPurchaseAlert = true },
            ContextFor(UserId));

        Assert.NotNull(response);
        Assert.True(userDb.EntityIUserSetting[0].IsNotifyPurchaseAlert);
    }

    /// <summary>
    /// Verifies that SetUserMessage updates the profile message and sets the message update timestamp.
    /// </summary>
    [Fact]
    public async Task SetUserMessage_WithExistingProfile_UpdatesMessageAndTimestamp()
    {
        var userDb = CreateUserDb();
        userDb.EntityIUserProfile.Add(new EntityIUserProfile
        {
            UserId = UserId,
            Name = "TestUser"
        });
        var store = CreateStore(UserId, userDb, MasterDb);
        var service = CreateService(store);

        var response = await service.SetUserMessage(
            new SetUserMessageRequest { Message = "Hello World" },
            ContextFor(UserId));

        Assert.NotNull(response);
        Assert.Equal("Hello World", userDb.EntityIUserProfile[0].Message);
        Assert.True(userDb.EntityIUserProfile[0].MessageUpdateDatetime > 0);
    }

    /// <summary>
    /// Verifies that SetUserFavoriteCostumeId updates the profile's favorite costume and timestamp.
    /// </summary>
    [Fact]
    public async Task SetUserFavoriteCostumeId_WithExistingProfile_UpdatesCostumeIdAndTimestamp()
    {
        var userDb = CreateUserDb();
        userDb.EntityIUserProfile.Add(new EntityIUserProfile
        {
            UserId = UserId,
            Name = "TestUser"
        });
        var store = CreateStore(UserId, userDb, MasterDb);
        var service = CreateService(store);

        var response = await service.SetUserFavoriteCostumeId(
            new SetUserFavoriteCostumeIdRequest { FavoriteCostumeId = 42 },
            ContextFor(UserId));

        Assert.NotNull(response);
        Assert.Equal(42, userDb.EntityIUserProfile[0].FavoriteCostumeId);
        Assert.True(userDb.EntityIUserProfile[0].FavoriteCostumeIdUpdateDatetime > 0);
    }

    /// <summary>
    /// Verifies that SetBirthYearMonth updates the user's birth year and month.
    /// </summary>
    [Fact]
    public async Task SetBirthYearMonth_WithExistingUser_UpdatesBirthYearAndMonth()
    {
        var userDb = CreateUserDb();
        userDb.EntityIUser.Add(new EntityIUser { UserId = UserId });
        var store = CreateStore(UserId, userDb, MasterDb);
        var service = CreateService(store);

        var response = await service.SetBirthYearMonth(
            new SetBirthYearMonthRequest { BirthYear = 1990, BirthMonth = 6 },
            ContextFor(UserId));

        Assert.NotNull(response);
        Assert.Equal(1990, userDb.EntityIUser[0].BirthYear);
        Assert.Equal(6, userDb.EntityIUser[0].BirthMonth);
    }

    /// <summary>
    /// Verifies that GetBirthYearMonth returns the stored birth year and month.
    /// </summary>
    [Fact]
    public async Task GetBirthYearMonth_WithExistingUser_ReturnsStoredValues()
    {
        var userDb = CreateUserDb();
        userDb.EntityIUser.Add(new EntityIUser
        {
            UserId = UserId,
            BirthYear = 1985,
            BirthMonth = 3
        });
        var store = CreateStore(UserId, userDb, MasterDb);
        var service = CreateService(store);

        var response = await service.GetBirthYearMonth(
            new Empty(),
            ContextFor(UserId));

        Assert.Equal(1985, response.BirthYear);
        Assert.Equal(3, response.BirthMonth);
    }

    /// <summary>
    /// Verifies that GetChargeMoney returns the stored charge money amount.
    /// </summary>
    [Fact]
    public async Task GetChargeMoney_WithExistingSUser_ReturnsChargeMoneyThisMonth()
    {
        var userDb = CreateUserDb();
        userDb.EntitySUser.Add(new EntitySUser
        {
            UserId = UserId,
            ChargeMoneyThisMonth = 500
        });
        var store = CreateStore(UserId, userDb, MasterDb);
        var service = CreateService(store);

        var response = await service.GetChargeMoney(
            new Empty(),
            ContextFor(UserId));

        Assert.Equal(500, response.ChargeMoneyThisMonth);
    }

    /// <summary>
    /// Verifies that GetChargeMoney returns zero when no EntitySUser record exists.
    /// </summary>
    [Fact]
    public async Task GetChargeMoney_WithNoSUser_ReturnsZero()
    {
        var userDb = CreateUserDb();
        var store = CreateStore(UserId, userDb, MasterDb);
        var service = CreateService(store);

        var response = await service.GetChargeMoney(
            new Empty(),
            ContextFor(UserId));

        Assert.Equal(0, response.ChargeMoneyThisMonth);
    }

    [Fact]
    public async Task GetUserProfile_ReturnsLevelFromStatus()
    {
        var userDb = CreateUserDb();
        userDb.EntityIUser.Add(new EntityIUser { UserId = UserId, PlayerId = 12345L });
        userDb.EntityIUserStatus.Add(new EntityIUserStatus { UserId = UserId, Level = 42 });
        var store = CreateStore(UserId, userDb, MasterDb);
        var service = CreateService(store);

        var response = await service.GetUserProfile(
            new GetUserProfileRequest { PlayerId = 12345L },
            ContextFor(UserId));

        Assert.Equal(42, response.Level);
    }

    [Fact]
    public async Task GetUserProfile_ReturnsNameFromProfile()
    {
        var userDb = CreateUserDb();
        userDb.EntityIUser.Add(new EntityIUser { UserId = UserId, PlayerId = 12345L });
        userDb.EntityIUserProfile.Add(new EntityIUserProfile { UserId = UserId, Name = "TestPlayer" });
        var store = CreateStore(UserId, userDb, MasterDb);
        var service = CreateService(store);

        var response = await service.GetUserProfile(
            new GetUserProfileRequest { PlayerId = 12345L },
            ContextFor(UserId));

        Assert.Equal("TestPlayer", response.Name);
    }

    [Fact]
    public async Task GetUserProfile_ReturnsFavoriteCostumeIdFromProfile()
    {
        var userDb = CreateUserDb();
        userDb.EntityIUser.Add(new EntityIUser { UserId = UserId, PlayerId = 12345L });
        userDb.EntityIUserProfile.Add(new EntityIUserProfile { UserId = UserId, FavoriteCostumeId = 99 });
        var store = CreateStore(UserId, userDb, MasterDb);
        var service = CreateService(store);

        var response = await service.GetUserProfile(
            new GetUserProfileRequest { PlayerId = 12345L },
            ContextFor(UserId));

        Assert.Equal(99, response.FavoriteCostumeId);
    }

    [Fact]
    public async Task GetUserProfile_ReturnsMessageFromProfile()
    {
        var userDb = CreateUserDb();
        userDb.EntityIUser.Add(new EntityIUser { UserId = UserId, PlayerId = 12345L });
        userDb.EntityIUserProfile.Add(new EntityIUserProfile { UserId = UserId, Message = "Hello!" });
        var store = CreateStore(UserId, userDb, MasterDb);
        var service = CreateService(store);

        var response = await service.GetUserProfile(
            new GetUserProfileRequest { PlayerId = 12345L },
            ContextFor(UserId));

        Assert.Equal("Hello!", response.Message);
    }

    [Fact]
    public async Task GetUserProfile_ReturnsPopulatedLatestUsedDeck()
    {
        var userDb = CreateUserDb();
        userDb.EntityIUser.Add(new EntityIUser { UserId = UserId, PlayerId = 12345L });
        userDb.EntityIUserDeck.Add(new EntityIUserDeck
        {
            UserId = UserId,
            DeckType = DeckType.QUEST,
            UserDeckNumber = 1,
            UserDeckCharacterUuid01 = "dc-uuid"
        });
        userDb.EntityIUserDeckCharacter.Add(new EntityIUserDeckCharacter
        {
            UserId = UserId,
            UserDeckCharacterUuid = "dc-uuid",
            Power = 5000,
            UserCostumeUuid = "c-uuid",
            MainUserWeaponUuid = "w-uuid"
        });
        userDb.EntityIUserCostume.Add(new EntityIUserCostume
        {
            UserId = UserId,
            UserCostumeUuid = "c-uuid",
            CostumeId = 200100
        });
        userDb.EntityIUserWeapon.Add(new EntityIUserWeapon
        {
            UserId = UserId,
            UserWeaponUuid = "w-uuid",
            WeaponId = 300100,
            Level = 15
        });
        var store = CreateStore(UserId, userDb, MasterDb);
        var service = CreateService(store);

        var response = await service.GetUserProfile(
            new GetUserProfileRequest { PlayerId = 12345L },
            ContextFor(UserId));

        Assert.Equal(5000, response.LatestUsedDeck.Power);
        Assert.Single(response.LatestUsedDeck.DeckCharacter);
        Assert.Equal(200100, response.LatestUsedDeck.DeckCharacter[0].CostumeId);
        Assert.Equal(300100, response.LatestUsedDeck.DeckCharacter[0].MainWeaponId);
        Assert.Equal(15, response.LatestUsedDeck.DeckCharacter[0].MainWeaponLevel);
    }

    [Fact]
    public async Task GetUserProfile_ReturnsMaxSeasonRankFromPvpResults()
    {
        var userDb = CreateUserDb();
        userDb.EntityIUser.Add(new EntityIUser { UserId = UserId, PlayerId = 12345L });
        userDb.EntityIUserPvpWeeklyResult.Add(new EntityIUserPvpWeeklyResult
        {
            UserId = UserId,
            PvpSeasonId = 1,
            PvpWeeklyVersion = 1,
            FinalRank = 50
        });
        userDb.EntityIUserPvpWeeklyResult.Add(new EntityIUserPvpWeeklyResult
        {
            UserId = UserId,
            PvpSeasonId = 1,
            PvpWeeklyVersion = 2,
            FinalRank = 30
        });
        var store = CreateStore(UserId, userDb, MasterDb);
        var service = CreateService(store);

        var response = await service.GetUserProfile(
            new GetUserProfileRequest { PlayerId = 12345L },
            ContextFor(UserId));

        Assert.Equal(30, response.PvpInfo.MaxSeasonRank);
    }

    [Fact]
    public async Task GetUserProfile_UnknownPlayerId_ReturnsEmptyResponse()
    {
        var userDb = CreateUserDb();
        var store = CreateStore(UserId, userDb, MasterDb);
        var service = CreateService(store);

        var response = await service.GetUserProfile(
            new GetUserProfileRequest { PlayerId = 99999L },
            ContextFor(UserId));

        Assert.Equal(string.Empty, response.Name);
        Assert.Equal(0, response.Level);
    }

    [Fact]
    public async Task GetAndroidArgs_ReturnsExpectedApiKeyAndNonce()
    {
        var store = CreateStore(UserId, CreateUserDb(), MasterDb);
        var service = CreateService(store);

        var response = await service.GetAndroidArgs(new GetAndroidArgsRequest(), ContextFor(UserId));

        Assert.Equal(UserService.AndroidApiKey, response.ApiKey);
        Assert.Equal(UserService.AndroidNonce, response.Nonce);
    }

    [Fact]
    public async Task GameStart_SetsGameStartDatetimeOnEntityIUser()
    {
        var userDb = CreateUserDb();
        userDb.EntityIUser.Add(new EntityIUser { UserId = UserId });
        var store = CreateStore(UserId, userDb, MasterDb);
        var service = CreateService(store);

        await service.GameStart(new Empty(), ContextFor(UserId));

        Assert.True(userDb.EntityIUser[0].GameStartDatetime > 0);
    }

    [Fact]
    public async Task GameStart_CreatesEntityIUserGemIfNoneExists()
    {
        var userDb = CreateUserDb();
        userDb.EntityIUser.Add(new EntityIUser { UserId = UserId });
        var store = CreateStore(UserId, userDb, MasterDb);
        var service = CreateService(store);

        await service.GameStart(new Empty(), ContextFor(UserId));

        Assert.Single(userDb.EntityIUserGem);
        Assert.Equal(0, userDb.EntityIUserGem[0].PaidGem);
        Assert.Equal(0, userDb.EntityIUserGem[0].FreeGem);
    }

    [Fact]
    public async Task GameStart_DoesNotCreateDuplicateEntityIUserGem()
    {
        var userDb = CreateUserDb();
        userDb.EntityIUser.Add(new EntityIUser { UserId = UserId });
        userDb.EntityIUserGem.Add(new EntityIUserGem { UserId = UserId, PaidGem = 100, FreeGem = 50 });
        var store = CreateStore(UserId, userDb, MasterDb);
        var service = CreateService(store);

        await service.GameStart(new Empty(), ContextFor(UserId));

        Assert.Single(userDb.EntityIUserGem);
        Assert.Equal(100, userDb.EntityIUserGem[0].PaidGem);
    }
}
