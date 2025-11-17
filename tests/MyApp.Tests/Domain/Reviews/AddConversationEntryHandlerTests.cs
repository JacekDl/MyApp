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
    public class AddConversationEntryHandlerTests : TestBase
    {
        #region Validator
        [Theory]
        [InlineData("", "user1", "Hello", "Numer zalecenia jest wymagany.")]
        [InlineData("SHORT", "user1", "Hello", "Numer zalecenia musi mieć długość 16 znaków.")]
        [InlineData("INVALID!!INVALID", "user1", "Hello", "Numer zalecenia może zawierać jedynie litery i cyfry.")]
        public void Validator_Fails_For_Invalid_Number(string number, string userId, string text, string expectedMessagePart)
        {
            var validator = new AddConversationEntryValidator();
            var cmd = new AddConversationEntryCommand(number, userId, text);

            var result = validator.TestValidate(cmd);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains(expectedMessagePart));
        }

        [Fact]
        public void Validator_Fails_When_RequestingUserId_IsEmpty()
        {
            var validator = new AddConversationEntryValidator();
            var cmd = new AddConversationEntryCommand("1234567890ABCDEF", "", "Hello");

            var result = validator.TestValidate(cmd);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Id użytkownika jest wymagane."));
        }

        [Fact]
        public void Validator_Fails_When_Text_IsNull()
        {
            var validator = new AddConversationEntryValidator();
            string? text = null;
            var cmd = new AddConversationEntryCommand("1234567890ABCDEF", "user1", text!);

            var result = validator.TestValidate(cmd);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Wiadomość nie może być pusta."));
        }

        [Fact]
        public void Validator_Fails_When_Text_IsWhitespace()
        {
            var validator = new AddConversationEntryValidator();
            var cmd = new AddConversationEntryCommand("1234567890ABCDEF", "user1", "   ");

            var result = validator.TestValidate(cmd);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Wiadomość nie może być pusta."));
        }

        [Fact]
        public void Validator_Fails_When_Text_TooLong()
        {
            var validator = new AddConversationEntryValidator();
            var longText = new string('x', 201); // > MaxTextLength (200)
            var cmd = new AddConversationEntryCommand("1234567890ABCDEF", "user1", longText);

            var result = validator.TestValidate(cmd);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Długość wiadomości nie może być dłuższa niż 200 znaków."));
        }

        [Fact]
        public void Validator_Succeeds_For_ValidData()
        {
            var validator = new AddConversationEntryValidator();
            var cmd = new AddConversationEntryCommand("1234567890ABCDEF", "user1", "Hello");

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
            var userManager = CreateUserManager(user: null, isAdmin: false);
            var sut = new AddConversationEntryHandler(db, userManager);

            var cmd = new AddConversationEntryCommand("", "user1", "Hello");

            var result = await sut.Handle(cmd, CancellationToken.None);

            result.Succeeded.Should().BeFalse();
            result.ErrorMessage.Should().NotBeNullOrWhiteSpace();
            result.ErrorMessage.Should().Contain("Numer zalecenia jest wymagany.");
            (await db.Set<Entry>().CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task Handle_ReturnsError_WhenReviewNotFound()
        {
            await using var db = CreateInMemoryDb();
            var userManager = CreateUserManager(user: null, isAdmin: false);
            var sut = new AddConversationEntryHandler(db, userManager);

            var cmd = new AddConversationEntryCommand("1234567890ABCDEF", "user1", "Hello");

            var result = await sut.Handle(cmd, CancellationToken.None);

            result.Succeeded.Should().BeFalse();
            result.ErrorMessage.Should().Be("Nie znaleziono zaleceń.");
            (await db.Set<Entry>().CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task Handle_ReturnsError_WhenUserIsNotParticipantAndNotAdmin()
        {
            await using var db = CreateInMemoryDb();

            var review = new Review
            {
                Number = "1234567890ABCDEF",
                PharmacistId = "pharm1",
                PatientId = "pat1",
                Entries = new List<Entry>(),
                PharmacistModified = false,
                PatientModified = false
            };

            db.Add(review);
            await db.SaveChangesAsync();

            var userManager = CreateUserManager(user: null, isAdmin: false);
            var sut = new AddConversationEntryHandler(db, userManager);

            var cmd = new AddConversationEntryCommand("1234567890ABCDEF", "otherUser", "Hello");

            var result = await sut.Handle(cmd, CancellationToken.None);

            result.Succeeded.Should().BeFalse();
            result.ErrorMessage.Should().Be("Brak dostępu.");
            review.Entries.Should().BeEmpty();
            review.PharmacistModified.Should().BeFalse();
            review.PatientModified.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_AddsEntry_WhenUserIsPharmacistParticipant()
        {
            await using var db = CreateInMemoryDb();

            var review = new Review
            {
                Number = "1234567890ABCDEF",
                PharmacistId = "pharm1",
                PatientId = "pat1",
                Entries = new List<Entry>(),
                PharmacistModified = false,
                PatientModified = false
            };

            db.Add(review);
            await db.SaveChangesAsync();

            var userManager = CreateUserManager(user: null, isAdmin: false);
            var sut = new AddConversationEntryHandler(db, userManager);

            var cmd = new AddConversationEntryCommand("1234567890ABCDEF", "pharm1", "  Hello world  ");

            var result = await sut.Handle(cmd, CancellationToken.None);

            result.Succeeded.Should().BeTrue();
            result.ErrorMessage.Should().BeNullOrEmpty();

            review.Entries.Should().HaveCount(1);
            var entry = review.Entries.Single();
            entry.UserId.Should().Be("pharm1");
            entry.Text.Should().Be("Hello world");
            entry.ReviewId.Should().Be(review.Id);
            entry.CreatedUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

            review.PharmacistModified.Should().BeTrue();
            review.PatientModified.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_AddsEntry_WhenUserIsAdminNonParticipant()
        {
            await using var db = CreateInMemoryDb();

            var review = new Review
            {
                Number = "1234567890ABCDEF",
                PharmacistId = "pharm1",
                PatientId = "pat1",
                Entries = new List<Entry>(),
                PharmacistModified = false,
                PatientModified = false
            };

            db.Add(review);
            await db.SaveChangesAsync();

            var adminUser = new User { Id = "admin1" };
            var userManager = CreateUserManager(user: adminUser, isAdmin: true);
            var sut = new AddConversationEntryHandler(db, userManager);

            var cmd = new AddConversationEntryCommand("1234567890ABCDEF", "admin1", "Admin message");

            var result = await sut.Handle(cmd, CancellationToken.None);

            result.Succeeded.Should().BeTrue();
            result.ErrorMessage.Should().BeNullOrEmpty();

            review.Entries.Should().HaveCount(1);
            var entry = review.Entries.Single();
            entry.UserId.Should().Be("admin1");
            entry.Text.Should().Be("Admin message");

            review.PharmacistModified.Should().BeTrue();
            review.PatientModified.Should().BeTrue();
        }
        #endregion
    }
}
