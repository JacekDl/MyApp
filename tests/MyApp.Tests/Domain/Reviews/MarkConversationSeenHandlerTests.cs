using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Reviews.Commands;
using MyApp.Model;
using MyApp.Tests.Common;
using static MyApp.Domain.Reviews.Commands.MarkConversationSeenHandler;

namespace MyApp.Tests.Domain.Reviews
{
    public class MarkConversationSeenHandlerTests : TestBase
    {
        #region Validator
        [Fact]
        public void Validator_Passes_On_ValidData()
        {
            var validator = new MarkConversationSeenValidator();
            var cmd = new MarkConversationSeenCommand(
                Number: "1234567890ABCDEF",
                UserId: "user-1"
            );

            var result = validator.TestValidate(cmd);

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validator_Fails_When_Number_Is_Empty()
        {
            var validator = new MarkConversationSeenValidator();
            var cmd = new MarkConversationSeenCommand(
                Number: "",
                UserId: "user-1"
            );

            var result = validator.TestValidate(cmd);

            result.IsValid.Should().BeFalse();
            result.ShouldHaveValidationErrorFor(x => x.Number)
                  .WithErrorMessage("Numer rozmowy jest wymagany.");
        }

        [Fact]
        public void Validator_Fails_When_Number_Has_Wrong_Length()
        {
            var validator = new MarkConversationSeenValidator();

            var cmd = new MarkConversationSeenCommand(
                Number: "1234567890",
                UserId: "user-1"
            );

            var result = validator.TestValidate(cmd);

            result.IsValid.Should().BeFalse();
            result.ShouldHaveValidationErrorFor(x => x.Number)
                  .WithErrorMessage("Numer rozmowy musi mieć dokładnie 16 znaków.");
        }

        [Fact]
        public void Validator_Fails_When_UserId_Is_Empty()
        {
            var validator = new MarkConversationSeenValidator();

            var cmd = new MarkConversationSeenCommand(
                Number: "1234567890ABCDEF",
                UserId: ""
            );

            var result = validator.TestValidate(cmd);

            result.IsValid.Should().BeFalse();
            result.ShouldHaveValidationErrorFor(x => x.UserId)
                  .WithErrorMessage("Identyfikator użytkownika jest wymagany.");
        }
        #endregion

        #region Handler
        [Fact]
        public async Task Handle_ReturnsError_WhenValidationFails()
        {
            using var db = CreateInMemoryDb();
            var handler = new MarkConversationSeenHandler(db);

            var cmd = new MarkConversationSeenCommand(
                Number: "12345678",
                UserId: ""
            );

            var result = await handler.Handle(cmd, CancellationToken.None);

            result.ErrorMessage.Should().NotBeNullOrWhiteSpace();
            result.ErrorMessage.Should().Contain("Numer rozmowy");
            result.ErrorMessage.Should().Contain("Identyfikator użytkownika");
        }

        [Fact]
        public async Task Handle_ReturnsError_WhenReviewNotFound()
        {
            using var db = CreateInMemoryDb();
            var handler = new MarkConversationSeenHandler(db);

            var cmd = new MarkConversationSeenCommand(
                Number: "1234567890ABCDEF",
                UserId: "user-1"
            );

            var result = await handler.Handle(cmd, CancellationToken.None);

            result.ErrorMessage.Should().Be("Nie znaleziono zaleceń.");
        }

        [Fact]
        public async Task Handle_ReturnsError_WhenUserHasNoAccess()
        {
            using var db = CreateInMemoryDb();

            var user = new User
            {
                Id = "user-1",
                Role = "User"
            };

            var review = new Review
            {
                Id = 1,
                Number = "1234567890ABCDEF",
                PharmacistId = "pharmacist-1",
                PatientId = "patient-1",
                PharmacistModified = true,
                PatientModified = true
            };

            db.Users.Add(user);
            db.Reviews.Add(review);
            await db.SaveChangesAsync();

            var handler = new MarkConversationSeenHandler(db);

            var cmd = new MarkConversationSeenCommand(
                Number: review.Number,
                UserId: user.Id
            );

            var result = await handler.Handle(cmd, CancellationToken.None);

            result.ErrorMessage.Should().Be("Brak dostępu.");
        }

        [Fact]
        public async Task Handle_SetsPatientModifiedFalse_WhenPharmacistViews()
        {
            using var db = CreateInMemoryDb();

            var pharmacistId = "pharmacist-1";
            var patientId = "patient-1";

            var pharmacist = new User
            {
                Id = pharmacistId,
                Role = "Pharmacist"
            };

            var review = new Review
            {
                Id = 1,
                Number = "1234567890ABCDEF",
                PharmacistId = pharmacistId,
                PatientId = patientId,
                PharmacistModified = true,
                PatientModified = true
            };

            db.Users.Add(pharmacist);
            db.Reviews.Add(review);
            await db.SaveChangesAsync();

            var handler = new MarkConversationSeenHandler(db);

            var cmd = new MarkConversationSeenCommand(
                Number: review.Number,
                UserId: pharmacistId
            );

            var result = await handler.Handle(cmd, CancellationToken.None);

            result.ErrorMessage.Should().BeNullOrEmpty();

            var updated = await db.Reviews.SingleAsync(r => r.Id == review.Id);
            updated.PatientModified.Should().BeFalse();
            updated.PharmacistModified.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_SetsPharmacistModifiedFalse_WhenPatientViews()
        {
            using var db = CreateInMemoryDb();

            var pharmacistId = "pharmacist-1";
            var patientId = "patient-1";

            var patient = new User
            {
                Id = patientId,
                Role = "Patient"
            };

            var review = new Review
            {
                Id = 1,
                Number = "1234567890ABCDEF",
                PharmacistId = pharmacistId,
                PatientId = patientId,
                PharmacistModified = true,
                PatientModified = true
            };

            db.Users.Add(patient);
            db.Reviews.Add(review);
            await db.SaveChangesAsync();

            var handler = new MarkConversationSeenHandler(db);

            var cmd = new MarkConversationSeenCommand(
                Number: review.Number,
                UserId: patientId
            );

            var result = await handler.Handle(cmd, CancellationToken.None);

            result.ErrorMessage.Should().BeNullOrEmpty();

            var updated = await db.Reviews.SingleAsync(r => r.Id == review.Id);
            updated.PharmacistModified.Should().BeFalse();
            updated.PatientModified.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_AllowsAdminEvenIfNotParticipant()
        {
            using var db = CreateInMemoryDb();

            var adminId = "admin-1";

            var admin = new User
            {
                Id = adminId,
                Role = "Admin"
            };

            var review = new Review
            {
                Id = 1,
                Number = "1234567890ABCDEF",
                PharmacistId = "pharmacist-1",
                PatientId = "patient-1",
                PharmacistModified = true,
                PatientModified = true
            };

            db.Users.Add(admin);
            db.Reviews.Add(review);
            await db.SaveChangesAsync();

            var handler = new MarkConversationSeenHandler(db);

            var cmd = new MarkConversationSeenCommand(
                Number: review.Number,
                UserId: adminId
            );

            var result = await handler.Handle(cmd, CancellationToken.None);

            result.ErrorMessage.Should().BeNullOrEmpty();

            var updated = await db.Reviews.SingleAsync(r => r.Id == review.Id);
            updated.PharmacistModified.Should().BeTrue();
            updated.PatientModified.Should().BeTrue();
        }
        #endregion

    }
}
