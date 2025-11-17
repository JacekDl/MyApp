using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Reviews.Queries;
using MyApp.Model;
using MyApp.Tests.Common;
using Xunit;

namespace MyApp.Tests.Domain.Reviews
{
    public class GetReviewHandlerTests : TestBase
    {
        #region Validator
        [Fact]
        public void Validator_Fails_When_Number_IsEmpty()
        {
            var validator = new GetReviewHandler.GetReviewValidator();
            var query = new GetReviewQuery("");

            var result = validator.TestValidate(query);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.PropertyName == nameof(GetReviewQuery.Number) &&
                e.ErrorMessage == "Numer tokenu jest wymagany.");
        }

        [Theory]
        [InlineData("123456789012345")]      // 15 chars
        [InlineData("12345678901234567")]    // 17 chars
        public void Validator_Fails_When_Number_HasInvalidLength(string number)
        {
            var validator = new GetReviewHandler.GetReviewValidator();
            var query = new GetReviewQuery(number);

            var result = validator.TestValidate(query);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.PropertyName == nameof(GetReviewQuery.Number) &&
                e.ErrorMessage == "Numer tokenu musi mieć dokładnie 16 znaków.");
        }

        [Fact]
        public void Validator_Fails_When_Number_NotAlphanumeric()
        {
            var validator = new GetReviewHandler.GetReviewValidator();
            var query = new GetReviewQuery("1234-5678_90ABCD");

            var result = validator.TestValidate(query);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.PropertyName == nameof(GetReviewQuery.Number) &&
                e.ErrorMessage == "Numer tokenu może zawierać tylko litery i cyfry.");
        }

        [Fact]
        public void Validator_Succeeds_For_Valid_Number()
        {
            var validator = new GetReviewHandler.GetReviewValidator();
            var query = new GetReviewQuery("1234567890ABCDEF");

            var result = validator.TestValidate(query);

            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }
        #endregion

        #region Handler
        [Fact]
        public async Task Handle_ReturnsError_WhenReviewNotFound()
        {
            await using var db = CreateInMemoryDb();
            var sut = new GetReviewHandler(db);

            var query = new GetReviewQuery("1234567890ABCDEF");

            var result = await sut.Handle(query, CancellationToken.None);

            result.Succeeded.Should().BeFalse();
            result.ErrorMessage.Should().Be("Nie znaleziono zaleceń.");
            result.Value.Should().BeNull();
        }

        [Fact]
        public async Task Handle_ReturnsError_WhenReviewAlreadyCompleted()
        {
            await using var db = CreateInMemoryDb();

            var review = Review.Create(
                pharmacistId: "pharm1",
                initialTxt: "Initial advice",
                number: "1234567890ABCDEF");

            review.Completed = true;

            db.Reviews.Add(review);
            await db.SaveChangesAsync();

            var sut = new GetReviewHandler(db);
            var query = new GetReviewQuery("1234567890ABCDEF");

            var result = await sut.Handle(query, CancellationToken.None);

            result.Succeeded.Should().BeFalse();
            result.ErrorMessage.Should().Be("Wykorzystano już kod zaleceń.");
            result.Value.Should().BeNull();
        }

        [Fact]
        public async Task Handle_ReturnsError_WhenReviewExpired()
        {
            await using var db = CreateInMemoryDb();

            var review = Review.Create(
                pharmacistId: "pharm1",
                initialTxt: "Initial advice",
                number: "1234567890ABCDEF");

            review.DateCreated = DateTime.UtcNow.AddDays(-61);

            db.Reviews.Add(review);
            await db.SaveChangesAsync();

            var sut = new GetReviewHandler(db);
            var query = new GetReviewQuery("1234567890ABCDEF");

            var result = await sut.Handle(query, CancellationToken.None);

            result.Succeeded.Should().BeFalse();
            result.ErrorMessage.Should().Be("Minął już termin wykorzystania kodu.");
            result.Value.Should().BeNull();
        }

        [Fact]
        public async Task Handle_ReturnsReviewDto_WhenValid_NotCompleted_NotExpired()
        {
            await using var db = CreateInMemoryDb();

            var review = Review.Create(
                pharmacistId: "pharm1",
                initialTxt: "Initial advice",
                number: "1234567890ABCDEF");

            review.Completed = false;
            review.DateCreated = DateTime.UtcNow.AddDays(-10);

            db.Reviews.Add(review);
            await db.SaveChangesAsync();

            var sut = new GetReviewHandler(db);
            var query = new GetReviewQuery("1234567890ABCDEF");

            var result = await sut.Handle(query, CancellationToken.None);

            result.Succeeded.Should().BeTrue();
            result.ErrorMessage.Should().BeNullOrEmpty();
            result.Value.Should().NotBeNull();

            var dto = result.Value!;

            dto.Id.Should().Be(review.Id);
            dto.PharmacistId.Should().Be(review.PharmacistId);
            dto.Number.Should().Be(review.Number);
            dto.DateCreated.Should().Be(review.DateCreated);

            dto.Text.Should().Be("Initial advice");
            dto.ReviewText.Should().Be(string.Empty);
            dto.Completed.Should().BeFalse();
            dto.IsNewForViewer.Should().BeTrue();
        }
        #endregion
    }
}
