using FluentAssertions;
using Moq;
using MyApp.Domain.Users.Queries;
using MyApp.Model;
using MyApp.Tests.Common;

namespace MyApp.Tests.Domain.Users;

public class GetUserByIdHandlerTests : TestBase
{
    [Fact]
    public async Task ReturnsError_UserNotFound()
    {
        var userManagerMock = CreateUserManagerMock(users: Array.Empty<User>());
        var sut = new GetUserByIdHandler(userManagerMock.Object);

        var result = await sut.Handle(new GetUserByIdQuery("missing"), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Be("Nie znaleziono użytkownika.");

        userManagerMock.Verify(x => x.GetRolesAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task ReturnsUserDto()
    {
        var user = new User
        {
            Id = "u1",
            Email = "u1@test.com",
            DisplayName = "Jan",
            CreatedUtc = DateTime.UtcNow.AddDays(-1)
        };

        var userManagerMock = CreateUserManagerMock(new[] { user });

        userManagerMock
            .Setup(x => x.GetRolesAsync(It.Is<User>(u => u.Id == "u1")))
            .ReturnsAsync(new List<string> { "Pharmacist", "Admin" });

        var sut = new GetUserByIdHandler(userManagerMock.Object);

        var result = await sut.Handle(new GetUserByIdQuery("u1"), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();

        var dto = result.Value!;
        dto.Id.Should().Be("u1");
        dto.Email.Should().Be("u1@test.com");
        dto.Role.Should().Be("Pharmacist");
        dto.DisplayName.Should().Be("Jan");
        dto.CreatedUtc.Should().Be(user.CreatedUtc);
    }
}
