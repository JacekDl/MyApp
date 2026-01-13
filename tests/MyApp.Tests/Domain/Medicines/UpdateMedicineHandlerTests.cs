using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Medicines.Commands;
using MyApp.Model;
using MyApp.Tests.Common;

namespace MyApp.Tests.Domain.Medicines
{
    public class UpdateMedicineHandlerTests : TestBase
    {
        [Fact]
        public async Task DoesNotUpdate_CodeAlreadyUsedByAnotherMedicine()
        {
            await using var db = CreateInMemoryDb();

            var medA = new Medicine { Code = "PARA", Name = "Paracetamol" };

            var (codeB, nameB) =
                FormatStringHelper.FormatCodeAndText("IBU", "Ibuprofen");
            var medB = new Medicine { Code = codeB, Name = nameB };

            db.AddRange(medA, medB);
            await db.SaveChangesAsync();

            var sut = new UpdateMedicineHandler(db);

            var cmd = new UpdateMedicineCommand(
                Id: medA.Id,
                Code: "IBU",
                Name: "Nowa nazwa"
            );

            var result = await sut.Handle(cmd, CancellationToken.None);

            result.Succeeded.Should().BeFalse();
            result.ErrorMessage.Should().Be($"Kod '{codeB}' jest już używany.");

            var reloadedA = await db.Set<Medicine>().SingleAsync(m => m.Id == medA.Id);
            var reloadedB = await db.Set<Medicine>().SingleAsync(m => m.Id == medB.Id);

            reloadedA.Code.Should().Be("PARA");
            reloadedA.Name.Should().Be("Paracetamol");

            reloadedB.Code.Should().Be(codeB);
            reloadedB.Name.Should().Be(nameB);
        }

        [Fact]
        public async Task UpdatesMedicine_CodeIsUnique()
        {
            await using var db = CreateInMemoryDb();

            var existing = new Medicine
            {
                Code = "STARA",
                Name = "Stara nazwa"
            };

            db.Add(existing);
            await db.SaveChangesAsync();

            var sut = new UpdateMedicineHandler(db);

            var cmd = new UpdateMedicineCommand(
                Id: existing.Id,
                Code: "  nowa ",
                Name: " nowa nazwa leku   "
            );

            var (expectedCode, expectedName) =
                FormatStringHelper.FormatCodeAndText(cmd.Code, cmd.Name);

            var result = await sut.Handle(cmd, CancellationToken.None);

            result.Succeeded.Should().BeTrue();
            result.ErrorMessage.Should().BeNullOrEmpty();

            var saved = await db.Set<Medicine>().SingleAsync();
            saved.Id.Should().Be(existing.Id);
            saved.Code.Should().Be(expectedCode);
            saved.Name.Should().Be(expectedName);
        }
    }
}
