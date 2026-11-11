using Grpc.Core;
using MariesWonderland.Accounts;
using MariesWonderland.Configuration;
using MariesWonderland.Data;
using MariesWonderland.Missions;
using MariesWonderland.Persistence;
using MariesWonderland.Proto.User;
using MariesWonderland.Tests.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using UserServiceImpl = MariesWonderland.Services.UserService;

namespace MariesWonderland.Tests.Accounts;

public sealed class TransferCodeTests
{
    [Fact]
    public void New_IsTwelveCharacters_AndNormalizesToItself()
    {
        string code = TransferCode.New();

        Assert.Equal(TransferCode.Length, code.Length);
        Assert.Equal(code, TransferCode.Normalize(code));
    }

    [Theory]
    [InlineData("k7qm-2xpa-9fdr", "K7QM2XPA9FDR")]
    [InlineData(" K7QM 2XPA 9FDR ", "K7QM2XPA9FDR")]
    [InlineData("O1IL-0000-0000", "0111" + "00000000")]
    public void Normalize_IsForgivingAboutCaseSpacesAndLookAlikes(string typed, string expected)
        => Assert.Equal(expected, TransferCode.Normalize(typed));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("K7QM-2XPA")]
    [InlineData("K7QM-2XPA-9FDU")]
    public void Normalize_RejectsWhatCantBeACode(string? typed) => Assert.Null(TransferCode.Normalize(typed));

    [Fact]
    public void Format_GroupsInFours() => Assert.Equal("K7QM-2XPA-9FDR", TransferCode.Format("K7QM2XPA9FDR"));

    [Fact]
    public void PasswordHashing_VerifiesOnlyTheRightPassword()
    {
        string hash = PasswordHashing.Hash("correct horse");

        Assert.True(PasswordHashing.Verify("correct horse", hash));
        Assert.False(PasswordHashing.Verify("wrong", hash));
    }
}

