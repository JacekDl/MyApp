using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Instructions.Queries;
using MyApp.Model;
using MyApp.Tests.Common;

namespace MyApp.Tests.Domain.Instructions;

public class GetInstructionsHandlerTests : TestBase
{

    [Fact]
    public async Task ReturnsEmptyList_NoInstructions()
    {
        await using var db = CreateInMemoryDb();

        var sut = new GetInstructionsHandler(db);
        var query = new GetInstructionsQuery();

        var result = await sut.Handle(query, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.ErrorMessage.Should().BeNullOrEmpty();
        result.Value.Should().NotBeNull().And.BeEmpty();

        result.TotalCount.Should().Be(0);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task ReturnsInstructionList()
    {
        await using var db = CreateInMemoryDb();

        var i1 = new Instruction { Code = "ZZZ", Text = "Ostatnia" };
        var i2 = new Instruction { Code = "AAA", Text = "Pierwsza" };
        var i3 = new Instruction { Code = "KOD", Text = "Środkowa" };
        var i4 = new Instruction { Code = "KOD", Text = "Środkowa 2" };

        db.Instructions.AddRange(i1, i2, i3, i4);
        await db.SaveChangesAsync();

        var sut = new GetInstructionsHandler(db);
        var query = new GetInstructionsQuery(Page: 1, PageSize: 10);

        var result = await sut.Handle(query, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.ErrorMessage.Should().BeNullOrEmpty();
        result.Value.Should().NotBeNull();
        result.Value!.Should().HaveCount(4);

        var expectedIds = await db.Instructions
            .AsNoTracking()
            .OrderBy(x => x.Code)
            .ThenBy(x => x.Id)
            .Select(x => x.Id)
            .ToListAsync();

        result.Value.Select(x => x.Id).Should().ContainInOrder(expectedIds);

        result.TotalCount.Should().Be(4);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }
}
