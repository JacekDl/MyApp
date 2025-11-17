using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Medicines.Queries;
using MyApp.Model;
using MyApp.Tests.Common;

namespace MyApp.Tests.Domain.Medicines
{
    public class GetMedicineHandlerTests : TestBase
    {
        #region Validator
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-10)]
        public void Validator_Fails_When_Id_IsNotPositive(int id)
        {
            var validator = new GetMedicineValidator();
            var query = new GetMedicineQuery(id);

            var result = validator.TestValidate(query);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.PropertyName == nameof(GetMedicineQuery.Id) &&
                e.ErrorMessage.Contains("Id leku musi być dodatnie."));
        }

        [Fact]
        public void Validator_Succeeds_For_Positive_Id()
        {
            var validator = new GetMedicineValidator();
            var query = new GetMedicineQuery(1);

            var result = validator.TestValidate(query);

            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }
        #endregion

        #region Handler
        [Fact]
        public async Task Handle_ReturnsError_WhenMedicineNotFound()
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
        public async Task Handle_ReturnsMedicineDto_WhenMedicineExists()
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
        #endregion
    }
}
