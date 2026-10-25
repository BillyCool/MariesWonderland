using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Proto.Data;
using System.Text.Json;

namespace MariesWonderland.Tests.Data;

public class UserDataDiffBuilderTests
{
    private const long UserId = 1L;

    private static DarkUserMemoryDatabase CreateDbWithWeapons(params string[] uuids)
    {
        DarkUserMemoryDatabase db = new() { UserId = UserId };
        foreach (string uuid in uuids)
        {
            db.EntityIUserWeapon.Add(new EntityIUserWeapon { UserId = UserId, UserWeaponUuid = uuid, WeaponId = 100, Level = 1 });
        }
        return db;
    }

    private static List<JsonElement> ParseRows(string json) => [.. JsonDocument.Parse(json).RootElement.EnumerateArray()];

    [Fact]
    public void Delta_Unchanged_IsEmpty()
    {
        DarkUserMemoryDatabase db = CreateDbWithWeapons("a", "b");
        Dictionary<string, TableSnapshot> before = UserDataDiffBuilder.Snapshot(db);

        Assert.Empty(UserDataDiffBuilder.Delta(before, db));
    }

    [Fact]
    public void Delta_RowRemoved_ReportsOnlyKeyFieldsOfDeletedRow()
    {
        DarkUserMemoryDatabase db = CreateDbWithWeapons("a", "b");
        Dictionary<string, TableSnapshot> before = UserDataDiffBuilder.Snapshot(db);

        db.EntityIUserWeapon.RemoveAll(w => w.UserWeaponUuid == "a");

        DiffData diff = UserDataDiffBuilder.Delta(before, db)["IUserWeapon"];
        JsonElement deleted = Assert.Single(ParseRows(diff.DeleteKeysJson));
        Assert.Equal(["userId", "userWeaponUuid"], deleted.EnumerateObject().Select(p => p.Name));
        Assert.Equal("a", deleted.GetProperty("userWeaponUuid").GetString());
        Assert.Equal(UserId, deleted.GetProperty("userId").GetInt64());
    }

    [Fact]
    public void Delta_LastRowRemoved_ReportsDeleteForEmptiedTable()
    {
        DarkUserMemoryDatabase db = CreateDbWithWeapons("a");
        Dictionary<string, TableSnapshot> before = UserDataDiffBuilder.Snapshot(db);

        db.EntityIUserWeapon.Clear();

        DiffData diff = UserDataDiffBuilder.Delta(before, db)["IUserWeapon"];
        Assert.Single(ParseRows(diff.DeleteKeysJson));
        Assert.Equal("[]", diff.UpdateRecordsJson);
    }

    [Fact]
    public void Delta_RowUpdatedOrAdded_SendsOnlyThoseRows()
    {
        DarkUserMemoryDatabase db = CreateDbWithWeapons("a", "b", "c");
        Dictionary<string, TableSnapshot> before = UserDataDiffBuilder.Snapshot(db);

        db.EntityIUserWeapon[0].Level = 5;
        db.EntityIUserWeapon.Add(new EntityIUserWeapon { UserId = UserId, UserWeaponUuid = "d", WeaponId = 200 });

        DiffData diff = UserDataDiffBuilder.Delta(before, db)["IUserWeapon"];
        Assert.Equal(["a", "d"], ParseRows(diff.UpdateRecordsJson).Select(row => row.GetProperty("userWeaponUuid").GetString()));
        Assert.Equal(5, ParseRows(diff.UpdateRecordsJson)[0].GetProperty("level").GetInt32());
        Assert.Equal("[]", diff.DeleteKeysJson);
    }

    [Fact]
    public void Delta_RowRemoved_DoesNotResendRemainingRows()
    {
        DarkUserMemoryDatabase db = CreateDbWithWeapons("a", "b");
        Dictionary<string, TableSnapshot> before = UserDataDiffBuilder.Snapshot(db);

        db.EntityIUserWeapon.RemoveAll(w => w.UserWeaponUuid == "a");

        Assert.Equal("[]", UserDataDiffBuilder.Delta(before, db)["IUserWeapon"].UpdateRecordsJson);
    }

    [Fact]
    public void Delta_RowsOnlyReordered_IsEmpty()
    {
        DarkUserMemoryDatabase db = CreateDbWithWeapons("a", "b");
        Dictionary<string, TableSnapshot> before = UserDataDiffBuilder.Snapshot(db);

        db.EntityIUserWeapon.Reverse();

        Assert.Empty(UserDataDiffBuilder.Delta(before, db));
    }

    [Fact]
    public void Delta_EmptyBaseline_SendsFullState()
    {
        DarkUserMemoryDatabase db = CreateDbWithWeapons("a", "b");
        db.EntityIUserStatus.Add(new EntityIUserStatus { UserId = UserId, Level = 1 });

        Dictionary<string, DiffData> diff = UserDataDiffBuilder.Delta([], db);

        Assert.Equal(2, ParseRows(diff["IUserWeapon"].UpdateRecordsJson).Count);
        Assert.Single(ParseRows(diff["IUserStatus"].UpdateRecordsJson));
        Assert.Equal(UserDataDiffBuilder.SerializeTable(db, "IUserWeapon"), diff["IUserWeapon"].UpdateRecordsJson);
    }

    [Fact]
    public void Delta_RowReplacedUnderSameKey_IsUpdateNotDelete()
    {
        DarkUserMemoryDatabase db = CreateDbWithWeapons("a");
        Dictionary<string, TableSnapshot> before = UserDataDiffBuilder.Snapshot(db);

        db.EntityIUserWeapon.Clear();
        db.EntityIUserWeapon.Add(new EntityIUserWeapon { UserId = UserId, UserWeaponUuid = "a", WeaponId = 100, Level = 9 });

        DiffData diff = UserDataDiffBuilder.Delta(before, db)["IUserWeapon"];
        Assert.Equal("[]", diff.DeleteKeysJson);
        Assert.Equal(9, Assert.Single(ParseRows(diff.UpdateRecordsJson)).GetProperty("level").GetInt32());
    }

    [Fact]
    public void Delta_OnlyChangedTablesIncluded()
    {
        DarkUserMemoryDatabase db = CreateDbWithWeapons("a");
        db.EntityIUserStatus.Add(new EntityIUserStatus { UserId = UserId, Level = 1 });
        Dictionary<string, TableSnapshot> before = UserDataDiffBuilder.Snapshot(db);

        db.EntityIUserStatus[0].Level = 2;

        Assert.Equal(["IUserStatus"], UserDataDiffBuilder.Delta(before, db).Keys);
    }
}
