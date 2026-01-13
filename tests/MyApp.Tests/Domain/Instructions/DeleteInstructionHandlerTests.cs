using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Instructions.Commands;
using MyApp.Model;
using MyApp.Tests.Common;

namespace MyApp.Tests.Domain.Instructions;

public class DeleteInstructionHandlerTests : TestBase
{
    [Fact]
    public async Task ReturnsError_InstructionDoesNotExist()
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
    public async Task DeletesInstruction()
    {
        await using var db = CreateInMemoryDb();

        var instruction = new Instruction
        {
            Code = "1T",
            Text = "Jedna tabletka"
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

}
