using FluentAssertions;
using MyApp.Domain.Dictionaries.Queries;
using MyApp.Model;
using MyApp.Tests.Common;

namespace MyApp.Tests.Domain.Dictionaries;

public class GetDictionariesHandlerTests : TestBase
{
    [Fact]
    public async Task ReturnsEmptyDictionaries()
    {
        await using var db = CreateInMemoryDb();
        var sut = new GetDictionariesHandler(db);
        var query = new GetDictionariesQuery();

        var result = await sut.Handle(query, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.ErrorMessage.Should().BeNullOrEmpty();
        result.Value.Should().NotBeNull();

        result.Value!.InstructionMap.Should().NotBeNull().And.BeEmpty();
        result.Value.MedicineMap.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task ReturnsDictionaries()
    {
        await using var db = CreateInMemoryDb();

        db.Set<Instruction>().AddRange(
            new Instruction { Code = "1T", Text = "Jedna tabletka" },
            new Instruction { Code = "2K", Text = "Dwie kapsułki" }
        );

        db.Set<Medicine>().AddRange(
            new Medicine { Code = "PARA", Name = "Paracetamol" },
            new Medicine { Code = "IBU", Name = "Ibuprofen" }
        );

        await db.SaveChangesAsync();

        var sut = new GetDictionariesHandler(db);
        var query = new GetDictionariesQuery();

        var result = await sut.Handle(query, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.ErrorMessage.Should().BeNullOrEmpty();
        result.Value.Should().NotBeNull();

        var dto = result.Value!;

        dto.InstructionMap.Should().HaveCount(2);
        dto.MedicineMap.Should().HaveCount(2);

        dto.InstructionMap["1T"].Should().Be("Jedna tabletka");
        dto.InstructionMap["2K"].Should().Be("Dwie kapsułki");

        dto.MedicineMap["para"].Should().Be("Paracetamol");
        dto.MedicineMap["ibu"].Should().Be("Ibuprofen");
    }
}
