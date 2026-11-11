using MariesWonderland.Accounts;
using MariesWonderland.Configuration;
using MariesWonderland.Data;
using MariesWonderland.MasterEditing;
using MariesWonderland.Models.Entities;
using MariesWonderland.Persistence;
using MariesWonderland.Tests.Infrastructure;
using MessagePack;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;

namespace MariesWonderland.Tests.MasterEditing;

public sealed class ArenaSeasonPlannerTests
{
    private const long Day = 86_400_000L;
    private static readonly long Jan2030 = new DateTimeOffset(2030, 1, 1, 8, 0, 0, TimeSpan.Zero).ToUnixTimeMilliseconds();

    [Fact]
    public void Plan_AddsBackToBackSeasons_CopyingTheLastValidSeasonsGroups_WithDowntimes()
    {
        EntityMPvpSeason last = new() { PvpSeasonId = 202100, NameAssetPath = "3", SeasonStartDatetime = Jan2030 - 28 * Day, SeasonEndDatetime = Jan2030 - 1000, PvpSeasonGroupingId = 6, PvpGradeGroupId = 2, PvpSeasonRankRewardRankGroupId = 4 };

        IReadOnlyList<PlannedSeason> plan = ArenaSeasonPlanner.Plan([last], [new EntityMMaintenance { MaintenanceId = 91050 }], 3, 28);

        Assert.Equal([202101, 202102, 202103], plan.Select(p => p.Season.PvpSeasonId));
        Assert.Equal(Jan2030, plan[0].Season.SeasonStartDatetime);
        Assert.Equal(plan[0].Season.SeasonEndDatetime + 1000, plan[1].Season.SeasonStartDatetime);
        Assert.All(plan, p => Assert.Equal((6, 2, 4), (p.Season.PvpSeasonGroupingId, p.Season.PvpGradeGroupId, p.Season.PvpSeasonRankRewardRankGroupId)));
        Assert.Equal([91051, 91052, 91053], plan.Select(p => p.Maintenance.MaintenanceId));
        Assert.All(plan, p => Assert.Equal(p.Season.SeasonStartDatetime + ArenaSeasonPlanner.DowntimeMs, p.Maintenance.EndDatetime));
        // A new year: the names count from 1 again.
        Assert.Equal(["1", "2", "3"], plan.Select(p => p.Season.NameAssetPath));
    }
}

