using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Instructions.Commands;
using MyApp.Model;
using MyApp.Tests.Common;

namespace MyApp.Tests.Domain.Instructions;

public class AddInstructionHandlerTests : TestBase
{

    [Fact]
    public async Task ReturnsError_CodeAlreadyExists()
    {
        await using var db = CreateInMemoryDb();

        var (existingCode, existingText) =
            FormatStringHelper.FormatCodeAndText("1T", "Istniejąca instrukcja");

        db.Add(new Instruction { Code = existingCode, Text = existingText });
        await db.SaveChangesAsync();

        var sut = new AddInstructionHandler(db);

        var command = new AddInstructionCommand("  1T  ", "Nowa instrukcja");

        var result = await sut.Handle(command, CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Be($"Kod '{existingCode}' jest już używany.");

        (await db.Set<Instruction>().CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task AddsInstruction_WhenValidAndUnique()
    {
        await using var db = CreateInMemoryDb();
        var sut = new AddInstructionHandler(db);

        var command = new AddInstructionCommand("  1t  ", "   jedna tabletka  ");
        var (expectedCode, expectedText) = FormatStringHelper.FormatCodeAndText(command.Code, command.Text);

        var result = await sut.Handle(command, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.ErrorMessage.Should().BeNullOrEmpty();

        var instructions = await db.Set<Instruction>().AsNoTracking().ToListAsync();
        instructions.Should().HaveCount(1);

        instructions[0].Code.Should().Be(expectedCode);
        instructions[0].Text.Should().Be(expectedText);
    }
}