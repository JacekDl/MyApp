using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Medicines.Queries;
using MyApp.Model;
using MyApp.Tests.Common;

namespace MyApp.Tests.Domain.Medicines
{
    public class GetMedicineHandlerTests : TestBase
    {
        [Fact]
        public async Task ReturnsError_MedicineNotFound()
        {
            await using var db = CreateInMemoryDb();

            var sut = new GetMedicineHandler(db);
            var query = new GetMedicineQuery(1234567);

            var result = await sut.Handle(query, CancellationToken.None);

            result.Succeeded.Should().BeFalse();
            result.ErrorMessage.Should().Be("Nie znaleziono leku o podanym Id.");
            result.Value.Should().BeNull();
        }

        [Fact]
        public async Task ReturnsMedicineDto_MedicineExists()
        {
            await using var db = CreateInMemoryDb();

            var medicine = new Medicine
            {
                Code = "PARA",
                Name = "Paracetamol"
            };

            db.Add(medicine);
            await db.SaveChangesAsync();

            var sut = new GetMedicineHandler(db);
            var query = new GetMedicineQuery(medicine.Id);

            var result = await sut.Handle(query, CancellationToken.None);

            result.Succeeded.Should().BeTrue();
            result.ErrorMessage.Should().BeNullOrEmpty();
            result.Value.Should().NotBeNull();

            var dto = result.Value!;
            dto.Id.Should().Be(medicine.Id);
            dto.Code.Should().Be(medicine.Code);
            dto.Name.Should().Be(medicine.Name);

            (await db.Set<Medicine>().CountAsync()).Should().Be(1);
        }
    }
}
