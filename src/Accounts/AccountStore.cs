using MariesWonderland.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace MariesWonderland.Accounts;

/// <summary>Who may make a new account by installing the game.</summary>
public enum RegistrationMode
{
    /// <summary>Any new install gets an account.</summary>
    Open = 0,
    /// <summary>New installs are refused; accounts come from invites (admin-made) or imports, reached by transfer.</summary>
    InviteOnly = 1,
    /// <summary>New installs and transfers are refused.</summary>
    Closed = 2,
}

/// <summary>The result of entering a transfer code on the Bridge transfer page.</summary>
public enum LinkOutcome
{
    Linked,
    TokenUnknown,
    TokenExpired,
    TokenUsed,
    BadCode,
    PasswordRequired,
    BadPassword,
    TooManyAttempts,
    AccountDisabled,
    AccountBanned,
    TransfersClosed,
}

/// <summary>Why an account may not play.</summary>
public enum AccountBlock
{
    None,
    Disabled,
    Banned,
}

public sealed record LinkResult(LinkOutcome Outcome, long UserId = 0, string NewCode = "");

/// <summary>
/// Accounts, transfer codes, Bridge tokens, server settings, administrators and the audit log, in the user database.
/// Every call uses its own context; the tables are small. Linking and finishing transfers run under one lock.
/// </summary>
public sealed class AccountStore(Func<UserDbContext> createContext, TimeProvider timeProvider)
{
    /// <summary>How long a Bridge token stays usable after <c>GetBackupToken</c> issued it.</summary>
    public static readonly TimeSpan TokenLifetime = TimeSpan.FromMinutes(30);

    /// <summary>Wrong codes or passwords allowed on one token.</summary>
    public const int MaxAttemptsPerToken = 10;

    public const string RegistrationModeKey = "RegistrationMode";

    private readonly Lock _linkLock = new();
    private readonly ConcurrentDictionary<string, string> _settings = new();
    private bool _settingsLoaded;

    // Disabled and banned accounts, read once and kept in step by Update: every game call checks it.
    private readonly ConcurrentDictionary<long, AccountBlock> _blocks = new();
    private bool _blocksLoaded;
    private readonly Lock _blocksLock = new();

    private DateTime Now => timeProvider.GetUtcNow().UtcDateTime;

    // ---- Settings ----

    public string? GetSetting(string key)
    {
        LoadSettings();
        return _settings.GetValueOrDefault(key);
    }

    public void SetSetting(string key, string value)
    {
        using UserDbContext db = createContext();
        ServerSetting? row = db.ServerSettings.AsTracking().FirstOrDefault(s => s.Key == key);
        if (row == null)
        {
            db.ServerSettings.Add(new ServerSetting { Key = key, Value = value });
        }
        else
        {
            row.Value = value;
        }
        db.SaveChanges();
        _settings[key] = value;
    }

    private void LoadSettings()
    {
        if (_settingsLoaded)
        {
            return;
        }

        using UserDbContext db = createContext();
        foreach (ServerSetting setting in db.ServerSettings.AsNoTracking())
        {
            _settings[setting.Key] = setting.Value;
        }
        _settingsLoaded = true;
    }

    public RegistrationMode RegistrationMode
    {
        get => Enum.TryParse(GetSetting(RegistrationModeKey), out RegistrationMode mode) ? mode : RegistrationMode.Open;
        set => SetSetting(RegistrationModeKey, value.ToString());
    }

    // ---- Accounts ----

    /// <summary>The account's record, or null when none was made yet (accounts from before this table).</summary>
    public AccountRecord? Find(long userId)
    {
        using UserDbContext db = createContext();
        return db.Accounts.AsNoTracking().FirstOrDefault(a => a.UserId == userId);
    }

    public IReadOnlyDictionary<long, AccountRecord> All()
    {
        using UserDbContext db = createContext();
        return db.Accounts.AsNoTracking().ToDictionary(a => a.UserId);
    }

