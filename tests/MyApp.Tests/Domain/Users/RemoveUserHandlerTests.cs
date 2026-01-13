using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Identity;
using Moq;
using MyApp.Domain.Users.Commands;
using MyApp.Model;
using MyApp.Tests.Common;

namespace MyApp.Tests.Domain.Users;

public class RemoveUserHandlerTests : TestBase
{
    [Fact]
    public async Task ReturnsError_DeleteFails()
    {
        var user = new User { Id = "u1", Email = "u1@test.com" };
        var userManagerMock = CreateUserManagerMock(new[] { user });

        userManagerMock
            .Setup(x => x.DeleteAsync(user))
            .ReturnsAsync(IdentityResult.Failed(
                new IdentityError { Code = "E1", Description = "err1" },
                new IdentityError { Code = "E2", Description = "err2" }
            ));

        var sut = new RemoveUserHandler(userManagerMock.Object);

        var result = await sut.Handle(new RemoveUserCommand("u1"), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Be("E1: err1;E2: err2");
    }

    [Fact]
    public async Task ReturnsSuccess_UserDeleted()
    {
        var user = new User { Id = "u1", Email = "u1@test.com" };
        var userManagerMock = CreateUserManagerMock(new[] { user });

        userManagerMock
            .Setup(x => x.DeleteAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        var sut = new RemoveUserHandler(userManagerMock.Object);

        var result = await sut.Handle(new RemoveUserCommand("u1"), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.ErrorMessage.Should().BeNullOrEmpty();

        userManagerMock.Verify(x => x.DeleteAsync(user), Times.Once);
    }
}
