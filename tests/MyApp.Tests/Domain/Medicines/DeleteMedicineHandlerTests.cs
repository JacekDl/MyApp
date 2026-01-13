using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Medicines.Commands;
using MyApp.Model;
using MyApp.Tests.Common;

namespace MyApp.Tests.Domain.Medicines
{
    public class DeleteMedicineHandlerTests : TestBase
    {
        [Fact]
        public async Task ReturnsError_MedicineDoesNotExist()
        {
            await using var db = CreateInMemoryDb();
            var sut = new DeleteMedicineHandler(db);

            var cmd = new DeleteMedicineCommand(1234567);

            var result = await sut.Handle(cmd, CancellationToken.None);

            result.Succeeded.Should().BeFalse();
            result.ErrorMessage.Should().Be("Nie znaleziono leku.");

            (await db.Set<Medicine>().CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task DeletesMedicine()
        {
            await using var db = CreateInMemoryDb();

            var medicine = new Medicine
            {
                Code = "PARA",
                Name = "Paracetamol"
            };

            db.Add(medicine);
            await db.SaveChangesAsync();

            var sut = new DeleteMedicineHandler(db);
            var cmd = new DeleteMedicineCommand(medicine.Id);

            var result = await sut.Handle(cmd, CancellationToken.None);

            result.Succeeded.Should().BeTrue();
            result.ErrorMessage.Should().BeNullOrEmpty();

            (await db.Set<Medicine>().CountAsync()).Should().Be(0);
        }
    }
}
