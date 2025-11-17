using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Instructions.Commands;
using MyApp.Model;
using MyApp.Tests.Common;
namespace MyApp.Tests.Domain.Instructions
{
    public class DeleteInstructionHandlerTests : TestBase
    {
        #region Validator
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-5)]
        public void Validator_Fails_When_Id_IsNotPositive(int id)
        {
            var validator = new DeleteInstructionValidator();
            var cmd = new DeleteInstructionCommand(id);

            var result = validator.TestValidate(cmd);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.PropertyName == nameof(DeleteInstructionCommand.Id) &&
                e.ErrorMessage.Contains("Id dawkowania musi być dodatnie."));
        }

        [Fact]
        public void Validator_Succeeds_For_Positive_Id()
        {
            var validator = new DeleteInstructionValidator();
            var cmd = new DeleteInstructionCommand(1);

            var result = validator.TestValidate(cmd);

            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        #endregion

        #region Handler
        [Fact]
        public async Task Handle_ReturnsError_WhenInstructionDoesNotExist()
        {
            await using var db = CreateInMemoryDb();

            var sut = new DeleteInstructionHandler(db);
            var cmd = new DeleteInstructionCommand(Id: 12345567);

            var result = await sut.Handle(cmd, CancellationToken.None);

            result.Succeeded.Should().BeFalse();
            result.ErrorMessage.Should().Be("Nie znaleziono dawkowania.");

            (await db.Set<Instruction>().CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task Handle_DeletesInstruction_WhenItExists()
        {
            await using var db = CreateInMemoryDb();

            var instruction = new Instruction
            {
                Code = "1T",
                Text = "Jedna trzy razy dziennie"
            };

            db.Add(instruction);
            await db.SaveChangesAsync();

            var sut = new DeleteInstructionHandler(db);
            var cmd = new DeleteInstructionCommand(instruction.Id);

            var result = await sut.Handle(cmd, CancellationToken.None);

            result.Succeeded.Should().BeTrue();
            result.ErrorMessage.Should().BeNullOrEmpty();

            (await db.Set<Instruction>().CountAsync()).Should().Be(0);
        }
        #endregion
    }
}
