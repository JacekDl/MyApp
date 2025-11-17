using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using MyApp.Domain.Reviews.Commands;
using MyApp.Model;
using MyApp.Tests.Common;

namespace MyApp.Tests.Domain.Reviews
{
    public class CreateReviewHandlerTests : TestBase
    {
        #region Validator
        [Fact]
        public void Validator_Fails_When_UserId_IsEmpty()
        {
            var validator = new CreateReviewValidator();
            var cmd = new CreateReviewCommand("", "Jakieś zalecenia");

            var result = validator.TestValidate(cmd);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.PropertyName == nameof(CreateReviewCommand.UserId) &&
                e.ErrorMessage.Contains("UserId jest wymagany."));
        }

        [Fact]
        public void Validator_Fails_When_Advice_IsEmpty()
        {
            var validator = new CreateReviewValidator();
            var cmd = new CreateReviewCommand("user1", "");

            var result = validator.TestValidate(cmd);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.PropertyName == nameof(CreateReviewCommand.Advice) &&
                e.ErrorMessage.Contains("Zalecenia są wymagane."));
        }

        [Fact]
        public void Validator_Fails_When_Advice_TooLong()
        {
            var validator = new CreateReviewValidator();
            var longAdvice = new string('x', 501); // > 500
            var cmd = new CreateReviewCommand("user1", longAdvice);

            var result = validator.TestValidate(cmd);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.PropertyName == nameof(CreateReviewCommand.Advice) &&
                e.ErrorMessage.Contains("500"));
        }

        [Fact]
        public void Validator_Succeeds_For_ValidCommand()
        {
            var validator = new CreateReviewValidator();
            var cmd = new CreateReviewCommand("user1", "Poprawne zalecenia");

            var result = validator.TestValidate(cmd);

            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }
        #endregion

        #region Handler
        [Fact]
        public async Task Handle_ReturnsError_WhenValidationFails()
        {
            await using var db = CreateInMemoryDb();
            var userManager = CreateUserManager(userToReturn: null);
            var sut = new CreateReviewHandler(userManager, db);

            var cmd = new CreateReviewCommand("", "");

            var result = await sut.Handle(cmd, CancellationToken.None);

            result.Succeeded.Should().BeFalse();
            result.ErrorMessage.Should().NotBeNullOrWhiteSpace();
            result.ErrorMessage!.Should().Contain("UserId jest wymagany.");
            result.ErrorMessage.Should().Contain("Zalecenia są wymagane.");

            (await db.Reviews.CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task Handle_ReturnsError_WhenUserNotFound()
        {
            await using var db = CreateInMemoryDb();
            var userManager = CreateUserManager(userToReturn: null);
            var sut = new CreateReviewHandler(userManager, db);

            var cmd = new CreateReviewCommand("pharm1", "Jakieś zalecenia");

            var result = await sut.Handle(cmd, CancellationToken.None);

            result.Succeeded.Should().BeFalse();
            result.ErrorMessage.Should().Be("Nie znaleziono użytkownika.");
            (await db.Reviews.CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task Handle_CreatesReview_WhenDataIsValid_AndUserExists()
        {
            await using var db = CreateInMemoryDb();

            var pharmacist = new User
            {
                Id = "pharm1",
                UserName = "pharmacist@example.com"
            };

            var userManager = CreateUserManager(pharmacist);
            var sut = new CreateReviewHandler(userManager, db);

            var advice = "Przyjmować 1 tabletkę 3 razy dziennie po posiłku.";
            var cmd = new CreateReviewCommand(pharmacist.Id, advice);

            var result = await sut.Handle(cmd, CancellationToken.None);

            result.Succeeded.Should().BeTrue();
            result.ErrorMessage.Should().BeNullOrEmpty();
            result.Value.Should().NotBeNull();

            var reviewFromResult = result.Value!;
            reviewFromResult.Id.Should().BeGreaterThan(0);
            reviewFromResult.Number.Should().NotBeNullOrWhiteSpace();
            reviewFromResult.Entries[0].Text.Should().Be(advice);
            reviewFromResult.Number.Length.Should().Be(16); // GenerateDigits(..bytes:16)
            reviewFromResult.PharmacistId.Should().Be(pharmacist.Id);

            var reviewInDb = await db.Reviews.SingleAsync();
            reviewInDb.Id.Should().Be(reviewFromResult.Id);
            reviewInDb.Number.Should().Be(reviewFromResult.Number);
            reviewInDb.Entries[0].Text.Should().Be(advice);
            reviewInDb.PharmacistId.Should().Be(pharmacist.Id);
        }

        #endregion
    }
}
