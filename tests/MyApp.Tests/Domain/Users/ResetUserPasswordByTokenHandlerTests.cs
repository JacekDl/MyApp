using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using MyApp.Domain.Users.Commands;
using MyApp.Model;
using MyApp.Tests.Common;

namespace MyApp.Tests.Domain.Users;

public class ResetUserPasswordByTokenHandlerTests : TestBase
{
    [Fact]
    public async Task ReturnsError_ResetPasswordFails()
    {
        var user = new User { Id = "u1", Email = "u1@test.com" };

        var userManagerMock = CreateUserManagerMock(new[] { user });

        userManagerMock
            .Setup(x => x.ResetPasswordAsync(user, "token", "123456"))
            .ReturnsAsync(IdentityResult.Failed(
                new IdentityError { Description = "err1" },
                new IdentityError { Description = "err2" }
            ));

        var sut = new ResetUserPasswordByTokenHandler(userManagerMock.Object);

        var cmd = new ResetUserPasswordByTokenCommand(
            UserId: "u1",
            Token: "token",
            NewPassword: "123456"
        );

        var result = await sut.Handle(cmd, CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Be("Nie udało się zresetować hasła: err1; err2");
    }

    [Fact]
    public async Task ReturnsSuccess_ResetPasswordSucceeds()
    {
        var user = new User { Id = "u1", Email = "u1@test.com" };

        var userManagerMock = CreateUserManagerMock(new[] { user });

        userManagerMock
            .Setup(x => x.ResetPasswordAsync(user, "token", "123456"))
            .ReturnsAsync(IdentityResult.Success);

        var sut = new ResetUserPasswordByTokenHandler(userManagerMock.Object);

        var cmd = new ResetUserPasswordByTokenCommand(
            UserId: "u1",
            Token: "token",
            NewPassword: "123456"
        );

        var result = await sut.Handle(cmd, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.ErrorMessage.Should().BeNullOrEmpty();

        userManagerMock.Verify(x => x.ResetPasswordAsync(user, "token", "123456"), Times.Once);
    }
}
