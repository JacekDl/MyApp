using System.Net;
using FluentAssertions;
using Moq;
using MyApp.Domain.Abstractions;
using MyApp.Domain.Users.Commands;
using MyApp.Model;
using MyApp.Tests.Common;

namespace MyApp.Tests.Domain.Users;

public class SendPasswordResetEmailHandlerTests : TestBase
{
    [Fact]
    public async Task ReturnsSuccess_UserNotFound_DoesNotSendEmail()
    {
        var emailMock = new Mock<IEmailSender>(MockBehavior.Strict);
        var userManagerMock = CreateUserManagerMock(users: Array.Empty<User>());

        var sut = new SendPasswordResetEmailHandler(emailMock.Object, userManagerMock.Object);

        var result = await sut.Handle(
            new SendPasswordResetEmailCommand("missing@test.com", "https://app/reset", null),
            CancellationToken.None
        );

        result.Succeeded.Should().BeTrue();
        result.ErrorMessage.Should().BeNullOrEmpty();

        userManagerMock.Verify(x => x.GeneratePasswordResetTokenAsync(It.IsAny<User>()), Times.Never);
        emailMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SendsEmail()
    {
        var emailMock = new Mock<IEmailSender>();

        var user = new User
        {
            Id = "u1",
            Email = "u1@test.com",
            UserName = "u1@test.com"
        };

        var userManagerMock = CreateUserManagerMock(new[] { user });

        const string token = "a+b/c== token&x=1";
        userManagerMock
            .Setup(x => x.GeneratePasswordResetTokenAsync(user))
            .ReturnsAsync(token);

        string? capturedBody = null;
        emailMock
            .Setup(x => x.SendEmailAsync(user.Email!, "Reset hasła", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, string, string, CancellationToken>((_, _, body, _) => capturedBody = body)
            .Returns(Task.CompletedTask);

        var sut = new SendPasswordResetEmailHandler(emailMock.Object, userManagerMock.Object);

        var callback = "https://app/reset";
        var result = await sut.Handle(
            new SendPasswordResetEmailCommand(user.Email!, callback, ReturnUrl: null),
            CancellationToken.None
        );

        result.Succeeded.Should().BeTrue();
        capturedBody.Should().NotBeNull();

        var expectedUrl = $"{callback}?userId={user.Id}&token={Uri.EscapeDataString(token)}";
        var expectedHtmlEncodedUrl = WebUtility.HtmlEncode(expectedUrl);

        capturedBody!.Should().Contain(expectedHtmlEncodedUrl);
        capturedBody.Should().NotContain("returnUrl=");

        emailMock.Verify(x => x.SendEmailAsync(user.Email!, "Reset hasła", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        userManagerMock.Verify(x => x.GeneratePasswordResetTokenAsync(user), Times.Once);
    }
}
