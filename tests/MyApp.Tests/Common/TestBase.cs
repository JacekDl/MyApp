using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Data;

namespace MyApp.Tests.Common;

public abstract class TestBase
{
    protected static ApplicationDbContext CreateInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;
        return new ApplicationDbContext(options);
    }
}
