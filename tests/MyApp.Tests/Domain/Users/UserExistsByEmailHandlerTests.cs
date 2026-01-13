using FluentAssertions;
using Moq;
using MyApp.Domain.Users.Queries;
using MyApp.Model;
using MyApp.Tests.Common;

namespace MyApp.Tests.Domain.Users;

public class UserExistsByEmailHandlerTests : TestBase
{
    [Fact]
    public async Task ReturnsError_UserDoesNotExist()
    {
        var userManagerMock = CreateUserManagerMock(users: Array.Empty<User>());
        var sut = new UserExistsByEmailHandler(userManagerMock.Object);

        var result = await sut.Handle(new UserExistsByEmailQuery("missing@test.com"), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Be("Użytkownik o podanym adresie email nie istnieje.");
    }

    [Fact]
    public async Task ReturnsSuccess_UserExists()
    {
        var user = new User { Id = "u1", Email = "u1@test.com", UserName = "u1@test.com" };
        var userManagerMock = CreateUserManagerMock(new[] { user });

        userManagerMock.Setup(x => x.FindByEmailAsync("u1@test.com")).ReturnsAsync(user);

        var sut = new UserExistsByEmailHandler(userManagerMock.Object);

        var result = await sut.Handle(new UserExistsByEmailQuery("u1@test.com"), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.ErrorMessage.Should().BeNullOrEmpty();

        userManagerMock.Verify(x => x.FindByEmailAsync("u1@test.com"), Times.Once);
    }
}
