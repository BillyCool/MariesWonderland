using System.Text.RegularExpressions;

namespace MariesWonderland.Data;

/// <summary>
/// Dev tool behind <c>/debug/save</c>, <c>/debug/load</c> and <c>/debug/saves</c>: named save points of one user's
/// data, one JSON file each (<c>{name}.json</c> in <see cref="Directory"/>). Save before testing something, load to
/// put the account back and test it again. Loading replaces only that user; devices and sessions stay, so the
/// client stays signed in (it shows the new data after returning to the title screen).
/// </summary>
public sealed partial class SavePoints(UserDataStore store, string directory)
{
    /// <summary>The save point used when no name is given.</summary>
    public const string DefaultName = "default";

    public string Directory { get; } = directory;

    /// <summary>
    /// Saves a user under <paramref name="name"/>, replacing an earlier save point of that name. Without a
    /// <paramref name="userId"/>, the only user on the server is saved. Returns the user saved.
    /// </summary>
    /// <exception cref="ArgumentException">Bad name, no such user, or no user given while there are several.</exception>
    public long Save(string? name, long? userId)
    {
        string path = PathOf(name);
        long user = userId ?? OnlyUser();
        string json = store.ExportUser(user) ?? throw new ArgumentException($"There is no user {user}.");

        System.IO.Directory.CreateDirectory(Directory);
        File.WriteAllText(path, json);
        return user;
    }

    /// <summary>Puts back the user saved under <paramref name="name"/>. Returns that user.</summary>
    /// <exception cref="ArgumentException">Bad name or no such save point.</exception>
    public long Load(string? name)
    {
        string path = PathOf(name);
        if (!File.Exists(path))
        {
            throw new ArgumentException($"There is no save point '{NameOrDefault(name)}'.");
        }

        return store.RestoreUser(File.ReadAllText(path));
    }

    /// <summary>The save points, newest first.</summary>
    public IReadOnlyList<SavePointInfo> List()
    {
        if (!System.IO.Directory.Exists(Directory))
        {
            return [];
        }

        return [.. new DirectoryInfo(Directory).GetFiles("*.json")
            .OrderByDescending(f => f.LastWriteTimeUtc)
            .Select(f => new SavePointInfo(Path.GetFileNameWithoutExtension(f.Name), f.LastWriteTimeUtc, f.Length))];
    }

    private long OnlyUser()
    {
        IReadOnlyList<long> users = store.UserIds();
        return users.Count switch
        {
            1 => users[0],
            0 => throw new ArgumentException("There are no users to save."),
            _ => throw new ArgumentException($"There are {users.Count} users; pick one with ?user= ({string.Join(", ", users)})."),
        };
    }

    private string PathOf(string? name)
    {
        string checkedName = NameOrDefault(name);
        if (!ValidName().IsMatch(checkedName))
        {
            throw new ArgumentException("A save point name may only use letters, digits, '-' and '_' (up to 64).");
        }
        return Path.Combine(Directory, $"{checkedName}.json");
    }

    private static string NameOrDefault(string? name) => string.IsNullOrWhiteSpace(name) ? DefaultName : name;

    [GeneratedRegex("^[A-Za-z0-9_-]{1,64}$")]
    private static partial Regex ValidName();
}

/// <summary>A save point: its name, when it was made and its file size.</summary>
public sealed record SavePointInfo(string Name, DateTime SavedAt, long Bytes);
