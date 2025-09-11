using Microsoft.AspNetCore.Identity;
using MyApp.Services;
using MyApp.Domain;
using Xunit;

namespace MyApp.Tests;

public class UserServiceTests
{
    private static UserService CreateSut(out TestContext ctx)
    {
        ctx = new TestContext();
        return new UserService(ctx.Db, new PasswordHasher<User>());
    }

    public sealed class TestContext : System.IDisposable
    {
        public MyApp.Infrastructure.ApplicationDbContext Db { get; }
        private readonly Microsoft.Data.Sqlite.SqliteConnection _conn;

        public TestContext()
        {
            (Db, _conn) = TestDbFactory.CreateSqliteInMemoryDb();
        }

        public void Dispose()
        {
            Db.Dispose();
            _conn.Dispose();
        }
    }

    [Fact]
    public async Task RegisterAsync_Creates_User_With_Hash()
    {
        var sut = CreateSut(out var ctx);

        var result = await sut.RegisterAsync("alice@example.com", "PasswordA");

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Value);
        Assert.Equal("alice@example.com", result.Value!.Email);
        Assert.False(string.IsNullOrWhiteSpace(result.Value.PasswordHash));
    }

    [Fact]
    public async Task RegisterAsync_Fails_On_Duplicate_Email()
    {
        var sut = CreateSut(out var ctx);

        var first = await sut.RegisterAsync("user@example.com", "PasswordU");
        var second = await sut.RegisterAsync("user@example.com", "PasswordU");

        Assert.True(first.Succeeded);
        Assert.False(second.Succeeded);
        Assert.NotNull(second.Error);
    }

    [Fact]
    public async Task ValidatCredentialsAsync_Returns_Null_On_Bad_Password()
    {
        var sut = CreateSut(out var ctx);

        var reg = await sut.RegisterAsync("bob@example.com", "PasswordB");
        Assert.True(reg.Succeeded);

        var user = await sut.ValidateCredentialsAsync("bob@example.com", "WrongPassword");
        Assert.Null(user);
    }

    [Fact]
    public async Task GetByEmailAsync_Returns_User_When_Exists_Else_Null()
    {
        var sut = CreateSut(out var ctx);
        var reg = await sut.RegisterAsync("cecil@example.com", "PasswordC");
        Assert.True(reg.Succeeded);

        var found = await sut.GetByEmailAsync("cecil@example.com");
        var missing = await sut.GetByEmailAsync("whoever@example.com");
        Assert.NotNull(found);
        Assert.Null(missing);

    }

}