    /// <summary>Makes the account's record if it has none yet.</summary>
    public AccountRecord Ensure(long userId, AccountOrigin origin = AccountOrigin.Device, string note = "")
    {
        using UserDbContext db = createContext();
        AccountRecord? record = db.Accounts.AsNoTracking().FirstOrDefault(a => a.UserId == userId);
        if (record != null)
        {
            return record;
        }

        record = new AccountRecord { UserId = userId, Origin = origin, CreatedAt = Now, Note = note };
        db.Accounts.Add(record);
        db.SaveChanges();
        return record;
    }

    /// <summary>Changes one account's record (made first if missing).</summary>
    public AccountRecord Update(long userId, Action<AccountRecord> change)
    {
        Ensure(userId);
        using UserDbContext db = createContext();
        AccountRecord record = db.Accounts.AsTracking().First(a => a.UserId == userId);
        change(record);
        db.SaveChanges();
        TrackBlock(record);
        return record;
    }

    /// <summary>Issues a new transfer code for the account (the old one stops working) and returns it, unformatted.</summary>
    public string IssueTransferCode(long userId)
    {
        string code;
        string hash;
        do
        {
            code = TransferCode.New();
            hash = TransferCode.Hash(code);
        }
        while (FindByCodeHash(hash) != null);

        Update(userId, r =>
        {
            r.TransferCodeHash = hash;
            r.TransferCodeIssuedAt = Now;
        });
        return code;
    }

    /// <summary>The account a typed transfer code belongs to, if any.</summary>
    public AccountRecord? FindByCode(string? typedCode)
    {
        string? code = TransferCode.Normalize(typedCode);
        return code == null ? null : FindByCodeHash(TransferCode.Hash(code));
    }

    private AccountRecord? FindByCodeHash(string hash)
    {
        using UserDbContext db = createContext();
        return db.Accounts.AsNoTracking().FirstOrDefault(a => a.TransferCodeHash == hash);
    }

    /// <summary>Sets the transfer password, or clears it with an empty one.</summary>
    public void SetPassword(long userId, string? password)
        => Update(userId, r => r.PasswordHash = string.IsNullOrEmpty(password) ? "" : PasswordHashing.Hash(password));

    public void SetDisabled(long userId, bool disabled)
        => Update(userId, r => r.DisabledAt = disabled ? Now : null);

    public bool IsDisabled(long userId) => BlockOf(userId) == AccountBlock.Disabled;

    /// <summary>Bans the account (with a reason for administrators) or lifts the ban.</summary>
    public void SetBanned(long userId, bool banned, string reason = "")
        => Update(userId, r =>
        {
            r.BannedAt = banned ? Now : null;
            r.BanReason = banned ? reason.Trim() : "";
        });

    public bool IsBanned(long userId) => BlockOf(userId) == AccountBlock.Banned;

    /// <summary>Whether the account may play; a ban wins over disabled. Kept in memory, so cheap on every call.</summary>
    public AccountBlock BlockOf(long userId)
    {
        LoadBlocks();
        return _blocks.GetValueOrDefault(userId, AccountBlock.None);
    }

    private static AccountBlock BlockOf(AccountRecord record)
        => record.BannedAt != null ? AccountBlock.Banned
            : record.DisabledAt != null ? AccountBlock.Disabled
            : AccountBlock.None;

    private void TrackBlock(AccountRecord record)
    {
        LoadBlocks();
        AccountBlock block = BlockOf(record);
        if (block == AccountBlock.None)
        {
            _blocks.TryRemove(record.UserId, out _);
        }
        else
        {
            _blocks[record.UserId] = block;
        }
    }

    private void LoadBlocks()
    {
        if (_blocksLoaded)
        {
            return;
        }

        lock (_blocksLock)
        {
            if (_blocksLoaded)
            {
                return;
            }

            using UserDbContext db = createContext();
            foreach (AccountRecord record in db.Accounts.AsNoTracking().Where(a => a.DisabledAt != null || a.BannedAt != null))
            {
                _blocks[record.UserId] = BlockOf(record);
            }
            _blocksLoaded = true;
        }
    }

