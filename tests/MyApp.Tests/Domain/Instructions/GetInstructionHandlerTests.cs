using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Instructions.Commands;
using MyApp.Domain.Instructions.Queries;
using MyApp.Model;
using MyApp.Tests.Common;

namespace MyApp.Tests.Domain.Instructions
{
    public class GetInstructionHandlerTests : TestBase
    {
        #region Validator
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validator_Fails_When_Id_IsNotPositive(int id)
        {
            var validator = new GetInstructionValidator();
            var query = new GetInstructionQuery(id);

            var result = validator.TestValidate(query);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.PropertyName == nameof(GetInstructionQuery.Id) &&
                e.ErrorMessage.Contains("Id dawkowania musi być dodatnie."));
        }

        [Fact]
        public void Validator_Succeeds_For_Positive_Id()
        {
            var validator = new GetInstructionValidator();
            var query = new GetInstructionQuery(1);

            var result = validator.TestValidate(query);

            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }
        #endregion

        #region Handler
        [Fact]
        public async Task Handle_ReturnsError_WhenInstructionNotFound()
        {
            await using var db = CreateInMemoryDb();

            var sut = new GetInstructionHandler(db);
            var query = new GetInstructionQuery(1234567);

            var result = await sut.Handle(query, CancellationToken.None);

            result.Succeeded.Should().BeFalse();
            result.ErrorMessage.Should().NotBeNullOrWhiteSpace();
            result.Value.Should().BeNull();
        }

        [Fact]
        public async Task Handle_ReturnsInstructionDto_WhenInstructionExists()
        {
            await using var db = CreateInMemoryDb();

            var instruction = new Instruction
            {
                Code = "1T",
                Text = "Jedna trzy razy dziennie"
            };

            db.Add(instruction);
            await db.SaveChangesAsync();

            var sut = new GetInstructionHandler(db);
            var query = new GetInstructionQuery(instruction.Id);

            var result = await sut.Handle(query, CancellationToken.None);

            result.Succeeded.Should().BeTrue();
            result.ErrorMessage.Should().BeNullOrEmpty();
            result.Value.Should().NotBeNull();

            var dto = result.Value!;
            dto.Id.Should().Be(instruction.Id);
            dto.Code.Should().Be(instruction.Code);
            dto.Text.Should().Be(instruction.Text);

            (await db.Set<Instruction>().CountAsync()).Should().Be(1);
        }
        #endregion
    }
}
