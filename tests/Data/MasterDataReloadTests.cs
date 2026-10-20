using MariesWonderland.Data;
using MariesWonderland.Tests.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;

namespace MariesWonderland.Tests.Data;

/// <summary>
/// Verifies the master-data holder, version stamping and the dev-mode file watcher.
/// The watcher tests work on a copy of the master data file in a temp folder.
/// </summary>
public sealed class MasterDataReloadTests(MasterDatabaseFixture fixture) : IDisposable
{
    private const string BaseVersion = "20240404193219";
    private static readonly DateTime EditedAt = new(2025, 1, 2, 3, 4, 5, DateTimeKind.Utc);

    private readonly string _folder = Directory.CreateTempSubdirectory("mw-masterdata-").FullName;

    private string FilePath => Path.Combine(_folder, $"{BaseVersion}.bin.e");

    public void Dispose() => Directory.Delete(_folder, recursive: true);

    [Theory]
    [InlineData("20240404193219", "20240404193219")]
    [InlineData("20240404193219_1735787045000", "20240404193219")]
    public void BaseVersion_StripsFileTimeStamp(string requested, string expected)
    {
        Assert.Equal(expected, MasterDataSnapshot.BaseVersion(requested));
    }

    [Fact]
    public void Load_StampsFileTimeOnlyWhenAsked()
    {
        MasterDatabaseFixture.SkipIfUnavailable();
        CopyMasterData(EditedAt);

        MasterDataSnapshot plain = MasterDataSnapshot.Load(FilePath, BaseVersion, stampFileTime: false);
        MasterDataSnapshot stamped = MasterDataSnapshot.Load(FilePath, BaseVersion, stampFileTime: true);

        Assert.Equal(BaseVersion, plain.Version);
        Assert.Equal($"{BaseVersion}_{new DateTimeOffset(EditedAt).ToUnixTimeMilliseconds()}", stamped.Version);
        Assert.Equal(fixture.GameConfig.ConsumableItemIdForGold, stamped.GameConfig.ConsumableItemIdForGold);
    }

    [Fact]
    public async Task Reloader_SwapsInEditedFile()
    {
        MasterDatabaseFixture.SkipIfUnavailable();
        CopyMasterData(EditedAt);
        MasterDataSnapshot initial = new(fixture.MasterDb, BaseVersion);
        MasterDataHolder holder = new(initial);

        using MasterDataReloader reloader = new(holder, FilePath, BaseVersion, NullLogger<MasterDataReloader>.Instance);
        await reloader.StartAsync(TestContext.Current.CancellationToken);

        DateTime editedAgain = EditedAt.AddHours(1);
        CopyMasterData(editedAgain);
        MasterDataSnapshot reloaded = await WaitForReload(holder, initial);

        await reloader.StopAsync(TestContext.Current.CancellationToken);
        Assert.NotSame(initial.Database, reloaded.Database);
        Assert.Equal($"{BaseVersion}_{new DateTimeOffset(editedAgain).ToUnixTimeMilliseconds()}", reloaded.Version);
    }

    [Fact]
    public async Task Reloader_KeepsOldDataWhenFileIsBad_AndRecovers()
    {
        MasterDatabaseFixture.SkipIfUnavailable();
        CopyMasterData(EditedAt);
        MasterDataSnapshot initial = new(fixture.MasterDb, BaseVersion);
        MasterDataHolder holder = new(initial);

        using MasterDataReloader reloader = new(holder, FilePath, BaseVersion, NullLogger<MasterDataReloader>.Instance);
        await reloader.StartAsync(TestContext.Current.CancellationToken);

        await File.WriteAllBytesAsync(FilePath, [1, 2, 3, 4], TestContext.Current.CancellationToken);
        await Task.Delay(TimeSpan.FromSeconds(2), TestContext.Current.CancellationToken);
        Assert.Same(initial, holder.Current);

        CopyMasterData(EditedAt.AddHours(2));
        MasterDataSnapshot reloaded = await WaitForReload(holder, initial);

        await reloader.StopAsync(TestContext.Current.CancellationToken);
        Assert.NotSame(initial, reloaded);
    }

    private void CopyMasterData(DateTime lastWriteUtc)
    {
        File.Copy(TestSettings.MasterDataPath, FilePath, overwrite: true);
        File.SetLastWriteTimeUtc(FilePath, lastWriteUtc);
    }

    private static async Task<MasterDataSnapshot> WaitForReload(MasterDataHolder holder, MasterDataSnapshot previous)
    {
        DateTime deadline = DateTime.UtcNow.AddSeconds(15);
        while (ReferenceEquals(holder.Current, previous))
        {
            Assert.True(DateTime.UtcNow < deadline, "Master data was not reloaded in time.");
            await Task.Delay(100, TestContext.Current.CancellationToken);
        }

        return holder.Current;
    }
}
