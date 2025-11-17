using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using MyApp.Domain.Reviews.Queries;
using MyApp.Model;
using MyApp.Tests.Common;
using Xunit;

namespace MyApp.Tests.Domain.Reviews
{
    public class GetConversationHandlerTests : TestBase
    {
        #region Validator
        [Fact]
        public void Validator_Fails_When_Number_IsEmpty()
        {
            var validator = new GetConversationHandler.GetConversationValidator();
            var query = new GetConversationQuery("", "user1");

            var result = validator.TestValidate(query);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "Numer tokenu jest wymagany.");
        }

        [Theory]
        [InlineData("1234")]
        [InlineData("12345678901234567")]
        public void Validator_Fails_When_Number_HasInvalidLength(string number)
        {
            var validator = new GetConversationHandler.GetConversationValidator();
            var query = new GetConversationQuery(number, "user1");

            var result = validator.TestValidate(query);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x =>
                x.ErrorMessage == "Numer tokenu musi mieć dokładnie 16 znaków.");
        }

        [Fact]
        public void Validator_Fails_When_Number_NotAlphanumeric()
        {
            var validator = new GetConversationHandler.GetConversationValidator();
            var query = new GetConversationQuery("1234_5678-90ABC!", "user1");

            var result = validator.TestValidate(query);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x =>
                x.ErrorMessage == "Numer tokenu może zawierać tylko litery i cyfry.");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Validator_Fails_When_RequestingUserId_Empty(string userId)
        {
            var validator = new GetConversationHandler.GetConversationValidator();
            var query = new GetConversationQuery("1234567890ABCDEF", userId);

            var result = validator.TestValidate(query);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x =>
                x.ErrorMessage == "Id użytkownika jest wymagane.");
        }

        [Fact]
        public void Validator_Succeeds_For_ValidData()
        {
            var validator = new GetConversationHandler.GetConversationValidator();
            var query = new GetConversationQuery("1234567890ABCDEF", "user1");

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
            var userManager = CreateUserManager(null, false);
            var sut = new GetConversationHandler(db, userManager);

            var query = new GetConversationQuery("", "");

            var result = await sut.Handle(query, CancellationToken.None);

            result.Succeeded.Should().BeFalse();
            result.ErrorMessage!.Should().Contain("Numer tokenu jest wymagany.");
            result.ErrorMessage.Should().Contain("Id użytkownika jest wymagane.");
        }

        [Fact]
        public async Task Handle_ReturnsError_WhenReviewNotFound()
        {
            await using var db = CreateInMemoryDb();
            var userManager = CreateUserManager(null, false);
            var sut = new GetConversationHandler(db, userManager);

            var query = new GetConversationQuery("1234567890ABCDEF", "user1");

            var result = await sut.Handle(query, CancellationToken.None);

            result.Succeeded.Should().BeFalse();
            result.ErrorMessage.Should().Be("Nie znaleziono zaleceń.");
        }

        [Fact]
        public async Task Handle_ReturnsError_WhenViewerIsNotParticipant_AndNotAdmin()
        {
            await using var db = CreateInMemoryDb();

            var review = Review.Create(
                pharmacistId: "pharm1",
                initialTxt: "Hello",
                number: "1234567890ABCDEF"
            );

            db.Reviews.Add(review);
            await db.SaveChangesAsync();

            var userManager = CreateUserManager(null, false);
            var sut = new GetConversationHandler(db, userManager);

            var query = new GetConversationQuery("1234567890ABCDEF", "stranger");

            var result = await sut.Handle(query, CancellationToken.None);

            result.Succeeded.Should().BeFalse();
            result.ErrorMessage.Should().Be("Brak dostępu.");
        }

        [Fact]
        public async Task Handle_ReturnsConversation_WhenViewerIsParticipant()
        {
            await using var db = CreateInMemoryDb();

            var review = Review.Create(
                pharmacistId: "user1",
                initialTxt: "Initial text",
                number: "1234567890ABCDEF"
            );

            db.Reviews.Add(review);
            await db.SaveChangesAsync();

            var entryUser = new User { Id = "user1", Role = "Pharmacist", DisplayName = "Adam Farmaceuta" };

            var userManager = CreateUserManager(entryUser, false);
            var sut = new GetConversationHandler(db, userManager);

            var query = new GetConversationQuery("1234567890ABCDEF", "user1");

            var result = await sut.Handle(query, CancellationToken.None);

            result.Succeeded.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value!.Entries.Should().HaveCount(1);

            var entry = result.Value.Entries.Single();
            entry.DisplayName.Should().Be("Adam Farmaceuta");
        }

        [Fact]
        public async Task Handle_ReturnsConversation_WhenViewerIsAdmin()
        {
            await using var db = CreateInMemoryDb();

            var review = Review.Create(
                pharmacistId: "pharm1",
                initialTxt: "Initial text",
                number: "1234567890ABCDEF"
            );

            db.Reviews.Add(review);
            await db.SaveChangesAsync();

            var adminUser = new User { Id = "admin1", Role = "Admin" };

            var userManager = CreateUserManager(adminUser, true);
            var sut = new GetConversationHandler(db, userManager);

            var query = new GetConversationQuery("1234567890ABCDEF", "admin1");

            var result = await sut.Handle(query, CancellationToken.None);

            result.Succeeded.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value!.Entries.Should().HaveCount(1);

            var entry = result.Value.Entries.Single();
            entry.DisplayName.Should().Be("Admin");
        }

        [Fact]
        public async Task Handle_ResolvesDisplayNames_ForAllUserRoleTypes()
        {
            await using var db = CreateInMemoryDb();

            var review = Review.Create(
                pharmacistId: "ph1",
                initialTxt: "Hello",
                number: "1234567890ABCDEF"
            );

            review.Entries.Add(new Entry { UserId = "u1", Text = "A", CreatedUtc = DateTime.UtcNow.AddMinutes(1) });
            review.Entries.Add(new Entry { UserId = "u2", Text = "B", CreatedUtc = DateTime.UtcNow.AddMinutes(2) });
            review.Entries.Add(new Entry { UserId = null, Text = "C", CreatedUtc = DateTime.UtcNow.AddMinutes(3) });

            db.Reviews.Add(review);
            await db.SaveChangesAsync();

            var user1 = new User { Id = "u1", Role = "Pharmacist", DisplayName = "" };

            var user2 = new User { Id = "u2", Role = "Patient", DisplayName = "" };

            var userManager = CreateUserManagerForMultipleUsers(user1, user2);

            var sut = new GetConversationHandler(db, userManager);

            var query = new GetConversationQuery("1234567890ABCDEF", "ph1");

            var result = await sut.Handle(query, CancellationToken.None);

            var entries = result.Value!.Entries.ToList();

            entries.Count.Should().Be(4);

            entries[1].DisplayName.Should().Be("Farmaceuta");
            entries[2].DisplayName.Should().Be("Pacjent");
            entries[3].DisplayName.Should().Be("Pacjent");
        }
        #endregion
    }
}
