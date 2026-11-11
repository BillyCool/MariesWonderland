namespace MariesWonderland.Accounts;

/// <summary>How an account came to exist.</summary>
public enum AccountOrigin
{
    /// <summary>A fresh install registered it (<c>RegisterUser</c> or <c>Auth</c> on a new device).</summary>
    Device = 0,
    /// <summary>An administrator created it, usually as an invite.</summary>
    Admin = 1,
    /// <summary>A save import made it.</summary>
    Import = 2,
    /// <summary>The dev-mode transfer of the seed save made it.</summary>
    Seed = 3,
}

/// <summary>Server-side facts about an account that aren't part of the save: transfer code, password, origin, state.</summary>
public sealed class AccountRecord
{
    public long UserId { get; set; }
    public AccountOrigin Origin { get; set; }
    public DateTime CreatedAt { get; set; }

    /// <summary>Free text, e.g. who an invite is for.</summary>
    public string Note { get; set; } = "";

    /// <summary>Hash of the current transfer code (<see cref="TransferCode.Hash"/>); empty when none was issued yet.</summary>
    public string TransferCodeHash { get; set; } = "";
    public DateTime? TransferCodeIssuedAt { get; set; }

    /// <summary>Hash of the optional transfer password (<see cref="PasswordHashing"/>); empty when none is set.</summary>
    public string PasswordHash { get; set; } = "";

    public DateTime? LastTransferAt { get; set; }
    public DateTime? LastSeenAt { get; set; }

    /// <summary>Set when the account is disabled: it can't sign in or be transferred.</summary>
    public DateTime? DisabledAt { get; set; }

    /// <summary>
    /// Set when the account is banned: like disabled, but the game shows its ban dialog ("unauthorized play data, use of
    /// this app is suspended").
    /// </summary>
    public DateTime? BannedAt { get; set; }

    /// <summary>Why it was banned, for administrators only (the player sees the game's fixed text).</summary>
    public string BanReason { get; set; } = "";
}

public enum BridgeTokenKind
{
    /// <summary>Issued to a signed-in device (menu → Data Linkage): the page shows that account's transfer code.</summary>
    Register = 0,
    /// <summary>Issued to a new device on the title screen's Data Transfer: the page links it to an account.</summary>
    Transfer = 1,
}

/// <summary>
/// The token the client puts in the Bridge page URL (from <c>GetBackupToken</c>). A transfer token starts with only
/// the waiting device; the page fills in the account, and <c>TransferUser</c> then uses it up.
/// </summary>
public sealed class BridgeToken
{
    public string Token { get; set; } = "";
    public BridgeTokenKind Kind { get; set; }
    public string DeviceUuid { get; set; } = "";
    public long? UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? LinkedAt { get; set; }
    public DateTime? UsedAt { get; set; }
    public int FailedAttempts { get; set; }
}

/// <summary>A server setting changed at runtime (registration mode and the like), overriding configuration.</summary>
public sealed class ServerSetting
{
    public string Key { get; set; } = "";
    public string Value { get; set; } = "";
}

/// <summary>One administrator action, for the audit log.</summary>
public sealed class AuditEntry
{
    public long Id { get; set; }
    public DateTime At { get; set; }
    public string Admin { get; set; } = "";
    public string Action { get; set; } = "";
    public string Target { get; set; } = "";
    public string Details { get; set; } = "";
}

/// <summary>An administrator who can sign in to <c>/admin</c>.</summary>
public sealed class AdminUser
{
    public string Name { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

public enum SaveImportStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
}

/// <summary>An uploaded save waiting for (or past) an administrator's decision.</summary>
public sealed class SaveImportRequest
{
    public long Id { get; set; }
    public DateTime CreatedAt { get; set; }

    /// <summary>Random ticket the uploader checks the result with (stored hashed).</summary>
    public string TicketHash { get; set; } = "";

    public SaveImportStatus Status { get; set; }
    public string FileName { get; set; } = "";
    public string Submitter { get; set; } = "";

    /// <summary>The parsed save, as the save-point JSON (<c>DarkUserMemoryDatabase</c>).</summary>
    public string SaveJson { get; set; } = "";

    // Summary shown in the queue.
    public string PlayerName { get; set; } = "";
    public int Level { get; set; }
    public long OriginalPlayerId { get; set; }
    public int RowCount { get; set; }

    public DateTime? DecidedAt { get; set; }
    public string DecidedBy { get; set; } = "";
    public string Reason { get; set; } = "";
    public long? UserId { get; set; }

    /// <summary>The new account's transfer code, kept until the uploader has seen it once, then cleared.</summary>
    public string PendingTransferCode { get; set; } = "";
}

/// <summary>One published master data change: which tables, what changed, and the version it went out as.</summary>
public sealed class MasterDataEdit
{
    public long Id { get; set; }
    public DateTime At { get; set; }
    public string Admin { get; set; } = "";
    public string Summary { get; set; } = "";
    public string Version { get; set; } = "";

    /// <summary>JSON list of row changes (<c>MasterRowChange</c>): table, key, before and after.</summary>
    public string ChangesJson { get; set; } = "";
}
