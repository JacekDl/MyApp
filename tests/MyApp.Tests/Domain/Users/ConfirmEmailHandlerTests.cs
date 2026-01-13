using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using MyApp.Domain.Users.Commands;
using MyApp.Model;
using MyApp.Tests.Common;

namespace MyApp.Tests.Domain.Users;

public class ConfirmEmailHandlerTests : TestBase
{
    [Fact]
    public async Task ReturnsSuccess_AndDoesNotCallConfirmEmail()
    {
        var user = new User
        {
            Id = "u1",
            Email = "u1@test.com",
            EmailConfirmed = true
        };

        var userManagerMock = CreateConfiguredUserManagerMock(new[] { user });
        var sut = new ConfirmEmailHandler(userManagerMock.Object);

        var cmd = new ConfirmEmailCommand(UserId: "u1", Token: "token");

        var result = await sut.Handle(cmd, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.ErrorMessage.Should().BeNullOrEmpty();

        userManagerMock.Verify(x => x.ConfirmEmailAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task CallsConfirmEmail()
    {
        var user = new User
        {
            Id = "u1",
            Email = "u1@test.com",
            EmailConfirmed = false
        };

        var userManagerMock = CreateConfiguredUserManagerMock(new[] { user });

        userManagerMock
            .Setup(x => x.ConfirmEmailAsync(user, "token"))
            .ReturnsAsync(IdentityResult.Success);

        var sut = new ConfirmEmailHandler(userManagerMock.Object);

        var cmd = new ConfirmEmailCommand(UserId: "u1", Token: "token");

        var result = await sut.Handle(cmd, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.ErrorMessage.Should().BeNullOrEmpty();

        userManagerMock.Verify(x => x.ConfirmEmailAsync(user, "token"), Times.Once);
    }
}
