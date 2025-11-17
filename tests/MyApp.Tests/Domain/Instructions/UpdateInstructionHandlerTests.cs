using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Instructions.Commands;
using MyApp.Model;
using MyApp.Tests.Common;

namespace MyApp.Tests.Domain.Instructions 
{
    public class UpdateInstructionHandlerTests : TestBase
    {
        #region Validator
        [Theory]
        [InlineData(1, "", "Tekst", "Kod instrukcji jest wymagany.")]
        [InlineData(1, "ABC", "", "Treść instrukcji jest wymagana.")]
        public void Validator_Fails_On_EmptyFields(int id, string code, string text, string expectedMessagePart)
        {
            var validator = new UpdateInstructionValidator();
            var cmd = new UpdateInstructionCommand(id, code, text);

            var res = validator.TestValidate(cmd);

            res.IsValid.Should().BeFalse();
            res.Errors.Should().Contain(e => e.ErrorMessage.Contains(expectedMessagePart));
        }

        [Fact]
        public void Validator_Fails_On_TooLongFields()
        {
            var validator = new UpdateInstructionValidator();
            var tooLongCode = new string('C', 33);   // > 32
            var tooLongText = new string('T', 257);  // > 256

            var cmd = new UpdateInstructionCommand(1, tooLongCode, tooLongText);

            var res = validator.TestValidate(cmd);

            res.IsValid.Should().BeFalse();
            res.Errors.Should().Contain(e => e.ErrorMessage.Contains("32"));
            res.Errors.Should().Contain(e => e.ErrorMessage.Contains("256"));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validator_Fails_On_NonPositiveId(int id)
        {
            var validator = new UpdateInstructionValidator();
            var cmd = new UpdateInstructionCommand(id, "1T", "Tekst");

            var res = validator.TestValidate(cmd);

            res.IsValid.Should().BeFalse();
            res.Errors.Should().Contain(e =>
                e.PropertyName == nameof(UpdateInstructionCommand.Id) &&
                e.ErrorMessage.Contains("Id dawkowania musi być dodatnie."));
        }

        [Fact]
        public void Validator_Succeeds_For_ValidValues()
        {
            var validator = new UpdateInstructionValidator();
            var cmd = new UpdateInstructionCommand(1, "1T", "Jedna trzy razy dziennie.");

            var res = validator.TestValidate(cmd);

            res.IsValid.Should().BeTrue();
            res.Errors.Should().BeEmpty();
        }

        #endregion

        #region Handler
        [Fact]
        public async Task Handle_ReturnsError_WhenInstructionNotFound()
        {
            await using var db = CreateInMemoryDb();

            var sut = new UpdateInstructionHandler(db);
            var cmd = new UpdateInstructionCommand(
                Id: 123,
                Code: "PARA",
                Text: "Nowy tekst");

            var result = await sut.Handle(cmd, CancellationToken.None);

            result.Succeeded.Should().BeFalse();
            result.ErrorMessage.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task Handle_UpdatesInstruction_WhenCodeIsUnique()
        {
            await using var db = CreateInMemoryDb();

            var existing = new Instruction { Code = "stary", Text = "Stary tekst" };
            db.Add(existing);
            await db.SaveChangesAsync();

            var sut = new UpdateInstructionHandler(db);

            var cmd = new UpdateInstructionCommand(
                existing.Id,
                Code: "nowy",                     
                Text: " nowy tekst instrukcji " 
            );

            var (expectedCode, expectedText) =
                FormatStringHelper.FormatCodeAndText(cmd.Code, cmd.Text);

            var result = await sut.Handle(cmd, CancellationToken.None);

            result.Succeeded.Should().BeTrue();
            result.ErrorMessage.Should().BeNullOrEmpty();

            var saved = await db.Set<Instruction>().SingleAsync();
            saved.Id.Should().Be(existing.Id);
            saved.Code.Should().Be(expectedCode);
            saved.Text.Should().Be(expectedText);
        }

        [Fact]
        public async Task Handle_DoesNotChange_WhenCodeAlreadyUsedByAnotherInstruction()
        {
            await using var db = CreateInMemoryDb();

            var insA = new Instruction { Code = "1T", Text = "Jedna trzy razy dziennie." };

            var (codeB, textB) =
                FormatStringHelper.FormatCodeAndText("1N", "Jedna wieczorem.");
            var insB = new Instruction { Code = codeB, Text = textB };

            db.AddRange(insA, insB);
            await db.SaveChangesAsync();

            var sut = new UpdateInstructionHandler(db);

            var cmd = new UpdateInstructionCommand(
                insA.Id,
                Code: "1N",
                Text: "Jedna na noc."
            );

            var result = await sut.Handle(cmd, CancellationToken.None);

            result.Succeeded.Should().BeFalse();
            result.ErrorMessage.Should().NotBeNullOrWhiteSpace();
            result.ErrorMessage!.Should().Contain("Kod '1N' jest już używany.");

            var reloadedA = await db.Set<Instruction>().SingleAsync(i => i.Id == insA.Id);
            var reloadedB = await db.Set<Instruction>().SingleAsync(i => i.Id == insB.Id);

            reloadedA.Code.Should().Be("1T");
            reloadedA.Text.Should().Be("Jedna trzy razy dziennie.");

            reloadedB.Code.Should().Be(codeB);
            reloadedB.Text.Should().Be(textB);
        }
        #endregion
    }
}