    /// <summary>Writes when these users were last seen (batched by <see cref="ActivityTracker"/>).</summary>
    public void SaveLastSeen(IReadOnlyDictionary<long, DateTime> lastSeen)
    {
        if (lastSeen.Count == 0)
        {
            return;
        }

        using UserDbContext db = createContext();
        List<long> ids = [.. lastSeen.Keys];
        Dictionary<long, AccountRecord> records = db.Accounts.AsTracking().Where(a => ids.Contains(a.UserId)).ToDictionary(a => a.UserId);
        foreach ((long userId, DateTime at) in lastSeen)
        {
            if (!records.TryGetValue(userId, out AccountRecord? record))
            {
                record = new AccountRecord { UserId = userId, Origin = AccountOrigin.Device, CreatedAt = at };
                db.Accounts.Add(record);
            }
            record.LastSeenAt = at;
        }
        db.SaveChanges();
    }

    // ---- Bridge tokens ----

    /// <summary>A token for a new device waiting to be linked to an account (title screen → Data Transfer).</summary>
    public string MintTransferToken(string deviceUuid) => Mint(BridgeTokenKind.Transfer, deviceUuid, null);

    /// <summary>A token for a signed-in device's Data Linkage page.</summary>
    public string MintRegisterToken(string deviceUuid, long userId) => Mint(BridgeTokenKind.Register, deviceUuid, userId);

    private string Mint(BridgeTokenKind kind, string deviceUuid, long? userId)
    {
        DateTime now = Now;
        string token = TransferCode.Random(32);
        using UserDbContext db = createContext();
        db.BridgeTokens.Where(t => t.ExpiresAt < now.AddDays(-1)).ExecuteDelete();
        db.BridgeTokens.Add(new BridgeToken
        {
            Token = token,
            Kind = kind,
            DeviceUuid = deviceUuid,
            UserId = userId,
            CreatedAt = now,
            ExpiresAt = now + TokenLifetime,
        });
        db.SaveChanges();
        return token;
    }

    public BridgeToken? FindToken(string? token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return null;
        }