/// <summary>
/// The transfer site end to end on a real (temporary) user database: Bridge tokens from GetBackupToken, linking on the
/// page, TransferUser signing the new device in, invites and registration modes.
/// </summary>
public sealed class TransferFlowTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"mw-accounts-{Guid.NewGuid():N}.db");
    private readonly FakeTimeProvider _time = new(new DateTimeOffset(2026, 11, 4, 12, 0, 0, TimeSpan.Zero));
    private readonly Func<UserDbContext> _createContext;
    private readonly EfUserPersistence _persistence;
    private readonly UserDataStore _store;
    private readonly AccountStore _accounts;
    private readonly DarkMasterMemoryDatabase _masterDb = new();

    public TransferFlowTests()
    {
        Microsoft.EntityFrameworkCore.DbContextOptions<SqliteUserDbContext> options = SqliteUserDbContext.Options($"Data Source={_path}");
        _createContext = () => new SqliteUserDbContext(options);
        _persistence = new EfUserPersistence(_createContext);
        _persistence.Migrate();
        _store = new UserDataStore(_time, _persistence, NullLogger<UserDataStore>.Instance);
        _accounts = new AccountStore(_createContext, _time);
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        File.Delete(_path);
    }

    private UserServiceImpl Service(bool devMode = false)
    {
        UserDataSeeder seeder = new(Options.Create(new ServerOptions { Data = new DataOptions { UserDataPath = "Data/UserData" } }));
        return new UserServiceImpl(_store, seeder, _masterDb, new MissionTracker(_masterDb, new MissionCatalog(_masterDb, MissionFilterTable.Empty)), _time,
            _accounts, Options.Create(new DevModeOptions { Enabled = devMode }));
    }

    /// <summary>An account with a device and a session, as a player who has been playing.</summary>
    private async Task<(long UserId, string Session)> PlayingAccount(string uuid)
    {
        AuthUserResponse auth = await Service().Auth(new AuthUserRequest { Uuid = uuid }, FakeServerCallContext.Anonymous());
        return (auth.UserId, auth.SessionKey);
    }

    [Fact]
    public async Task Transfer_ThroughTheSite_MovesTheAccountToTheNewDevice_AndSignsTheOldOneOut()
    {
        (long userId, string oldSession) = await PlayingAccount("old-phone");
        string code = _accounts.IssueTransferCode(userId);

        // Title screen → Data Transfer on the new phone: a token for the new device, then the page.
        GetBackupTokenResponse token = await Service().GetBackupToken(new GetBackupTokenRequest { Uuid = "new-phone" }, FakeServerCallContext.Anonymous());
        Assert.Equal(BridgeTokenKind.Transfer, _accounts.FindToken(token.BackupToken)!.Kind);
        LinkResult link = _accounts.LinkTransfer(token.BackupToken, TransferCode.Format(code).ToLowerInvariant(), null);
        Assert.Equal(LinkOutcome.Linked, link.Outcome);
        Assert.Equal(userId, link.UserId);

        TransferUserResponse transfer = await Service().TransferUser(new TransferUserRequest { Uuid = "new-phone" }, FakeServerCallContext.Anonymous());
        AuthUserResponse auth = await Service().Auth(new AuthUserRequest { Uuid = "new-phone" }, FakeServerCallContext.Anonymous());

        Assert.Equal(userId, transfer.UserId);
        Assert.Equal(userId, auth.UserId);
        Assert.Equal(["new-phone"], _store.DevicesOf(userId));
        Assert.False(_store.TryResolveSession(oldSession, out _));
        // The old code was used up; the page showed the new one.
        Assert.Null(_accounts.FindByCode(code));
        Assert.Equal(userId, _accounts.FindByCode(link.NewCode)!.UserId);
        Assert.NotNull(_accounts.Find(userId)!.LastTransferAt);
    }

    [Fact]
    public async Task TransferUser_WithoutALink_FailsAndChangesNothing()
    {
        RpcException error = await Assert.ThrowsAsync<RpcException>(() => Service().TransferUser(new TransferUserRequest { Uuid = "new-phone" }, FakeServerCallContext.Anonymous()));

        Assert.Equal(StatusCode.FailedPrecondition, error.StatusCode);
        Assert.False(_store.IsKnownDevice("new-phone"));
    }

    [Fact]
    public async Task TransferUser_WithoutALink_InDevMode_StillLoadsTheSeedSave()
    {
        TransferUserResponse transfer = await Service(devMode: true).TransferUser(new TransferUserRequest { Uuid = "dev-phone" }, FakeServerCallContext.Anonymous());

        Assert.True(_store.TryGet(transfer.UserId, out _));
        Assert.Equal(AccountOrigin.Seed, _accounts.Find(transfer.UserId)!.Origin);
    }

    [Fact]
    public async Task ALinkedToken_IsUsedOnce()
    {
        (long userId, _) = await PlayingAccount("old-phone");
        string code = _accounts.IssueTransferCode(userId);
        string token = _accounts.MintTransferToken("new-phone");
        _accounts.LinkTransfer(token, code, null);

        Assert.Equal(userId, _accounts.CompleteTransfer("new-phone"));
        Assert.Null(_accounts.CompleteTransfer("new-phone"));
        Assert.Equal(LinkOutcome.TokenUsed, _accounts.LinkTransfer(token, code, null).Outcome);
    }

    [Fact]
    public async Task GetBackupToken_ForASignedInDevice_GivesADataLinkageToken()
    {
        (long userId, string session) = await PlayingAccount("phone");

        GetBackupTokenResponse response = await Service().GetBackupToken(new GetBackupTokenRequest { Uuid = "phone" }, FakeServerCallContext.Anonymous(session));

        BridgeToken token = _accounts.FindToken(response.BackupToken)!;
        Assert.Equal(BridgeTokenKind.Register, token.Kind);
        Assert.Equal(userId, token.UserId);
    }

    [Fact]
    public async Task Link_WrongCodes_CountAgainstTheToken_UntilItLocks()
    {
        (long userId, _) = await PlayingAccount("old-phone");
        string code = _accounts.IssueTransferCode(userId);
        string token = _accounts.MintTransferToken("new-phone");

        for (int i = 0; i < AccountStore.MaxAttemptsPerToken; i++)
        {
            Assert.Equal(LinkOutcome.BadCode, _accounts.LinkTransfer(token, TransferCode.New(), null).Outcome);
        }

        Assert.Equal(LinkOutcome.TooManyAttempts, _accounts.LinkTransfer(token, code, null).Outcome);
    }

    [Fact]
    public async Task Link_WithAPassword_NeedsIt()
    {
        (long userId, _) = await PlayingAccount("old-phone");
        string code = _accounts.IssueTransferCode(userId);
        _accounts.SetPassword(userId, "hunter22");
        string token = _accounts.MintTransferToken("new-phone");

        Assert.Equal(LinkOutcome.PasswordRequired, _accounts.LinkTransfer(token, code, null).Outcome);
        Assert.Equal(LinkOutcome.BadPassword, _accounts.LinkTransfer(token, code, "nope").Outcome);
        Assert.Equal(LinkOutcome.Linked, _accounts.LinkTransfer(token, code, "hunter22").Outcome);
    }

    [Fact]
    public async Task Link_AfterTheTokenExpires_Fails()
    {
        (long userId, _) = await PlayingAccount("old-phone");
        string code = _accounts.IssueTransferCode(userId);
        string token = _accounts.MintTransferToken("new-phone");

        _time.Advance(AccountStore.TokenLifetime + TimeSpan.FromSeconds(1));

        Assert.Equal(LinkOutcome.TokenExpired, _accounts.LinkTransfer(token, code, null).Outcome);
    }

    [Fact]
    public async Task ADisabledAccount_CantSignInOrBeLinked()
    {
        (long userId, _) = await PlayingAccount("phone");
        string code = _accounts.IssueTransferCode(userId);
        _accounts.SetDisabled(userId, true);

        RpcException error = await Assert.ThrowsAsync<RpcException>(() => Service().Auth(new AuthUserRequest { Uuid = "phone" }, FakeServerCallContext.Anonymous()));
        Assert.Equal(StatusCode.PermissionDenied, error.StatusCode);
        // "Process failed. Returning to title screen."
        Assert.Equal("3", error.Trailers.GetValue("x-apb-screen-transition-type"));
        Assert.Equal("900000", error.Trailers.GetValue("x-apb-message-code"));
        Assert.Equal(LinkOutcome.AccountDisabled, _accounts.LinkTransfer(_accounts.MintTransferToken("new"), code, null).Outcome);
    }

    private static void AssertBanDialog(RpcException error)
    {
        Assert.Equal(StatusCode.PermissionDenied, error.StatusCode);
        Assert.Equal("3", error.Trailers.GetValue("x-apb-screen-transition-type"));
        Assert.Equal("102002", error.Trailers.GetValue("x-apb-message-code"));
        Assert.Equal("102002", error.Trailers.GetValue("x-apb-message-text-id"));
    }

    [Fact]
    public async Task ABannedAccount_GetsTheBanDialog_OnSignIn()
    {
        (long userId, _) = await PlayingAccount("phone");
        _accounts.SetBanned(userId, true, "cheating");

        AssertBanDialog(await Assert.ThrowsAsync<RpcException>(() => Service().Auth(new AuthUserRequest { Uuid = "phone" }, FakeServerCallContext.Anonymous())));
        Assert.Equal("cheating", _accounts.Find(userId)!.BanReason);
    }

    [Fact]
    public async Task ABannedAccount_GetsTheBanDialog_OnItsNextCall_WithItsSessionKept()
    {
        (long userId, string session) = await PlayingAccount("phone");
        _accounts.SetBanned(userId, true);
        AccountStatusInterceptor interceptor = new(_accounts);
        bool handled = false;

        RpcException error = await Assert.ThrowsAsync<RpcException>(() => interceptor.UnaryServerHandler(new AuthUserRequest(), FakeServerCallContext.For(userId, session),
            (_, _) => { handled = true; return Task.FromResult(new AuthUserResponse()); }));

        AssertBanDialog(error);
        Assert.False(handled);
        Assert.True(_store.TryResolveSession(session, out _));
    }

    [Fact]
    public async Task ABan_CantBeTransferredAway_AndLifting_ItLetsThePlayerBack()
    {
        (long userId, _) = await PlayingAccount("phone");
        string code = _accounts.IssueTransferCode(userId);
        _accounts.SetBanned(userId, true);

        Assert.Equal(LinkOutcome.AccountBanned, _accounts.LinkTransfer(_accounts.MintTransferToken("new"), code, null).Outcome);

        _accounts.SetBanned(userId, false);
        Assert.Equal(AccountBlock.None, _accounts.BlockOf(userId));
        Assert.Equal("", _accounts.Find(userId)!.BanReason);
        Assert.Equal(userId, (await Service().Auth(new AuthUserRequest { Uuid = "phone" }, FakeServerCallContext.Anonymous())).UserId);
    }

    [Fact]
    public async Task ABan_IsKnownAfterARestart_AndWinsOverDisabled()
    {
        (long userId, _) = await PlayingAccount("phone");
        _accounts.SetDisabled(userId, true);
        _accounts.SetBanned(userId, true);

        AccountStore restarted = new(_createContext, _time);

        Assert.Equal(AccountBlock.Banned, restarted.BlockOf(userId));
        Assert.Equal(AccountBlock.None, restarted.BlockOf(userId + 1));
    }

    [Fact]
    public async Task InviteOnly_RefusesNewInstalls_ButAnInviteGetsInByTransfer()
    {
        _accounts.RegistrationMode = RegistrationMode.InviteOnly;

        RpcException refused = await Assert.ThrowsAsync<RpcException>(() => Service().RegisterUser(new RegisterUserRequest { Uuid = "stranger" }, FakeServerCallContext.Anonymous()));
        Assert.Equal(StatusCode.PermissionDenied, refused.StatusCode);
        Assert.False(_store.IsKnownDevice("stranger"));

        // The administrator makes an empty account and sends its code.
        long invited = _store.CreateUser(_masterDb);
        _accounts.Ensure(invited, AccountOrigin.Admin, "for Sam");
        string code = _accounts.IssueTransferCode(invited);

        GetBackupTokenResponse token = await Service().GetBackupToken(new GetBackupTokenRequest { Uuid = "sams-phone" }, FakeServerCallContext.Anonymous());
        Assert.Equal(LinkOutcome.Linked, _accounts.LinkTransfer(token.BackupToken, code, null).Outcome);
        await Service().TransferUser(new TransferUserRequest { Uuid = "sams-phone" }, FakeServerCallContext.Anonymous());
        AuthUserResponse auth = await Service().Auth(new AuthUserRequest { Uuid = "sams-phone" }, FakeServerCallContext.Anonymous());

        Assert.Equal(invited, auth.UserId);
        Assert.True(_store.TryGet(invited, out DarkUserMemoryDatabase db));
        Assert.Single(db.EntityIUser);
    }

    [Fact]
    public void RegistrationMode_SurvivesARestart()
    {
        _accounts.RegistrationMode = RegistrationMode.Closed;

        Assert.Equal(RegistrationMode.Closed, new AccountStore(_createContext, _time).RegistrationMode);
    }

    [Fact]
    public async Task Closed_RefusesLinking()
    {
        (long userId, _) = await PlayingAccount("phone");
        string code = _accounts.IssueTransferCode(userId);
        _accounts.RegistrationMode = RegistrationMode.Closed;

        Assert.Equal(LinkOutcome.TransfersClosed, _accounts.LinkTransfer(_accounts.MintTransferToken("new"), code, null).Outcome);
    }

    [Fact]
    public void Admins_VerifyOnlyTheRightPassword()
    {
        Assert.False(_accounts.AnyAdmin());
        _accounts.SetAdmin("admin", "a long password");

        Assert.True(_accounts.AnyAdmin());
        Assert.True(_accounts.VerifyAdmin("admin", "a long password"));
        Assert.False(_accounts.VerifyAdmin("admin", "wrong"));
        Assert.False(_accounts.VerifyAdmin("someone", "a long password"));
    }

    [Fact]
    public void SaveImport_ApprovedSave_BecomesANewAccount_WithItsCodeShownOnce()
    {
        ParsedSave parsed = SaveImporter.ParseFolder("Data/UserData", _masterDb);
        long originalUserId = parsed.Save.UserId;
        SaveImportService imports = new(_store, _accounts, _createContext, _time);

        string ticket = imports.Submit(parsed, "save.zip", "Sam");
        Assert.Equal(SaveImportStatus.Pending, imports.Status(ticket)!.Status);
        long requestId = imports.List().Single().Id;
        (long userId, string code) = imports.Approve(requestId, "admin");

        ImportStatusView first = imports.Status(ticket)!;
        ImportStatusView second = imports.Status(ticket)!;
        Assert.Equal(code, first.TransferCode);
        Assert.Null(second.TransferCode);
        Assert.True(second.CodeShownBefore);

        Assert.NotEqual(originalUserId, userId);
        Assert.True(_store.TryGet(userId, out DarkUserMemoryDatabase imported));
        Assert.All(imported.EntityIUserCostume, c => Assert.Equal(userId, c.UserId));
        Assert.Equal(parsed.PlayerId, imported.EntityIUser.Single().PlayerId);
        Assert.Equal(userId, _accounts.FindByCode(code)!.UserId);
        Assert.Equal(AccountOrigin.Import, _accounts.Find(userId)!.Origin);
    }
}
