using MariesWonderland.Accounts;
using MariesWonderland.Configuration;
using MariesWonderland.Data;
using MariesWonderland.Helpers;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using Microsoft.Extensions.Options;

namespace MariesWonderland.Web.Services;

/// <summary>One account in the admin user list.</summary>
public sealed record UserRow(long UserId, long PlayerId, string Name, int Level, int MaxDeckPower, DateTime? LastSeen, AccountOrigin Origin,
    string Note, bool Disabled, bool Banned, bool HasCode, DateTime? CreatedAt);

/// <summary>What the admin user page shows about one account.</summary>
public sealed record UserDetail(
    long UserId, long PlayerId, string Name, string Message, int Level, long Exp,
    int FreeGems, int PaidGems, long Gold,
    int Costumes, int Weapons, int Companions, int Memoirs, int Materials, int UnclaimedGifts,
    DateTime RegisteredAt, DateTime? LastLogin, int TutorialPhase,
    IReadOnlyList<string> Devices, IReadOnlyList<UserSession> Sessions, AccountRecord? Account);

/// <summary>An owned item for the inventory tables.</summary>
public sealed record OwnedItem(PossessionType Type, int Id, string Name, long Count, string Detail);

/// <summary>
/// Administrator operations on accounts. Everything that touches a save takes the user's lock and writes the changes
/// like a request does. Direct edits sign the player out first, because the client only learns about changes from
/// call responses: signed out, it returns to the title and reloads everything.
/// </summary>
public sealed class UserAdmin(
    UserDataStore store,
    AccountStore accounts,
    MasterDataHolder masterData,
    ActivityTracker activity,
    GameText text,
    TimeProvider timeProvider,
    IOptions<ServerOptions> options)
{
    private DarkMasterMemoryDatabase MasterDb => masterData.Current.Database;
    private long NowMs => timeProvider.GetUtcNow().ToUnixTimeMilliseconds();

    private SavePoints SavePoints => new(store, DataOptions.ResolvePath(options.Value.Data.SavesDirectory));

    public IReadOnlyList<UserRow> List()
    {
        IReadOnlyDictionary<long, AccountRecord> records = accounts.All();
        Dictionary<long, PlayerCard> cards = store.PlayerCards().ToDictionary(c => c.UserId);
        HashSet<long> ids = [.. cards.Keys, .. records.Keys];
        return [.. ids.Select(id =>
        {
            PlayerCard? card = cards.GetValueOrDefault(id);
            AccountRecord? record = records.GetValueOrDefault(id);
            DateTime? lastSeen = activity.LastSeen(id) ?? (card?.LastLoginDatetime > 0 ? DateTimeOffset.FromUnixTimeMilliseconds(card.LastLoginDatetime).UtcDateTime : null);
            return new UserRow(id, card?.PlayerId ?? 0, card?.Name ?? "", card?.Level ?? 0, card?.MaxDeckPower ?? 0, lastSeen,
                record?.Origin ?? AccountOrigin.Device, record?.Note ?? "", record?.DisabledAt != null, record?.BannedAt != null, !string.IsNullOrEmpty(record?.TransferCodeHash),
                record?.CreatedAt);
        }).OrderByDescending(r => r.LastSeen ?? DateTime.MinValue)];
    }

    public UserDetail? Detail(long userId)
    {
        using IDisposable userLock = Lock(userId);
        if (!store.TryGet(userId, out DarkUserMemoryDatabase db))
        {
            return null;
        }

        EntityIUser? user = db.EntityIUser.FirstOrDefault();
        EntityIUserProfile? profile = db.EntityIUserProfile.FirstOrDefault();
        EntityIUserStatus? status = db.EntityIUserStatus.FirstOrDefault();
        EntityIUserGem? gem = db.EntityIUserGem.FirstOrDefault();
        long lastLogin = db.EntityIUserLogin.FirstOrDefault()?.LastLoginDatetime ?? 0;
        return new UserDetail(
            userId, user?.PlayerId ?? 0, profile?.Name ?? "", profile?.Message ?? "", status?.Level ?? 0, status?.Exp ?? 0,
            gem?.FreeGem ?? 0, gem?.PaidGem ?? 0, db.EntityIUserConsumableItem.FirstOrDefault(c => c.ConsumableItemId == 1)?.Count ?? 0,
            db.EntityIUserCostume.Count, db.EntityIUserWeapon.Count, db.EntityIUserCompanion.Count, db.EntityIUserParts.Count, db.EntityIUserMaterial.Count,
            db.EntitySUserGift.Count(g => g.ReceivedDatetime == 0),
            DateTimeOffset.FromUnixTimeMilliseconds(user?.RegisterDatetime ?? 0).UtcDateTime,
            lastLogin > 0 ? DateTimeOffset.FromUnixTimeMilliseconds(lastLogin).UtcDateTime : null,
            db.EntityIUserTutorialProgress.Where(t => t.TutorialType == TutorialType.GAME_START).Select(t => t.ProgressPhase).FirstOrDefault(),
            store.DevicesOf(userId), store.SessionsOf(userId), accounts.Find(userId));
    }

    /// <summary>The account's costumes, weapons, companions, materials and items, by name.</summary>
    public IReadOnlyList<OwnedItem> Inventory(long userId)
    {
        using IDisposable userLock = Lock(userId);
        if (!store.TryGet(userId, out DarkUserMemoryDatabase db))
        {
            return [];
        }

        List<OwnedItem> items = [];
        items.AddRange(db.EntityIUserCostume.Select(c => new OwnedItem(PossessionType.COSTUME, c.CostumeId, text.Costume(MasterDb, c.CostumeId), 1, $"Lv {c.Level}, limit break {c.LimitBreakCount}")));
        items.AddRange(db.EntityIUserWeapon.Select(w => new OwnedItem(PossessionType.WEAPON, w.WeaponId, text.Weapon(MasterDb, w.WeaponId), 1, $"Lv {w.Level}, limit break {w.LimitBreakCount}")));
        items.AddRange(db.EntityIUserCompanion.Select(c => new OwnedItem(PossessionType.COMPANION, c.CompanionId, text.Companion(MasterDb, c.CompanionId), 1, $"Lv {c.Level}")));
        items.AddRange(db.EntityIUserMaterial.Select(m => new OwnedItem(PossessionType.MATERIAL, m.MaterialId, text.Material(m.MaterialId), m.Count, "")));
        items.AddRange(db.EntityIUserConsumableItem.Select(c => new OwnedItem(PossessionType.CONSUMABLE_ITEM, c.ConsumableItemId, text.Consumable(MasterDb, c.ConsumableItemId), c.Count, "")));
        items.AddRange(db.EntityIUserImportantItem.Select(i => new OwnedItem(PossessionType.IMPORTANT_ITEM, i.ImportantItemId, text.ImportantItem(MasterDb, i.ImportantItemId), i.Count, "")));
        return items;
    }

    /// <summary>Gifts waiting in the account's gift box.</summary>
    public IReadOnlyList<(EntitySUserGift Gift, string Name)> Gifts(long userId)
    {
        using IDisposable userLock = Lock(userId);
        if (!store.TryGet(userId, out DarkUserMemoryDatabase db))
        {
            return [];
        }
        return [.. db.EntitySUserGift.Where(g => g.ReceivedDatetime == 0).OrderByDescending(g => g.GrantDatetime)
            .Select(g => (g, text.Possession(MasterDb, (PossessionType)g.PossessionType, g.PossessionId)))];
    }

    /// <summary>Sends a gift to one account; the player claims it in the gift box.</summary>
    public void SendGift(long userId, PossessionType type, int id, int count, string admin)
    {
        Change(userId, db => GiftBoxHelper.Add(db, type, id, count, NowMs));
        accounts.Audit(admin, "user.gift", userId.ToString(), $"{count} × {text.Possession(MasterDb, type, id)}");
    }

    /// <summary>Sends a gift to every account. Returns how many got it.</summary>
    public int SendGiftToAll(PossessionType type, int id, int count, string admin)
    {
        int sent = 0;
        foreach (long userId in store.UserIds())
        {
            Change(userId, db => GiftBoxHelper.Add(db, type, id, count, NowMs));
            sent++;
        }
        accounts.Audit(admin, "all.gift", "all users", $"{count} × {text.Possession(MasterDb, type, id)} to {sent} accounts");
        return sent;
    }

    /// <summary>
    /// Grants straight into the save, as a claimed gift would (costumes and weapons come with their character and
    /// story rows). A save point is taken first, and the player is signed out so the game reloads.
    /// </summary>
    public string GrantNow(long userId, PossessionType type, int id, int count, string admin)
    {
        string savePoint = Backup(userId);
        Change(userId, db => PossessionHelper.Apply(db, type, id, count, MasterDb, NowMs));
        store.RevokeSessions(userId);
        accounts.Audit(admin, "user.grant", userId.ToString(), $"{count} × {text.Possession(MasterDb, type, id)} (save point {savePoint})");
        return savePoint;
    }

    /// <summary>Sets a material, consumable or gem count outright (signs the player out).</summary>
    public string SetCount(long userId, PossessionType type, int id, long count, string admin)
    {
        string savePoint = Backup(userId);
        Change(userId, db =>
        {
            switch (type)
            {
                case PossessionType.MATERIAL:
                    db.EntityIUserMaterial.GetOrCreate(m => m.MaterialId == id, () => new EntityIUserMaterial { UserId = userId, MaterialId = id, FirstAcquisitionDatetime = NowMs }).Count = (int)count;
                    break;
                case PossessionType.CONSUMABLE_ITEM:
                    db.EntityIUserConsumableItem.GetOrCreate(c => c.ConsumableItemId == id, () => new EntityIUserConsumableItem { UserId = userId, ConsumableItemId = id, FirstAcquisitionDatetime = NowMs }).Count = (int)count;
                    break;
                case PossessionType.FREE_GEM:
                    db.EntityIUserGem.GetOrCreate(_ => true, () => new EntityIUserGem { UserId = userId }).FreeGem = (int)count;
                    break;
                case PossessionType.PAID_GEM:
                    db.EntityIUserGem.GetOrCreate(_ => true, () => new EntityIUserGem { UserId = userId }).PaidGem = (int)count;
                    break;
                default:
                    throw new InvalidOperationException($"Counts can't be set for {type}.");
            }
        });
        store.RevokeSessions(userId);
        accounts.Audit(admin, "user.set-count", userId.ToString(), $"{text.Possession(MasterDb, type, id)} = {count} (save point {savePoint})");
        return savePoint;
    }

    public void SetProfile(long userId, string name, string message, string admin)
    {
        Change(userId, db =>
        {
            EntityIUserProfile profile = db.EntityIUserProfile.GetOrCreate(userId);
            profile.Name = name;
            profile.Message = message;
            profile.NameUpdateDatetime = NowMs;
            profile.MessageUpdateDatetime = NowMs;
        });
        store.RevokeSessions(userId);
        accounts.Audit(admin, "user.profile", userId.ToString(), $"name '{name}'");
    }

    /// <summary>Makes an empty account (an invite) and returns its id and transfer code.</summary>
    public (long UserId, string Code) CreateInvite(string note, string admin)
    {
        long userId = store.CreateUser(MasterDb);
        accounts.Ensure(userId, AccountOrigin.Admin, note);
        string code = accounts.IssueTransferCode(userId);
        accounts.Audit(admin, "user.create", userId.ToString(), note);
        return (userId, code);
    }

    public string NewTransferCode(long userId, string admin)
    {
        string code = accounts.IssueTransferCode(userId);
        accounts.Audit(admin, "user.new-code", userId.ToString());
        return code;
    }

    // Sessions are kept: the player's next call gets the game's dialog (AccountStatusInterceptor), which a signed-out
    // call wouldn't.
    public void SetDisabled(long userId, bool disabled, string admin)
    {
        accounts.SetDisabled(userId, disabled);
        accounts.Audit(admin, disabled ? "user.disable" : "user.enable", userId.ToString());
    }

    public void SetBanned(long userId, bool banned, string reason, string admin)
    {
        accounts.SetBanned(userId, banned, reason);
        accounts.Audit(admin, banned ? "user.ban" : "user.unban", userId.ToString(), banned ? reason.Trim() : "");
    }

    public void SetNote(long userId, string note, string admin)
    {
        accounts.Update(userId, r => r.Note = note);
        accounts.Audit(admin, "user.note", userId.ToString(), note);
    }

    public int SignOut(long userId, string admin)
    {
        int count = store.RevokeSessions(userId);
        accounts.Audit(admin, "user.sign-out", userId.ToString());
        return count;
    }

    public int RemoveDevices(long userId, string admin)
    {
        store.RevokeSessions(userId);
        int count = store.RemoveDevices(userId);
        accounts.Audit(admin, "user.remove-devices", userId.ToString(), $"{count} devices");
        return count;
    }

    public string? Export(long userId) => store.ExportUser(userId);

    public IReadOnlyList<SavePointInfo> SavePointsOf(long userId)
        => [.. SavePoints.List().Where(s => s.Name.StartsWith($"admin-{userId}-", StringComparison.Ordinal) || s.Name.StartsWith($"user-{userId}", StringComparison.Ordinal))];

    /// <summary>Puts a save point back (signing the player out) and returns the user it restored.</summary>
    public long RestoreSavePoint(string name, string admin)
    {
        long userId = SavePoints.Load(name);
        store.RevokeSessions(userId);
        accounts.Audit(admin, "user.restore", userId.ToString(), name);
        return userId;
    }

    /// <summary>Saves the account as a save point named <c>admin-&lt;user&gt;-&lt;time&gt;</c>.</summary>
    public string Backup(long userId)
    {
        string name = $"admin-{userId}-{timeProvider.GetUtcNow():yyyyMMddHHmmss}";
        SavePoints.Save(name, userId);
        return name;
    }

    /// <summary>Changes the user's data under their lock and writes it like a request would.</summary>
    private void Change(long userId, Action<DarkUserMemoryDatabase> change)
    {
        using IDisposable userLock = Lock(userId);
        if (!store.TryGet(userId, out DarkUserMemoryDatabase db))
        {
            throw new InvalidOperationException($"There is no user {userId}.");
        }

        Dictionary<string, TableSnapshot> before = UserDataDiffBuilder.Snapshot(db);
        change(db);
        store.SaveChanges(userId, before, db);
    }

    private IDisposable Lock(long userId)
        => store.Locks.TryAcquire(userId, UserDataStore.LockTimeout) ?? throw new TimeoutException($"User {userId} stayed busy; try again.");
}
