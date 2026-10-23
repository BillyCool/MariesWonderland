using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MariesWonderland.Persistence;

/// <summary>The user database on SQLite. Its migrations live in <c>Persistence/Migrations/Sqlite</c>.</summary>
public sealed class SqliteUserDbContext(DbContextOptions<SqliteUserDbContext> options) : UserDbContext(options)
{
    /// <summary>Options for a SQLite file (or any SQLite connection string).</summary>
    public static DbContextOptions<SqliteUserDbContext> Options(string connectionString)
        => new DbContextOptionsBuilder<SqliteUserDbContext>()
            .UseSqlite(connectionString)
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
            .Options;
}

/// <summary>Lets <c>dotnet ef migrations add</c> build the SQLite context without starting the server.</summary>
public sealed class SqliteUserDbContextDesignFactory : IDesignTimeDbContextFactory<SqliteUserDbContext>
{
    public SqliteUserDbContext CreateDbContext(string[] args)
        => new(SqliteUserDbContext.Options("Data Source=design-time.db"));
}
