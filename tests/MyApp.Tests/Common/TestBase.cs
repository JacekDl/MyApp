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

        userManagerMock
            .Setup(m => m.IsInRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(isAdmin);

        return userManagerMock.Object;
    }

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

        return userManager.Object;
    }

    protected static UserManager<User> CreateUserManagerForMultipleUsers(params User[] users)
    {
        var store = new Mock<IUserStore<User>>();

        var userManager = new Mock<UserManager<User>>(
            store.Object,
            new Mock<IOptions<IdentityOptions>>().Object,
            new PasswordHasher<User>(),
            Array.Empty<IUserValidator<User>>(),
            Array.Empty<IPasswordValidator<User>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            null,
            new Mock<ILogger<UserManager<User>>>().Object);

        userManager.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((string id) => users.FirstOrDefault(u => u.Id == id));

        userManager.Setup(m => m.IsInRoleAsync(It.IsAny<User>(), "Admin"))
            .ReturnsAsync((User u, string role) => u.Role == "Admin");

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
