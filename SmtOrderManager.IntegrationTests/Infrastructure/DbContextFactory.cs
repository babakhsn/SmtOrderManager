using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SmtOrderManager.Infrastructure.Persistence;
using System;

namespace SmtOrderManager.IntegrationTests.Infrastructure;

internal static class DbContextFactory
{
    public static (AppDbContext Db, SqliteConnection Connection) CreateSqliteInMemoryContext()
    {
        // Keep the connection open for the life of the context so the DB persists during the test
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var db = new AppDbContext(options);
        db.Database.EnsureCreated();

        return (db, connection);
    }
}
