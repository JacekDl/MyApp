using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using MyApp.Domain.Users.Commands;
using MyApp.Model;
using MyApp.Tests.Common;

namespace MyApp.Tests.Domain.Users;

public class UpdateUserDetailsHandlerTests : TestBase
{
    [Fact]
    public async Task ReturnsError_UpdateFails()
    {
        var user = new User
        {
            Id = "u1",
            Email = "u1@test.com",
            UserName = "u1@test.com",
            DisplayName = null
        };

        var userManagerMock = CreateUserManagerMock(new[] { user });

        userManagerMock
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Failed(
                new IdentityError { Code = "E1", Description = "err1" },
                new IdentityError { Code = "E2", Description = "err2" }
            ));

        var sut = new UpdateUserDetailsHandler(userManagerMock.Object);

        var result = await sut.Handle(new UpdateUserDetailsCommand("u1", "Jan"), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Be("E1: err1;E2: err2");
    }

    [Fact]
    public async Task Sets_DisplayName()
    {
        var user = new User
        {
            Id = "u1",
            Email = "u1@test.com",
            UserName = "u1@test.com",
            DisplayName = null
        };

        var userManagerMock = CreateUserManagerMock(new[] { user });

        userManagerMock
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Success);

        var sut = new UpdateUserDetailsHandler(userManagerMock.Object);

        var result = await sut.Handle(new UpdateUserDetailsCommand("u1", "  Jan  "), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be("u1");
        result.Value.DisplayName.Should().Be("Jan");

        userManagerMock.Verify(x => x.UpdateAsync(It.Is<User>(u => u.Id == "u1" && u.DisplayName == "Jan")), Times.Once);
    }
}
