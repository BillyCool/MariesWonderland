namespace MariesWonderland.Tests.Infrastructure;

/// <summary>
/// A <see cref="FactAttribute"/> for tests that load the real master data. The test is skipped
/// when the master data file isn't available on this machine (see <see cref="TestSettings"/>).
/// </summary>
public sealed class MasterDataFactAttribute : FactAttribute
{
    public MasterDataFactAttribute()
    {
        if (!TestSettings.IsMasterDataAvailable)
        {
            Skip = $"Master data not found at '{TestSettings.MasterDataPath}'. " +
                   "Set Server:Paths:MasterDatabase in src/appsettings.Local.json or the Server__Paths__MasterDatabase environment variable.";
        }
    }
}
