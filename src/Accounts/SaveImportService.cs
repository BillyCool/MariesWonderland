using MariesWonderland.Data;
using MariesWonderland.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace MariesWonderland.Accounts;

/// <summary>What the uploader sees when checking a ticket.</summary>
public sealed record ImportStatusView(SaveImportStatus Status, string PlayerName, string Reason, string? TransferCode, bool CodeShownBefore);

/// <summary>
/// The save import queue: uploads wait for an administrator, who approves (a new account, with a transfer code for the
/// uploader) or rejects them. Administrators can also import a save straight away.
/// </summary>
public sealed class SaveImportService(UserDataStore store, AccountStore accounts, Func<UserDbContext> createContext, TimeProvider timeProvider)
{
    private static readonly JsonSerializerOptions SaveJson = new();

    /// <summary>Queues a parsed save. Returns the ticket the uploader checks the result with (shown once).</summary>
    public string Submit(ParsedSave parsed, string fileName, string submitter)
    {
        string ticket = TransferCode.New();
        using UserDbContext db = createContext();
        db.SaveImports.Add(new SaveImportRequest
        {
            CreatedAt = timeProvider.GetUtcNow().UtcDateTime,
            TicketHash = TransferCode.Hash(ticket),
            Status = SaveImportStatus.Pending,
            FileName = fileName,
            Submitter = submitter,
            SaveJson = JsonSerializer.Serialize(parsed.Save, SaveJson),
            PlayerName = parsed.PlayerName,
            Level = parsed.Level,
            OriginalPlayerId = parsed.PlayerId,
            RowCount = parsed.RowCount,
        });
        db.SaveChanges();
        return ticket;
    }

    public IReadOnlyList<SaveImportRequest> List()
    {
        using UserDbContext db = createContext();
        // The save itself stays out of the list.
        return [.. db.SaveImports.AsNoTracking().OrderByDescending(r => r.Id).Select(r => new SaveImportRequest
        {
            Id = r.Id, CreatedAt = r.CreatedAt, Status = r.Status, FileName = r.FileName, Submitter = r.Submitter,
            PlayerName = r.PlayerName, Level = r.Level, OriginalPlayerId = r.OriginalPlayerId, RowCount = r.RowCount,
            DecidedAt = r.DecidedAt, DecidedBy = r.DecidedBy, Reason = r.Reason, UserId = r.UserId,
        })];
    }

    public int PendingCount()
    {
        using UserDbContext db = createContext();
        return db.SaveImports.Count(r => r.Status == SaveImportStatus.Pending);
    }

    /// <summary>Approves a queued save: it becomes a new account. Returns the new user id and its transfer code.</summary>
    public (long UserId, string Code) Approve(long id, string admin)
    {
        using UserDbContext db = createContext();
        SaveImportRequest request = db.SaveImports.AsTracking().FirstOrDefault(r => r.Id == id)
            ?? throw new InvalidOperationException($"No import {id}.");
        if (request.Status != SaveImportStatus.Pending)
        {
            throw new InvalidOperationException($"Import {id} was already {request.Status.ToString().ToLowerInvariant()}.");
        }

        DarkUserMemoryDatabase save = JsonSerializer.Deserialize<DarkUserMemoryDatabase>(request.SaveJson, SaveJson)
            ?? throw new InvalidDataException("The stored save is empty.");
        (long userId, string code) = CreateAccount(save, admin, $"Import #{id} ({request.FileName})");

        request.Status = SaveImportStatus.Approved;
        request.DecidedAt = timeProvider.GetUtcNow().UtcDateTime;
        request.DecidedBy = admin;
        request.UserId = userId;
        request.PendingTransferCode = code;
        request.SaveJson = "";
        db.SaveChanges();
        accounts.Audit(admin, "import.approve", userId.ToString(), $"Import #{id} {request.PlayerName}");
        return (userId, code);
    }

    public void Reject(long id, string admin, string reason)
    {
        using UserDbContext db = createContext();
        SaveImportRequest request = db.SaveImports.AsTracking().FirstOrDefault(r => r.Id == id)
            ?? throw new InvalidOperationException($"No import {id}.");
        request.Status = SaveImportStatus.Rejected;
        request.DecidedAt = timeProvider.GetUtcNow().UtcDateTime;
        request.DecidedBy = admin;
        request.Reason = reason;
        request.SaveJson = "";
        db.SaveChanges();
        accounts.Audit(admin, "import.reject", $"import #{id}", reason);
    }

    /// <summary>An administrator's import, straight to an account.</summary>
    public (long UserId, string Code) ImportNow(ParsedSave parsed, string admin, string fileName)
    {
        (long userId, string code) = CreateAccount(parsed.Save, admin, $"Imported by {admin} ({fileName})");
        accounts.Audit(admin, "import.direct", userId.ToString(), $"{parsed.PlayerName} from {fileName}");
        return (userId, code);
    }

    /// <summary>The uploader's view of a ticket. The transfer code shows only the first time after approval.</summary>
    public ImportStatusView? Status(string? ticket)
    {
        string? normalized = TransferCode.Normalize(ticket);
        if (normalized == null)
        {
            return null;
        }

        string hash = TransferCode.Hash(normalized);
        using UserDbContext db = createContext();
        SaveImportRequest? request = db.SaveImports.AsTracking().FirstOrDefault(r => r.TicketHash == hash);
        if (request == null)
        {
            return null;
        }

        string? code = request.PendingTransferCode.Length > 0 ? request.PendingTransferCode : null;
        if (code != null)
        {
            request.PendingTransferCode = "";
            db.SaveChanges();
        }
        return new ImportStatusView(request.Status, request.PlayerName, request.Reason, code, request.Status == SaveImportStatus.Approved && code == null);
    }

    private (long UserId, string Code) CreateAccount(DarkUserMemoryDatabase save, string admin, string note)
    {
        long userId = store.ImportUser(save);
        accounts.Ensure(userId, AccountOrigin.Import, note);
        string code = accounts.IssueTransferCode(userId);
        return (userId, code);
    }
}
