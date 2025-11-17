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
    public class GetInstructionsHandlerTests : TestBase
    {
        #region Validator
        [Fact]
        public void Validator_Succeeds_For_DefaultQuery()
        {
            var validator = new GetInstructionsValidator();
            var query = new GetInstructionsQuery();

            var result = validator.TestValidate(query);

            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }
        #endregion

        #region Handler
        [Fact]
        public async Task Handle_ReturnsEmptyList_WhenNoInstructions()
        {
            await using var db = CreateInMemoryDb();

            var sut = new GetInstructionsHandler(db);
            var query = new GetInstructionsQuery();

            var result = await sut.Handle(query, CancellationToken.None);

            result.Succeeded.Should().BeTrue();
            result.ErrorMessage.Should().BeNullOrEmpty();
            result.Value.Should().NotBeNull();
            result.Value!.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_ReturnsOrderedListOfInstructions_WhenTheyExist()
        {
            await using var db = CreateInMemoryDb();

            var i1 = new Instruction { Code = "ZZZ", Text = "Ostatnia" };
            var i2 = new Instruction { Code = "AAA", Text = "Pierwsza" };
            var i3 = new Instruction { Code = "KOD", Text = "Środkowa" };

            db.Instructions.AddRange(i1, i2, i3);
            await db.SaveChangesAsync();

            var sut = new GetInstructionsHandler(db);
            var query = new GetInstructionsQuery();

            var result = await sut.Handle(query, CancellationToken.None);

            result.Succeeded.Should().BeTrue();
            result.ErrorMessage.Should().BeNullOrEmpty();
            result.Value.Should().NotBeNull();
            result.Value!.Should().HaveCount(3);

            var list = result.Value!.ToList();

            list[0].Code.Should().Be("AAA");
            list[0].Text.Should().Be("Pierwsza");

            list[1].Code.Should().Be("KOD");
            list[1].Text.Should().Be("Środkowa");

            list[2].Code.Should().Be("ZZZ");
            list[2].Text.Should().Be("Ostatnia");

            var idsFromDb = await db.Instructions
                .OrderBy(i => i.Code)
                .Select(i => i.Id)
                .ToListAsync();

            list.Select(d => d.Id).Should().ContainInOrder(idsFromDb);
        }
        #endregion
    }
}
