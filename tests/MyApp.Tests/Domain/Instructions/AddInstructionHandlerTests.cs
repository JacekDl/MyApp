using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Instructions.Commands;
using MyApp.Model;
using MyApp.Tests.Common;

namespace MyApp.Tests.Domain.Instructions
{
    public class AddInstructionHandlerTests : TestBase
    {
        #region Validator
        [Theory]
        [InlineData("", "Jakaś treść", "Kod instrukcji jest wymagany.")]
        [InlineData("ABC", "", "Treść instrukcji jest wymagana.")]
        public void Validator_Fails_On_EmptyFields(string code, string text, string expectedMessagePart)
        {
            var validator = new AddInstructionValidator();
            var command = new AddInstructionCommand(code, text);

            var result = validator.TestValidate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains(expectedMessagePart));
        }

        [Fact]
        public void Validator_Fails_On_TooLongFields()
        {
            var validator = new AddInstructionValidator();
            var tooLongCode = new string('C', 33);   
            var tooLongText = new string('T', 257);

            var command = new AddInstructionCommand(tooLongCode, tooLongText);

            var result = validator.TestValidate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("32"));
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("256"));
        }

        [Fact]
        public void Validator_Succeeds_For_ValidValues()
        {
            var validator = new AddInstructionValidator();
            var command = new AddInstructionCommand("1T", "Jedna trzy razy dziennie.");

            var result = validator.TestValidate(command);

            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }
        #endregion

        #region Handler
        [Fact]
        public async Task Handle_ReturnsError_WhenValidationFails()
        {
            await using var db = CreateInMemoryDb();
            var sut = new AddInstructionHandler(db);

            var command = new AddInstructionCommand("", "");

            var result = await sut.Handle(command, CancellationToken.None);

            result.Succeeded.Should().BeFalse();
            result.ErrorMessage.Should().NotBeNullOrWhiteSpace();
            result.ErrorMessage!.Should().Contain("Kod instrukcji jest wymagany.");
            result.ErrorMessage.Should().Contain("Treść instrukcji jest wymagana.");

            (await db.Set<Instruction>().CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task Handle_ReturnsError_WhenCodeAlreadyExists()
        {
            await using var db = CreateInMemoryDb();

            var (code, text) = FormatStringHelper.FormatCodeAndText("PARA", "Istniejąca instrukcja");
            db.Add(new Instruction { Code = code, Text = text });
            await db.SaveChangesAsync();

            var sut = new AddInstructionHandler(db);

            var command = new AddInstructionCommand("  para  ", "Nowa instrukcja");

            var result = await sut.Handle(command, CancellationToken.None);

            result.Succeeded.Should().BeFalse();
            result.ErrorMessage.Should().Be($"Kod '{code}' jest już używany.");

            (await db.Set<Instruction>().CountAsync()).Should().Be(1);
        }

        [Fact]
        public async Task Handle_AddsInstruction_WhenValidAndUnique()
        {
            await using var db = CreateInMemoryDb();
            var sut = new AddInstructionHandler(db);

            var command = new AddInstructionCommand("  1t  ", "   jedna trzy razy dziennie.  ");
            var (expectedCode, expectedText) = FormatStringHelper.FormatCodeAndText(command.Code, command.Text);

            var result = await sut.Handle(command, CancellationToken.None);

            result.Succeeded.Should().BeTrue();
            result.ErrorMessage.Should().BeNullOrEmpty();

            var instructions = await db.Set<Instruction>().ToListAsync();
            instructions.Should().HaveCount(1);

            var instruction = instructions.Single();
            instruction.Code.Should().Be(expectedCode);
            instruction.Text.Should().Be(expectedText);
        }
        #endregion
    }
}
