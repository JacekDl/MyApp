using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Medicines.Commands;
using MyApp.Model;
using MyApp.Tests.Common;

namespace MyApp.Tests.Domain.Medicines
{
    public class DeleteMedicineHandlerTests : TestBase
    {
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-42)]
        public void Validator_Fails_When_Id_IsNotPositive(int id)
        {
            var validator = new DeleteMedicineValidator();

            var result = validator.TestValidate(new DeleteMedicineCommand(id));

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.PropertyName == nameof(DeleteMedicineCommand.Id) &&
                e.ErrorMessage.Contains("dodatnie"));
        }

        [Fact]
        public async Task Handle_DeletesMedicine_WhenItExists()
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

        [Fact]
        public async Task Handle_ReturnsError_WhenMedicineDoesNotExist()
        {
            await using var db = CreateInMemoryDb();

            var sut = new DeleteMedicineHandler(db);
            var cmd = new DeleteMedicineCommand(Id: 1234567);

            var result = await sut.Handle(cmd, CancellationToken.None);

            result.Succeeded.Should().BeFalse();
            result.ErrorMessage.Should().Be("Nie znaleziono leku.");

            (await db.Set<Medicine>().CountAsync()).Should().Be(0);
        }
    }
}
