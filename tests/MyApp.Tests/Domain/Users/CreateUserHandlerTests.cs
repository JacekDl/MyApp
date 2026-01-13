using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using MyApp.Domain.Users;
using MyApp.Domain.Users.Commands;
using MyApp.Model;
using MyApp.Tests.Common;

namespace MyApp.Tests.Domain.Users;

public class CreateUserHandlerTests : TestBase
{
    [Fact]
    public async Task ReturnsError_EmailAlreadyExists()
    {
        var existing = new User { Id = "u1", Email = "existing@test.com" };

        var userManagerMock = CreateConfiguredUserManagerMock(new[] { existing });
        var roleManagerMock = CreateRoleManagerMock();

        var sut = new CreateUserHandler(userManagerMock.Object, roleManagerMock.Object);

        var cmd = new CreateUserCommand(Email: " existing@test.com ", Password: "123456", Role: "Patient");

        var result = await sut.Handle(cmd, CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Be("Wybierz inny adres email.");

        userManagerMock.Verify(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
        userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ReturnsError_CreateUser_Fails()
    {
        var userManagerMock = CreateConfiguredUserManagerMock();
        var roleManagerMock = CreateRoleManagerMock();

        userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(
                new IdentityError { Code = "PasswordTooWeak", Description = "weak" },
                new IdentityError { Code = "Other", Description = "other" }
            ));

        var sut = new CreateUserHandler(userManagerMock.Object, roleManagerMock.Object);

        var cmd = new CreateUserCommand(Email: "a@b.com", Password: "123456", Role: "Patient");

        var result = await sut.Handle(cmd, CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Be("PasswordTooWeak: weak;Other: other");

        userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
    }

    [Theory]
    [InlineData("Patient", UserRoles.Patient)]
    [InlineData("patient", UserRoles.Patient)]
    [InlineData("Pharmacist", UserRoles.Pharmacist)]
    [InlineData("PHARMACIST", UserRoles.Pharmacist)]
    public async Task CreatesUser(string inputRole, string expectedRole)
    {
        var userManagerMock = CreateConfiguredUserManagerMock();
        var roleManagerMock = CreateRoleManagerMock();

        User? createdUser = null;

        userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .Callback<User, string>((u, _) =>
            {
                u.Id = "new-id";
                createdUser = u;
            })
            .ReturnsAsync(IdentityResult.Success);

        userManagerMock
            .Setup(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        var sut = new CreateUserHandler(userManagerMock.Object, roleManagerMock.Object);

        var result = await sut.Handle(
            new CreateUserCommand("  New@Email.com  ", "123456", inputRole),
            CancellationToken.None
        );

        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();

        createdUser.Should().NotBeNull();
        createdUser!.Email.Should().Be("New@Email.com");
        createdUser.UserName.Should().Be("New@Email.com");
        createdUser.EmailConfirmed.Should().BeFalse();

        userManagerMock.Verify(x => x.AddToRoleAsync(createdUser, expectedRole), Times.Once);

        result.Value!.Id.Should().Be("new-id");
        result.Value.Email.Should().Be("New@Email.com");
        result.Value.Role.Should().Be(expectedRole);
    }

}
