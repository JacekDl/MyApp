using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using MyApp.Domain.Reviews.Queries;
using MyApp.Domain.Users;
using MyApp.Model;
using MyApp.Tests.Common;
using Xunit;

namespace MyApp.Tests.Domain.Reviews
{
    public class GetReviewsHandlerTests : TestBase
    {
        #region Validator
        [Fact]
        public void Validator_Fails_When_SearchTxt_TooLong()
        {
            var validator = new GetReviewsHandler.GetReviewsValidator();
            var tooLong = new string('x', 101);
            var query = new GetReviewsQuery(tooLong, "user1", null, null);

            var result = validator.TestValidate(query);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.PropertyName == nameof(GetReviewsQuery.SearchTxt) &&
                e.ErrorMessage == "Tekst wyszukiwania nie może przekraczać 100 znaków.");
        }

        [Fact]
        public void Validator_Fails_When_CurrentUserId_IsWhitespace()
        {
            var validator = new GetReviewsHandler.GetReviewsValidator();
            var query = new GetReviewsQuery(null, "   ", null, "user@example.com");

            var result = validator.TestValidate(query);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.PropertyName == nameof(GetReviewsQuery.CurrentUserId) &&
                e.ErrorMessage == "Id użytkownika nie może być puste.");
        }

        [Fact]
        public void Validator_Fails_When_Email_Invalid()
        {
            var validator = new GetReviewsHandler.GetReviewsValidator();
            var query = new GetReviewsQuery(null, "user1", null, "invalid-email");

            var result = validator.TestValidate(query);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.PropertyName == nameof(GetReviewsQuery.UserEmail) &&
                e.ErrorMessage == "Nieprawidłowy adres e-mail.");
        }

        [Fact]
        public void Validator_Fails_When_No_UserId_And_No_Email()
        {
            var validator = new GetReviewsHandler.GetReviewsValidator();
            var query = new GetReviewsQuery(null, null, null, null);

            var result = validator.TestValidate(query);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.PropertyName == string.Empty &&
                e.ErrorMessage == "Należy podać Id użytkownika lub adres e-mail.");
        }

        [Fact]
        public void Validator_Succeeds_For_Valid_Query()
        {
            var validator = new GetReviewsHandler.GetReviewsValidator();
            var query = new GetReviewsQuery("abc", "user1", Completed: true, UserEmail: "user@example.com");

            var result = validator.TestValidate(query);

            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }
        #endregion

        #region Handler
        [Fact]
        public async Task Handle_ReturnsError_WhenValidationFails()
        {
            await using var db = CreateInMemoryDb();
            var userManager = CreateUserManager(null);
            var sut = new GetReviewsHandler(userManager, db);

            // invalid: no CurrentUserId and no UserEmail
            var query = new GetReviewsQuery(null, null, null, null);

            var result = await sut.Handle(query, CancellationToken.None);

            result.Succeeded.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Należy podać Id użytkownika lub adres e-mail.");
            result.Value.Should().BeNull();
        }

        [Fact]
        public async Task Handle_Filters_By_SearchTxt_On_FirstEntryText()
        {
            await using var db = CreateInMemoryDb();
            var userManager = CreateUserManager(null);

            var r1 = Review.Create("ph1", "Paracetamol 500 mg", "NUM000000000001", "Pharmacist");
            var r2 = Review.Create("ph1", "Ibuprofen 200 mg", "NUM000000000002", "Pharmacist");

            db.Reviews.AddRange(r1, r2);
            await db.SaveChangesAsync();

            var sut = new GetReviewsHandler(userManager, db);

            // Search only for "Para" -> matches first review only
            var query = new GetReviewsQuery("Para", "ph1", null, null);

            var result = await sut.Handle(query, CancellationToken.None);

            result.Succeeded.Should().BeTrue();
            result.ErrorMessage.Should().BeNullOrEmpty();
            result.Value.Should().NotBeNull();
            result.Value!.Should().HaveCount(1);
            result.Value![0].Number.Should().Be("NUM000000000001");
            result.Value![0].Text.Should().Contain("Paracetamol");
        }

        [Fact]
        public async Task Handle_Filters_By_CurrentUserId_As_Participant()
        {
            await using var db = CreateInMemoryDb();
            var userManager = CreateUserManager(null);

            var r1 = Review.Create("ph1", "First", "NUM000000000001", UserRoles.Pharmacist);
            var r2 = Review.Create("ph2", "Second", "NUM000000000002", UserRoles.Pharmacist);
            r2.PatientId = "user1";

            db.Reviews.AddRange(r1, r2);
            await db.SaveChangesAsync();

            var sut = new GetReviewsHandler(userManager, db);

            // CurrentUserId = "user1" -> should see only reviews where user is pharmacist or patient
            var query = new GetReviewsQuery(null, "user1", null, null);

            var result = await sut.Handle(query, CancellationToken.None);

            result.Succeeded.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value!.Should().HaveCount(1);
            result.Value![0].Number.Should().Be("NUM000000000002");
        }

        [Fact]
        public async Task Handle_Filters_By_Completed_Flag()
        {
            await using var db = CreateInMemoryDb();
            var userManager = CreateUserManager(null);

            var r1 = Review.Create("ph1", "First", "NUM000000000001", "Pharmacist");
            r1.Completed = false;

            var r2 = Review.Create("ph1", "Second", "NUM000000000002", "Pharmacist");
            r2.Completed = true;

            db.Reviews.AddRange(r1, r2);
            await db.SaveChangesAsync();

            var sut = new GetReviewsHandler(userManager, db);

            var queryCompleted = new GetReviewsQuery(null, "ph1", Completed: true, null);
            var resultCompleted = await sut.Handle(queryCompleted, CancellationToken.None);

            resultCompleted.Succeeded.Should().BeTrue();
            resultCompleted.Value!.Should().HaveCount(1);
            resultCompleted.Value![0].Number.Should().Be("NUM000000000002");

            var queryNotCompleted = new GetReviewsQuery(null, "ph1", Completed: false, null);
            var resultNotCompleted = await sut.Handle(queryNotCompleted, CancellationToken.None);

            resultNotCompleted.Succeeded.Should().BeTrue();
            resultNotCompleted.Value!.Should().HaveCount(1);
            resultNotCompleted.Value![0].Number.Should().Be("NUM000000000001");
        }

        [Fact]
        public async Task Handle_Uses_UserEmail_To_Compute_IsNewForViewer()
        {
            await using var db = CreateInMemoryDb();

            var r1 = Review.Create("ph1", "First", "NUM000000000001", "Pharmacist");
            r1.PatientModified = true;   // new for pharmacist
            r1.PharmacistModified = false;

            var r2 = Review.Create("ph1", "Second", "NUM000000000002", "Pharmacist");
            r2.PatientModified = false;
            r2.PharmacistModified = true; // new for patient, not pharmacist

            db.Reviews.AddRange(r1, r2);
            await db.SaveChangesAsync();

            var viewer = new User { Id = "ph1", Email = "ph1@example.com" };
            var userManager = CreateUserManager(viewer);

            var sut = new GetReviewsHandler(userManager, db);


            var query = new GetReviewsQuery(null, null, null, "ph1@example.com");

            var result = await sut.Handle(query, CancellationToken.None);

            result.Succeeded.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value!.Should().HaveCount(2);

            var list = result.Value!.ToList();
            list[0].Number.Should().Be("NUM000000000001");
            list[0].IsNewForViewer.Should().BeTrue();
            list[1].Number.Should().Be("NUM000000000002");
            list[1].IsNewForViewer.Should().BeFalse();
        }
        #endregion
    }
}
