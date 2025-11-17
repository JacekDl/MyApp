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
    public class ClaimReviewByPatientHandlerTests : TestBase
    {
        #region Validator
        [Fact]
        public void Validator_Fails_When_Number_IsEmpty()
        {
            var validator = new ClaimReviewByPatientCommandValidator();
            var cmd = new ClaimReviewByPatientCommand("", "patient1");

            var result = validator.TestValidate(cmd);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Numer jest wymagany."));
        }

        [Fact]
        public void Validator_Fails_When_Number_HasWrongLength()
        {
            var validator = new ClaimReviewByPatientCommandValidator();
            var cmd = new ClaimReviewByPatientCommand("1234567890ABCDEFXX", "patient1"); //18 znakow

            var result = validator.TestValidate(cmd);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Numer musi mieć 16 znaków."));
        }

        [Fact]
        public void Validator_Fails_When_Number_IsNotAlphanumeric()
        {
            var validator = new ClaimReviewByPatientCommandValidator();
            var cmd = new ClaimReviewByPatientCommand("1234-5678_90ABCD", "patient1");

            var result = validator.TestValidate(cmd);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Numer musi się składać z liter i cyfr."));
        }

        [Fact]
        public void Validator_Fails_When_PatientId_IsEmpty()
        {
            var validator = new ClaimReviewByPatientCommandValidator();
            var cmd = new ClaimReviewByPatientCommand("1234567890ABCDEF", "");

            var result = validator.TestValidate(cmd);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Id użytkownika jest wymagane."));
        }

        [Fact]
        public void Validator_Succeeds_For_ValidCommand()
        {
            var validator = new ClaimReviewByPatientCommandValidator();
            var cmd = new ClaimReviewByPatientCommand("1234567890ABCDEF", "patient1");

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
            var sut = new ClaimReviewByPatientHandler(db);

            var cmd = new ClaimReviewByPatientCommand("", "");

            var result = await sut.Handle(cmd, CancellationToken.None);

            result.Succeeded.Should().BeFalse();
            result.ErrorMessage.Should().NotBeNullOrWhiteSpace();
            result.ErrorMessage!.Should().Contain("Numer jest wymagany.");
            result.ErrorMessage.Should().Contain("Id użytkownika jest wymagane.");
            (await db.Reviews.CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task Handle_ReturnsError_WhenReviewNotFound()
        {
            await using var db = CreateInMemoryDb();
            var sut = new ClaimReviewByPatientHandler(db);

            var cmd = new ClaimReviewByPatientCommand("1234567890ABCDEF", "patient1");

            var result = await sut.Handle(cmd, CancellationToken.None);

            result.Succeeded.Should().BeFalse();
            result.ErrorMessage.Should().Be("Nie znaleziono zaleceń.");
        }

        [Fact]
        public async Task Handle_ReturnsError_WhenReviewAlreadyCompleted()
        {
            await using var db = CreateInMemoryDb();

            var review = new Review
            {
                Number = "1234567890ABCDEF",
                PatientId = null,
                Completed = true,
                DateCreated = DateTime.UtcNow
            };

            db.Reviews.Add(review);
            await db.SaveChangesAsync();

            var sut = new ClaimReviewByPatientHandler(db);
            var cmd = new ClaimReviewByPatientCommand("1234567890ABCDEF", "patient1");

            var result = await sut.Handle(cmd, CancellationToken.None);

            result.Succeeded.Should().BeFalse();
            result.ErrorMessage.Should().Be("Zalecenia zostały już pobrane.");
            review.PatientId.Should().BeNull();
        }

        [Fact]
        public async Task Handle_ReturnsError_WhenReviewExpired()
        {
            await using var db = CreateInMemoryDb();

            var review = new Review
            {
                Number = "1234567890ABCDEF",
                PatientId = null,
                Completed = false,
                DateCreated = DateTime.UtcNow.AddDays(-61)
            };

            db.Reviews.Add(review);
            await db.SaveChangesAsync();

            var sut = new ClaimReviewByPatientHandler(db);
            var cmd = new ClaimReviewByPatientCommand("1234567890ABCDEF", "patient1");

            var result = await sut.Handle(cmd, CancellationToken.None);

            result.Succeeded.Should().BeFalse();
            result.ErrorMessage.Should().Be("Upłynął termin ważności zaleceń.");
            review.PatientId.Should().BeNull();
        }

        [Fact]
        public async Task Handle_ReturnsError_WhenReviewAlreadyClaimedByOtherPatient()
        {
            await using var db = CreateInMemoryDb();

            var review = new Review
            {
                Number = "1234567890ABCDEF",
                PatientId = "otherPatient",
                Completed = false,
                DateCreated = DateTime.UtcNow
            };

            db.Reviews.Add(review);
            await db.SaveChangesAsync();

            var sut = new ClaimReviewByPatientHandler(db);
            var cmd = new ClaimReviewByPatientCommand("1234567890ABCDEF", "patient1");

            var result = await sut.Handle(cmd, CancellationToken.None);

            result.Succeeded.Should().BeFalse();
            result.ErrorMessage.Should().Be("Zalecenia zostały już użyte.");
            review.PatientId.Should().Be("otherPatient");
        }

        [Fact]
        public async Task Handle_Succeeds_WhenReviewUnclaimedAndValid()
        {
            await using var db = CreateInMemoryDb();

            var review = new Review
            {
                Number = "1234567890ABCDEF",
                PatientId = null,
                Completed = false,
                DateCreated = DateTime.UtcNow
            };

            db.Reviews.Add(review);
            await db.SaveChangesAsync();

            var sut = new ClaimReviewByPatientHandler(db);
            var cmd = new ClaimReviewByPatientCommand("1234567890ABCDEF", "patient1");

            var result = await sut.Handle(cmd, CancellationToken.None);

            result.Succeeded.Should().BeTrue();
            result.ErrorMessage.Should().BeNullOrEmpty();

            review.PatientId.Should().Be("patient1");
        }

        [Fact]
        public async Task Handle_Succeeds_WhenClaimedAgainBySamePatient()
        {
            await using var db = CreateInMemoryDb();

            var review = new Review
            {
                Number = "1234567890ABCDEF",
                PatientId = "patient1",
                Completed = false,
                DateCreated = DateTime.UtcNow
            };

            db.Reviews.Add(review);
            await db.SaveChangesAsync();

            var sut = new ClaimReviewByPatientHandler(db);
            var cmd = new ClaimReviewByPatientCommand("1234567890ABCDEF", "patient1");

            var result = await sut.Handle(cmd, CancellationToken.None);

            result.Succeeded.Should().BeTrue();
            result.ErrorMessage.Should().BeNullOrEmpty();

            review.PatientId.Should().Be("patient1");
        }
        #endregion
    }
}
