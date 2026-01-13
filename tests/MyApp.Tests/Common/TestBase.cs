using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using MyApp.Domain.Data;
using MyApp.Domain.Users;
using MyApp.Model;

namespace MyApp.Tests.Common;

public class TestBase
{
    protected static ApplicationDbContext CreateInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;
        return new ApplicationDbContext(options);
    }

    protected static UserManager<User> CreateUserManager(User[]? users = null, bool firstUserIsAdmin = false)
    {
        return CreateUserManagerMock(users, firstUserIsAdmin).Object;
    }

    protected static Mock<UserManager<User>> CreateUserManagerMock(User[]? users = null, bool firstUserIsAdmin = false)
    {
        users ??= Array.Empty<User>();

        var store = new Mock<IUserStore<User>>();
        var options = Mock.Of<IOptions<IdentityOptions>>(o => o.Value == new IdentityOptions());
        var logger = Mock.Of<ILogger<UserManager<User>>>();

        var mock = new Mock<UserManager<User>>(
            store.Object,
            options,
            new PasswordHasher<User>(),
            Array.Empty<IUserValidator<User>>(),
            Array.Empty<IPasswordValidator<User>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            null!,
            logger
        );

        mock.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((string id) => users.FirstOrDefault(u => u.Id == id));

        mock.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((string email) =>
                users.FirstOrDefault(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase)));

        mock.Setup(m => m.IsInRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync((User u, string role) =>
                firstUserIsAdmin &&
                role == UserRoles.Admin &&
                users.Length > 0 &&
                u.Id == users[0].Id);

        mock.Setup(m => m.GetRolesAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) =>
                firstUserIsAdmin && users.Length > 0 && u.Id == users[0].Id
                    ? new List<string> { UserRoles.Admin }
                    : new List<string>());

        mock.Setup(m => m.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        return mock;
    }

    protected static Mock<UserManager<User>> CreateConfiguredUserManagerMock(User[]? users = null)
    {
        var um = CreateUserManagerMock(users);

        um.Setup(x => x.IsInRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
          .ReturnsAsync(false);

        um.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
          .ReturnsAsync(IdentityResult.Success);

        um.Setup(x => x.RemoveFromRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
          .ReturnsAsync(IdentityResult.Success);

        return um;
    }

    protected static Mock<RoleManager<IdentityRole>> CreateRoleManagerMock()
    {
        var store = new Mock<IRoleStore<IdentityRole>>();
        var roles = Array.Empty<IRoleValidator<IdentityRole>>();
        var keyNormalizer = new UpperInvariantLookupNormalizer();
        var errors = new IdentityErrorDescriber();
        var logger = new Mock<ILogger<RoleManager<IdentityRole>>>();
        var options = Mock.Of<IOptions<IdentityOptions>>(o => o.Value == new IdentityOptions());

        return new Mock<RoleManager<IdentityRole>>(
            store.Object, roles, keyNormalizer, errors, logger.Object
        );
    }
}
