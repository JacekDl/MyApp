using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Instructions.Commands;
using MyApp.Domain.Reviews.Commands;
using MyApp.Model;
using MyApp.Tests.Common;

namespace MyApp.Tests.Domain.Reviews
{
    public class UpdateReviewHandlerTests : TestBase
    {
        #region Validator
        [Fact]
        public void Validator_Fails_When_Number_IsEmpty()
        {
            var validator = new UpdateReviewHandler.UpdateReviewCommandValidator();
            var cmd = new UpdateReviewCommand("", "Jakaś opinia");

            var result = validator.TestValidate(cmd);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.PropertyName == nameof(UpdateReviewCommand.Number) &&
                e.ErrorMessage == "Numer tokenu jest wymagany.");
        }

        [Theory]
        [InlineData("123456789012345")]      // 15 chars
        [InlineData("12345678901234567")]    // 17 chars
        public void Validator_Fails_When_Number_HasInvalidLength(string number)
        {
            var validator = new UpdateReviewHandler.UpdateReviewCommandValidator();
            var cmd = new UpdateReviewCommand(number, "Jakaś opinia");

            var result = validator.TestValidate(cmd);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.PropertyName == nameof(UpdateReviewCommand.Number) &&
                e.ErrorMessage.Contains("musi mieć dokładnie 16 znaków"));
        }

        [Fact]
        public void Validator_Fails_When_Number_IsNotAlphanumeric()
        {
            var validator = new UpdateReviewHandler.UpdateReviewCommandValidator();
            var cmd = new UpdateReviewCommand("1234-5678_90ABC!", "Jakaś opinia");

            var result = validator.TestValidate(cmd);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.PropertyName == nameof(UpdateReviewCommand.Number) &&
                e.ErrorMessage == "Numer tokenu może zawierać tylko litery i cyfry.");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Validator_Fails_When_ReviewText_IsNullOrWhitespace(string? reviewText)
        {
            var validator = new UpdateReviewHandler.UpdateReviewCommandValidator();
            var cmd = new UpdateReviewCommand("1234567890ABCDEF", reviewText!);

            var result = validator.TestValidate(cmd);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.PropertyName == nameof(UpdateReviewCommand.ReviewText) &&
                e.ErrorMessage == "Opinia nie może być pusta.");
        }

        [Fact]
        public void Validator_Fails_When_ReviewText_TooLong()
        {
            var validator = new UpdateReviewHandler.UpdateReviewCommandValidator();
            var longText = new string('x', 201);
            var cmd = new UpdateReviewCommand("1234567890ABCDEF", longText);

            var result = validator.TestValidate(cmd);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.PropertyName == nameof(UpdateReviewCommand.ReviewText) &&
                e.ErrorMessage == "Opinia nie może przekraczać 200 znaków.");
        }

        [Fact]
        public void Validator_Succeeds_For_ValidCommand()
        {
            var validator = new UpdateReviewHandler.UpdateReviewCommandValidator();
            var cmd = new UpdateReviewCommand("1234567890ABCDEF", "Poprawna opinia");

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
            var sut = new UpdateReviewHandler(db);

            var cmd = new UpdateReviewCommand("", "");

            var result = await sut.Handle(cmd, CancellationToken.None);

            result.Succeeded.Should().BeFalse();
            result.ErrorMessage.Should().NotBeNullOrWhiteSpace();
            result.ErrorMessage!.Should().Contain("Numer tokenu jest wymagany.");
            result.ErrorMessage.Should().Contain("Opinia nie może być pusta.");
            (await db.Reviews.CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task Handle_ReturnsError_WhenReviewNotFound()
        {
            await using var db = CreateInMemoryDb();
            var sut = new UpdateReviewHandler(db);

            var cmd = new UpdateReviewCommand("1234567890ABCDEF", "Jakaś opinia");

            var result = await sut.Handle(cmd, CancellationToken.None);

            result.Succeeded.Should().BeFalse();
            result.ErrorMessage.Should().Be("Nie znaleziono zaleceń.");
        }

        [Fact]
        public async Task Handle_ReturnsError_WhenReviewAlreadyCompleted()
        {
            await using var db = CreateInMemoryDb();

            var review = Review.Create(
                pharmacistId: "pharm1",
                initialTxt: "Początkowe zalecenia",
                userRole: "Pharmacist",
                number: "1234567890ABCDEF"
                );

            review.Completed = true;
            db.Reviews.Add(review);
            await db.SaveChangesAsync();

            var sut = new UpdateReviewHandler(db);
            var cmd = new UpdateReviewCommand("1234567890ABCDEF", "Nowa opinia pacjenta");

            var result = await sut.Handle(cmd, CancellationToken.None);

            result.Succeeded.Should().BeFalse();
            result.ErrorMessage.Should().Be("Opinia została już przesłana.");
            review.Entries.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_ReturnsError_WhenReviewExpired()
        {
            await using var db = CreateInMemoryDb();

            var review = Review.Create(
                pharmacistId: "pharm1",
                initialTxt: "Początkowe zalecenia",
                userRole: "Pharmacist",
                number: "1234567890ABCDEF");

            review.DateCreated = DateTime.Now.AddDays(-61);
            db.Reviews.Add(review);
            await db.SaveChangesAsync();

            var sut = new UpdateReviewHandler(db);
            var cmd = new UpdateReviewCommand("1234567890ABCDEF", "Opinia po terminie");

            var result = await sut.Handle(cmd, CancellationToken.None);

            result.Succeeded.Should().BeFalse();
            result.ErrorMessage.Should().Be("Zalecenia już wygasły.");
            review.Entries.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_AddsAnonymousEntry_AndMarksCompleted_WhenValid()
        {
            await using var db = CreateInMemoryDb();

            var review = Review.Create(
                pharmacistId: "pharm1",
                initialTxt: "Początkowe zalecenia",
                userRole: "Pharmacist",
                number: "1234567890ABCDEF"
                );

            review.DateCreated = DateTime.Now.AddDays(-59);
            review.Completed = false;
            db.Reviews.Add(review);
            await db.SaveChangesAsync();

            var sut = new UpdateReviewHandler(db);

            var cmd = new UpdateReviewCommand("1234567890ABCDEF", "  Bardzo pomocne zalecenia  ");
            var trimmed = "Bardzo pomocne zalecenia";

            var result = await sut.Handle(cmd, CancellationToken.None);

            result.Succeeded.Should().BeTrue();
            result.ErrorMessage.Should().BeNullOrEmpty();

            var reloaded = await db.Reviews
                .Include(r => r.Entries)
                .SingleAsync(r => r.Id == review.Id);

            reloaded.Completed.Should().BeTrue();
            reloaded.Entries.Should().HaveCount(2);

            var lastEntry = reloaded.Entries.Last();
            lastEntry.UserId.Should().BeNull();           
            lastEntry.Text.Should().Be(trimmed);          
            lastEntry.ReviewId.Should().Be(review.Id);
        }
        #endregion
    }
}