/// <summary>Writing, publishing and undoing master data edits, on a copy of the real master data file.</summary>
public sealed class MasterPublishTests : IDisposable
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), $"mw-master-{Guid.NewGuid():N}");
    private readonly string _dbPath;
    private const string Version = "20240404193219";

    public MasterPublishTests()
    {
        Directory.CreateDirectory(_folder);
        _dbPath = Path.Combine(_folder, "users.db");
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        Directory.Delete(_folder, recursive: true);
    }

    [Fact]
    public void Write_WithNothingReplaced_GivesTheSameFile()
    {
        MasterDatabaseFixture.SkipIfUnavailable();
        byte[] original = File.ReadAllBytes(TestSettings.MasterDataPath);

        byte[] written = MasterDataFile.Parse(original).Write(new Dictionary<string, byte[]>());

        Assert.Equal(original, written);
    }

    [Fact]
    public void EveryTable_SerializesBackToTheSameBytes()
    {
        MasterDatabaseFixture.SkipIfUnavailable();
        MasterDataFile file = MasterDataFile.Read(TestSettings.MasterDataPath);
        DarkMasterMemoryDatabase db = BinaryMasterDataLoader.Load(TestSettings.MasterDataPath);
        MessagePackSerializerOptions lz4 = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4Block);

        List<string> differing = [];
        foreach (MasterTable table in MasterTables.All.Where(t => file.TableNames.Contains(t.FileName)))
        {
            // Compare decompressed contents: the compressor may pick other blocks for the same data.
            byte[] original = MessagePackSerializer.ConvertToJson(file.TableBytes(table.FileName), lz4).Select(c => (byte)c).ToArray();
            byte[] ours = MessagePackSerializer.ConvertToJson(MasterDataFile.Serialize(table, table.Rows(db)), lz4).Select(c => (byte)c).ToArray();
            if (!original.AsSpan().SequenceEqual(ours))
            {
                differing.Add(table.FileName);
            }
        }

        Assert.Empty(differing);
    }

    [Fact]
    public void Publish_WritesTheFile_SwapsItInLive_AndUndoPutsItBack()
    {
        MasterDatabaseFixture.SkipIfUnavailable();
        File.Copy(TestSettings.MasterDataPath, Path.Combine(_folder, $"{Version}.bin.e"));
        FakeTimeProvider time = new(DateTimeOffset.UtcNow);
        Microsoft.EntityFrameworkCore.DbContextOptions<SqliteUserDbContext> options = SqliteUserDbContext.Options($"Data Source={_dbPath}");
        Func<UserDbContext> createContext = () => new SqliteUserDbContext(options);
        new EfUserPersistence(createContext).Migrate();
        AccountStore accounts = new(createContext, time);
        MasterDataHolder holder = new(MasterDataSnapshot.Load(Path.Combine(_folder, $"{Version}.bin.e"), Version, stampFileTime: false));
        MasterDraft draft = new(holder);
        MasterDataPublisher publisher = new(holder, draft, accounts, createContext,
            Options.Create(new ServerOptions { Paths = new PathsOptions { MasterDatabase = _folder }, Data = new DataOptions { LatestMasterDataVersion = Version } }),
            NullLogger<MasterDataPublisher>.Instance);

        MasterTable seasons = MasterTables.Get<EntityMPvpSeason>();
        EntityMPvpSeason first = holder.Current.Database.EntityMPvpSeason.OrderBy(s => s.PvpSeasonId).First();
        long originalEnd = first.SeasonEndDatetime;
        int changed = draft.Modify(seasons, r => ((EntityMPvpSeason)r).PvpSeasonId == first.PvpSeasonId, r => ((EntityMPvpSeason)r).SeasonEndDatetime = originalEnd + 1000);
        Assert.Equal(1, changed);
        Assert.Equal(originalEnd, first.SeasonEndDatetime);
        Assert.Single(draft.Changes());

        string version = publisher.Publish("admin", "test");

        Assert.StartsWith(Version + "_", version);
        Assert.Equal(version, holder.Current.Version);
        Assert.Equal(originalEnd + 1000, holder.Current.Database.EntityMPvpSeason.Find(first.PvpSeasonId)!.SeasonEndDatetime);
        Assert.Empty(draft.EditedTables);
        // The other tables are untouched.
        Assert.Equal(BinaryMasterDataLoader.Load(TestSettings.MasterDataPath).EntityMShop.Count, holder.Current.Database.EntityMShop.Count);

        MasterDataEdit edit = Assert.Single(publisher.History());
        draft.ApplyInverse(MasterDataPublisher.ChangesOf(edit));
        publisher.Publish("admin", "undo");

        Assert.Equal(originalEnd, holder.Current.Database.EntityMPvpSeason.Find(first.PvpSeasonId)!.SeasonEndDatetime);

        // A restart advertises the stamped version again.
        MasterDataHolder restarted = new(MasterDataSnapshot.Load(Path.Combine(_folder, $"{Version}.bin.e"), Version, stampFileTime: false));
        new MasterDataPublisher(restarted, new MasterDraft(restarted), new AccountStore(createContext, time), createContext,
            Options.Create(new ServerOptions { Paths = new PathsOptions { MasterDatabase = _folder }, Data = new DataOptions { LatestMasterDataVersion = Version } }),
            NullLogger<MasterDataPublisher>.Instance).ApplyStartupVersion();
        Assert.StartsWith(Version + "_", restarted.Current.Version);
    }

    [Fact]
    public void Draft_RefusesADuplicateKey()
    {
        MasterDatabaseFixture.SkipIfUnavailable();
        MasterDataHolder holder = new(MasterDataSnapshot.Load(TestSettings.MasterDataPath, Version, stampFileTime: false));
        MasterDraft draft = new(holder);
        MasterTable seasons = MasterTables.Get<EntityMPvpSeason>();
        EntityMPvpSeason existing = holder.Current.Database.EntityMPvpSeason.First();

        Assert.Throws<InvalidOperationException>(() => draft.Add(seasons, [seasons.Copy(existing)]));
    }
}
