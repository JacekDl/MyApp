using FluentAssertions;
using Moq;
using MyApp.Domain.Abstractions;
using MyApp.Domain.Users.Commands;
using MyApp.Model;
using MyApp.Tests.Common;

namespace MyApp.Tests.Domain.Users;

public class SendEmailConfirmationHandlerTests : TestBase
{
    [Fact]
    public async Task ReturnsError_UserNotFound()
    {
        var emailMock = new Mock<IEmailSender>(MockBehavior.Strict);
        var userManagerMock = CreateUserManagerMock(users: Array.Empty<User>());

        var sut = new SendEmailConfirmationHandler(emailMock.Object, userManagerMock.Object);

        var result = await sut.Handle(
            new SendEmailConfirmationCommand("missing", "https://app/callback"),
            CancellationToken.None
        );

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Be("Nie znaleziono Id użytkownika.");

        userManagerMock.Verify(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<User>()), Times.Never);
        emailMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ReturnsSuccess_EmailAlreadyConfirmed()
    {
        var emailMock = new Mock<IEmailSender>(MockBehavior.Strict);

        var user = new User
        {
            Id = "u1",
            Email = "u1@test.com",
            EmailConfirmed = true
        };

        var userManagerMock = CreateUserManagerMock(new[] { user });

        var sut = new SendEmailConfirmationHandler(emailMock.Object, userManagerMock.Object);

        var result = await sut.Handle(
            new SendEmailConfirmationCommand("u1", "https://app/callback"),
            CancellationToken.None
        );

        result.Succeeded.Should().BeTrue();
        result.ErrorMessage.Should().BeNullOrEmpty();

        userManagerMock.Verify(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<User>()), Times.Never);
        emailMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
