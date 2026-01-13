using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Instructions.Commands;
using MyApp.Model;
using MyApp.Tests.Common;

namespace MyApp.Tests.Domain.Instructions;

public class UpdateInstructionHandlerTests : TestBase
{

    [Fact]
    public async Task DoesNotChange_CodeAlreadyUsedByAnotherInstruction()
    {
        await using var db = CreateInMemoryDb();

        var insA = new Instruction { Code = "1T", Text = "Jedna tabletka" };

        var (codeB, textB) =
            FormatStringHelper.FormatCodeAndText("2K", "Dwie kapsułki");
        var insB = new Instruction { Code = codeB, Text = textB };

        db.AddRange(insA, insB);
        await db.SaveChangesAsync();

        var sut = new UpdateInstructionHandler(db);

        var cmd = new UpdateInstructionCommand(
            Id: insA.Id,
            Code: "2K",
            Text: "Dwie kapsułeczki"
        );

        var result = await sut.Handle(cmd, CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Be($"Kod '{codeB}' jest już używany.");

        var reloadedA = await db.Set<Instruction>().SingleAsync(i => i.Id == insA.Id);
        var reloadedB = await db.Set<Instruction>().SingleAsync(i => i.Id == insB.Id);

        reloadedA.Code.Should().Be("1T");
        reloadedA.Text.Should().Be("Jedna tabletka");

        reloadedB.Code.Should().Be(codeB);
        reloadedB.Text.Should().Be(textB);
    }

    [Fact]
    public async Task UpdatesInstruction_CodeIsUnique()
    {
        await using var db = CreateInMemoryDb();

        var existing = new Instruction { Code = "stary", Text = "Stary tekst" };
        db.Add(existing);
        await db.SaveChangesAsync();

        var sut = new UpdateInstructionHandler(db);

        var cmd = new UpdateInstructionCommand(
            Id: existing.Id,
            Code: " nowy ",
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
}