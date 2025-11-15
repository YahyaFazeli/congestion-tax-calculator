using Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Tests.Integration.TestHelpers;

public class DatabaseFixture : IDisposable
{
    private readonly SqliteConnection _connection;

    public DatabaseFixture()
    {
        // Create in-memory SQLite database
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
    }

    public CongestionTaxDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CongestionTaxDbContext>()
            .UseSqlite(_connection)
            .Options;

        var context = new CongestionTaxDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    public void Dispose()
    {
        _connection.Close();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }
}
