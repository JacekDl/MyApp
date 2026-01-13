using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Medicines.Commands;
using MyApp.Model;
using MyApp.Tests.Common;

namespace MyApp.Tests.Domain.Medicines;

public class AddMedicineHandlerTests : TestBase
{
    [Fact]
    public async Task ReturnsError_CodeAlreadyExists()
    {
        await using var db = CreateInMemoryDb();

        var (seedCode, seedName) = FormatStringHelper.FormatCodeAndText("PARA", "Paracetamol");
        db.Add(new Medicine { Code = seedCode, Name = seedName });
        await db.SaveChangesAsync();

        var sut = new AddMedicineHandler(db);

        var cmd = new AddMedicineCommand("  para  ", "PARACETAMOL");

        var result = await sut.Handle(cmd, CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Be($"Kod '{seedCode}' jest już używany.");

        (await db.Set<Medicine>().CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task AddsMedicine_CodeDoesNotExist()
    {
        await using var db = CreateInMemoryDb();

        var cmd = new AddMedicineCommand("  ibu  ", "  ibuprofen  ");
        var (expectedCode, expectedName) =
            FormatStringHelper.FormatCodeAndText(cmd.Code, cmd.Name);

        var sut = new AddMedicineHandler(db);

        var result = await sut.Handle(cmd, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.ErrorMessage.Should().BeNullOrEmpty();

        var saved = await db.Set<Medicine>().SingleAsync();
        saved.Code.Should().Be(expectedCode);
        saved.Name.Should().Be(expectedName);
    }
}
