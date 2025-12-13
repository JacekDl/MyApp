using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using MyApp.Domain.Data;
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

    /// <summary>
    /// Creates a UserManager mock that always returns the given user from FindByIdAsync
    /// and treats them as Admin/non-Admin according to isAdmin.
    /// </summary>
    protected static UserManager<User> CreateUserManager(User? user, bool isAdmin)
    {
        var storeMock = new Mock<IUserStore<User>>();

        var optionsMock = new Mock<IOptions<IdentityOptions>>();
        optionsMock.Setup(o => o.Value).Returns(new IdentityOptions());

        var userValidators = Array.Empty<IUserValidator<User>>();
        var pwdValidators = Array.Empty<IPasswordValidator<User>>();

        var userManagerMock = new Mock<UserManager<User>>(
            storeMock.Object,
            optionsMock.Object,
            new PasswordHasher<User>(),
            userValidators,
            pwdValidators,
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            null!,
            new Mock<ILogger<UserManager<User>>>().Object);

        userManagerMock
            .Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(user);

        // Simulate roles
        userManagerMock
            .Setup(m => m.IsInRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync((User u, string role) => isAdmin && role == "Admin");

        userManagerMock
            .Setup(m => m.GetRolesAsync(It.IsAny<User>()))
            .ReturnsAsync(isAdmin ? new List<string> { "Admin" } : new List<string>());

        userManagerMock
            .Setup(m => m.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        return userManagerMock.Object;
    }

    /// <summary>
    /// Creates a UserManager mock that returns userToReturn from FindByEmailAsync.
    /// Roles are empty by default.
    /// </summary>
    protected static UserManager<User> CreateUserManager(User? userToReturn)
    {
        var store = new Mock<IUserStore<User>>();

        var options = new Mock<IOptions<IdentityOptions>>();
        options.Setup(o => o.Value).Returns(new IdentityOptions());

        var userManager = new Mock<UserManager<User>>(
            store.Object,
            options.Object,
            new PasswordHasher<User>(),
            Array.Empty<IUserValidator<User>>(),
            Array.Empty<IPasswordValidator<User>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            null,
            new Mock<ILogger<UserManager<User>>>().Object);

        userManager
            .Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(userToReturn);

        userManager
            .Setup(m => m.GetRolesAsync(It.IsAny<User>()))
            .ReturnsAsync(new List<string>());

        userManager
            .Setup(m => m.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        return userManager.Object;
    }

    /// <summary>
    /// Creates a UserManager mock that can return multiple users by Id.
    /// For simplicity: the first user in the array is treated as Admin.
    /// </summary>
    protected static UserManager<User> CreateUserManagerForMultipleUsers(params User[] users)
    {
        var store = new Mock<IUserStore<User>>();

        var options = new Mock<IOptions<IdentityOptions>>();
        options.Setup(o => o.Value).Returns(new IdentityOptions());

        var userManager = new Mock<UserManager<User>>(
            store.Object,
            options.Object,
            new PasswordHasher<User>(),
            Array.Empty<IUserValidator<User>>(),
            Array.Empty<IPasswordValidator<User>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            null,
            new Mock<ILogger<UserManager<User>>>().Object);

        userManager.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((string id) => users.FirstOrDefault(u => u.Id == id));

        // First user in the array is considered Admin
        userManager.Setup(m => m.IsInRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync((User u, string role) =>
            {
                if (role != "Admin") return false;
                if (users.Length == 0) return false;
                return u.Id == users[0].Id;
            });

        userManager.Setup(m => m.GetRolesAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) =>
            {
                if (users.Length > 0 && u.Id == users[0].Id)
                {
                    return new List<string> { "Admin" };
                }

                return new List<string>();
            });

        userManager
            .Setup(m => m.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        return userManager.Object;
    }

    protected static Mock<RoleManager<IdentityRole>> CreateRoleManagerMock()
    {
        var store = new Mock<IRoleStore<IdentityRole>>();

        var roleManager = new Mock<RoleManager<IdentityRole>>(
            store.Object,
            Array.Empty<IRoleValidator<IdentityRole>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            new Mock<ILogger<RoleManager<IdentityRole>>>().Object);

        return roleManager;
    }

    protected static Mock<SignInManager<User>> CreateSignInManagerMock(UserManager<User> userManager)
    {
        var contextAccessor = new Mock<IHttpContextAccessor>();
        var claimsFactory = new Mock<IUserClaimsPrincipalFactory<User>>();
        var options = new Mock<IOptions<IdentityOptions>>();
        options.Setup(o => o.Value).Returns(new IdentityOptions());
        var logger = new Mock<ILogger<SignInManager<User>>>();
        var schemes = new Mock<IAuthenticationSchemeProvider>();
        var confirmation = new Mock<IUserConfirmation<User>>();

        return new Mock<SignInManager<User>>(
            userManager,
            contextAccessor.Object,
            claimsFactory.Object,
            options.Object,
            logger.Object,
            schemes.Object,
            confirmation.Object);
    }
}
