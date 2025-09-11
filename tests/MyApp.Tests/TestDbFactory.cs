using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using MyApp.Infrastructure;

namespace MyApp.Tests;

public static class TestDbFactory
{
    public static (ApplicationDbContext Db, SqliteConnection Connection) CreateSqliteInMemoryDb()
    {
        var conn = new SqliteConnection("Filename=:memory:");
        conn.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(conn)
            .EnableSensitiveDataLogging()
            .Options;

        var db = new ApplicationDbContext(options);
        db.Database.EnsureCreated();

        return (db, conn);
    }
}