        using UserDbContext db = createContext();
        return db.BridgeTokens.AsNoTracking().FirstOrDefault(t => t.Token == token);
    }

    /// <summary>Whether a token can still be used now.</summary>
    public bool IsLive(BridgeToken token) => token.UsedAt == null && token.ExpiresAt > Now;

    /// <summary>
    /// The transfer page's form: checks the code (and password, when the account has one), then points the token at
    /// the account. The old code is used up and a new one issued; the result carries it, to show the player.
    /// </summary>
    public LinkResult LinkTransfer(string tokenValue, string? typedCode, string? password)
    {
        lock (_linkLock)
        {
            if (RegistrationMode == RegistrationMode.Closed)
            {
                return new LinkResult(LinkOutcome.TransfersClosed);
            }

            using UserDbContext db = createContext();
            BridgeToken? token = db.BridgeTokens.AsTracking().FirstOrDefault(t => t.Token == tokenValue && t.Kind == BridgeTokenKind.Transfer);
            if (token == null)
            {
                return new LinkResult(LinkOutcome.TokenUnknown);
            }
            if (token.UsedAt != null)
            {
                return new LinkResult(LinkOutcome.TokenUsed);
            }
            if (token.ExpiresAt <= Now)
            {
                return new LinkResult(LinkOutcome.TokenExpired);
            }
            if (token.FailedAttempts >= MaxAttemptsPerToken)
            {
                return new LinkResult(LinkOutcome.TooManyAttempts);
            }

            AccountRecord? account = FindByCode(typedCode);
            LinkOutcome? failure = account == null ? LinkOutcome.BadCode
                : account.BannedAt != null ? LinkOutcome.AccountBanned
                : account.DisabledAt != null ? LinkOutcome.AccountDisabled
                : account.PasswordHash.Length > 0 && string.IsNullOrEmpty(password) ? LinkOutcome.PasswordRequired
                : account.PasswordHash.Length > 0 && !PasswordHashing.Verify(password!, account.PasswordHash) ? LinkOutcome.BadPassword
                : null;
            if (failure != null)
            {
                if (failure is LinkOutcome.BadCode or LinkOutcome.BadPassword)
                {
                    token.FailedAttempts++;
                    db.SaveChanges();
                }
                return new LinkResult(failure.Value);
            }

            token.UserId = account!.UserId;
            token.LinkedAt = Now;
            db.SaveChanges();

            string newCode = IssueTransferCode(account.UserId);
            return new LinkResult(LinkOutcome.Linked, account.UserId, newCode);
        }
    }

    /// <summary>
    /// <c>TransferUser</c>: the account a live, linked transfer token of this device points at, using the token up.
    /// Null when the device has none (the player closed the page without entering a code).
    /// </summary>
    public long? CompleteTransfer(string deviceUuid)
    {
        lock (_linkLock)
        {
            DateTime now = Now;
            using UserDbContext db = createContext();
            BridgeToken? token = db.BridgeTokens.AsTracking()
                .Where(t => t.DeviceUuid == deviceUuid && t.Kind == BridgeTokenKind.Transfer && t.UserId != null && t.UsedAt == null && t.ExpiresAt > now)
                .OrderByDescending(t => t.LinkedAt)
                .FirstOrDefault();
            if (token == null)
            {
                return null;
            }

            token.UsedAt = now;
            db.SaveChanges();
            long userId = token.UserId!.Value;
            Update(userId, r => r.LastTransferAt = now);
            return userId;
        }
    }

    // ---- Administrators ----

    public bool AnyAdmin()
    {
        using UserDbContext db = createContext();
        return db.AdminUsers.Any();
    }

    public IReadOnlyList<AdminUser> Admins()
    {
        using UserDbContext db = createContext();
        return [.. db.AdminUsers.AsNoTracking().OrderBy(a => a.Name)];
    }

    /// <summary>Adds an administrator, or sets the password of an existing one.</summary>
    public void SetAdmin(string name, string password)
    {
        using UserDbContext db = createContext();
        AdminUser? admin = db.AdminUsers.AsTracking().FirstOrDefault(a => a.Name == name);
        if (admin == null)
        {
            db.AdminUsers.Add(new AdminUser { Name = name, PasswordHash = PasswordHashing.Hash(password), CreatedAt = Now });
        }
        else
        {
            admin.PasswordHash = PasswordHashing.Hash(password);
        }
        db.SaveChanges();
    }

    public bool RemoveAdmin(string name)
    {
        using UserDbContext db = createContext();
        return db.AdminUsers.Where(a => a.Name == name).ExecuteDelete() > 0;
    }

    /// <summary>Checks a sign-in; stamps the login time when it's right.</summary>
    public bool VerifyAdmin(string name, string password)
    {
        using UserDbContext db = createContext();
        AdminUser? admin = db.AdminUsers.AsTracking().FirstOrDefault(a => a.Name == name);
        if (admin == null || !PasswordHashing.Verify(password, admin.PasswordHash))
        {
            return false;
        }

        admin.LastLoginAt = Now;
        db.SaveChanges();
        return true;
    }

    // ---- Audit log ----

    public void Audit(string admin, string action, string target, string details = "")
    {
        using UserDbContext db = createContext();
        db.AuditLog.Add(new AuditEntry { At = Now, Admin = admin, Action = action, Target = target, Details = details });
        db.SaveChanges();
    }

    public IReadOnlyList<AuditEntry> RecentAudit(int count, string? target = null)
    {
        using UserDbContext db = createContext();
        IQueryable<AuditEntry> query = db.AuditLog.AsNoTracking();
        if (target != null)
        {
            query = query.Where(e => e.Target == target);
        }
        return [.. query.OrderByDescending(e => e.Id).Take(count)];
    }
}
