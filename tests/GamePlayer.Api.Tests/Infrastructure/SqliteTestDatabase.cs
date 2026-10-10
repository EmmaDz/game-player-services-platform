using GamePlayer.Api.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace GamePlayer.Api.Tests.Infrastructure;

public sealed class SqliteTestDatabase : IAsyncDisposable
{
    public SqliteConnection Connection { get; }

    public AppDbContext DbContext { get; }

    private SqliteTestDatabase(
        SqliteConnection connection,
        AppDbContext dbContext
    )
    {
        Connection = connection;
        DbContext = dbContext;
    }

    public static async Task<SqliteTestDatabase> CreateAsync()
    {
        var connection =
            new SqliteConnection(
                "Data Source=:memory:"
            );

        await connection.OpenAsync();

        var options =
            new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connection)
                .Options;

        var dbContext =
            new AppDbContext(options);

        await dbContext.Database
            .EnsureCreatedAsync();

        return new SqliteTestDatabase(
            connection,
            dbContext
        );
    }

    public async ValueTask DisposeAsync()
    {
        await DbContext.DisposeAsync();
        await Connection.DisposeAsync();
    }
}