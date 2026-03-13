namespace MariesWonderland.Data;

public class UserDataStore
{
    private readonly Dictionary<long, DarkUserMemoryDatabase> _users = new();

    public DarkUserMemoryDatabase GetOrCreate(long playerId)
    {
        if (!_users.TryGetValue(playerId, out var db))
        {
            db = new DarkUserMemoryDatabase();
            _users[playerId] = db;
        }
        return db;
    }

    public bool TryGet(long playerId, out DarkUserMemoryDatabase db)
        => _users.TryGetValue(playerId, out db!);

    public void Set(long playerId, DarkUserMemoryDatabase db)
        => _users[playerId] = db;

    public IReadOnlyDictionary<long, DarkUserMemoryDatabase> All => _users;
}
