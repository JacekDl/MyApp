using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Instructions.Queries;
using MyApp.Model;
using MyApp.Tests.Common;

namespace MyApp.Tests.Domain.Instructions;

public class GetInstructionHandlerTests : TestBase
{
    [Fact]
    public async Task ReturnsError_InstructionNotFound()
    {
        await using var db = CreateInMemoryDb();
        var sut = new GetInstructionHandler(db);

        var query = new GetInstructionQuery(1234567);

        var result = await sut.Handle(query, CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Be("Nie znaleziono dawkowania o podanym Id.");
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task ReturnsInstructionDto_InstructionExists()
    {
        await using var db = CreateInMemoryDb();

        var instruction = new Instruction
        {
            Code = "1T",
            Text = "Jedna tabletka"
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

}
