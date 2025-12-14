using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Reviews.Commands;
using MyApp.Domain.Users;
using MyApp.Model;
using MyApp.Tests.Common;

namespace MyApp.Tests.Domain.Reviews
{
    public class DeleteReviewHandlerTests : TestBase
    {
        #region Validator
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-10)]
        public void Validator_Fails_When_Id_IsNotPositive(int id)
        {
            var validator = new DeleteReviewCommandValidator();
            var cmd = new DeleteReviewCommand(id);

            var result = validator.TestValidate(cmd);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.PropertyName == nameof(DeleteReviewCommand.Id) &&
                e.ErrorMessage == "Nieprawidłowy identyfikator zaleceń.");
        }

        [Fact]
        public void Validator_Succeeds_For_Positive_Id()
        {
            var validator = new DeleteReviewCommandValidator();
            var cmd = new DeleteReviewCommand(1);

            var result = validator.TestValidate(cmd);

            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }
        #endregion

        #region Handler
        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public async Task Handle_ReturnsError_When_Id_IsInvalid(int id)
        {
            await using var db = CreateInMemoryDb();
            var sut = new DeleteReviewHandler(db);
            var cmd = new DeleteReviewCommand(id);

            var result = await sut.Handle(cmd, CancellationToken.None);

            result.Succeeded.Should().BeFalse();
            result.ErrorMessage.Should().Be("Nieprawidłowy identyfikator zaleceń.");
            (await db.Reviews.CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task Handle_ReturnsError_When_Review_NotFound()
        {
            await using var db = CreateInMemoryDb();
            var sut = new DeleteReviewHandler(db);
            var cmd = new DeleteReviewCommand(1234567); // non-existing id

            var result = await sut.Handle(cmd, CancellationToken.None);

            result.Succeeded.Should().BeFalse();
            result.ErrorMessage.Should().Be("Nie znaleziono zaleceń.");
            (await db.Reviews.CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task Handle_DeletesReview_When_ItExists()
        {
            await using var db = CreateInMemoryDb();

            var review = Review.Create(
                pharmacistId: "pharm1",
                initialTxt: "Jakieś zalecenia",
                userRole: UserRoles.Pharmacist,
                number: "1234567890ABCDEF"
            );

            db.Reviews.Add(review);
            await db.SaveChangesAsync();

            var sut = new DeleteReviewHandler(db);
            var cmd = new DeleteReviewCommand(review.Id);

            var result = await sut.Handle(cmd, CancellationToken.None);

            result.Succeeded.Should().BeTrue();
            result.ErrorMessage.Should().BeNullOrEmpty();

            (await db.Reviews.CountAsync()).Should().Be(0);
            var deleted = await db.Reviews.FirstOrDefaultAsync(r => r.Id == review.Id);
            deleted.Should().BeNull();
        }
        #endregion
    }
}
