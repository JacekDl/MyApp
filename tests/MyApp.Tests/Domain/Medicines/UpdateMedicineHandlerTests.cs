using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Medicines.Commands;
using MyApp.Model;
using MyApp.Tests.Common;

namespace MyApp.Tests.Domain.Medicines
{
    public class UpdateMedicineHandlerTests : TestBase
    {
        #region Validator
        [Theory]
        [InlineData(1, "", "Nazwa", "Kod leku jest wymagany.")]
        [InlineData(1, "ABC", "", "Nazwa leku jest wymagana.")]
        public void Validator_Fails_On_EmptyFields(int id, string code, string name, string expectedMessagePart)
        {
            var validator = new UpdateMedicineValidator();

            var res = validator.TestValidate(new UpdateMedicineCommand(id, code, name));

            res.IsValid.Should().BeFalse();
            res.Errors.Should().Contain(e => e.ErrorMessage.Contains(expectedMessagePart));
        }

        [Fact]
        public void Validator_Fails_On_TooLongFields()
        {
            var validator = new UpdateMedicineValidator();

            var tooLongCode = new string('C', 33);
            var tooLongName = new string('N', 129);

            var res = validator.TestValidate(new UpdateMedicineCommand(1, tooLongCode, tooLongName));

            res.IsValid.Should().BeFalse();
            res.Errors.Should().Contain(e => e.ErrorMessage.Contains("32"));
            res.Errors.Should().Contain(e => e.ErrorMessage.Contains("128"));
        }

        [Fact]
        public void Validator_Succeeds_For_ValidValues()
        {
            var validator = new UpdateMedicineValidator();
            var cmd = new UpdateMedicineCommand(1, "PARA", "Paracetamol");

            var res = validator.TestValidate(cmd);

            res.IsValid.Should().BeTrue();
            res.Errors.Should().BeEmpty();
        }
        #endregion

        #region Handler
        [Fact]
        public async Task Handle_ReturnsError_WhenMedicineNotFound()
        {
            await using var db = CreateInMemoryDb();

            var sut = new UpdateMedicineHandler(db);
            var cmd = new UpdateMedicineCommand(
                Id: 1234567,          
                Code: "PARA",
                Name: "Paracetamol");

            var result = await sut.Handle(cmd, CancellationToken.None);

            result.Succeeded.Should().BeFalse();
            result.ErrorMessage.Should().Be("Nie znaleziono leku w bazie.");
        }

        [Fact]
        public async Task Handle_UpdatesMedicine_WhenCodeIsUnique()
        {
            await using var db = CreateInMemoryDb();

            var existing = new Medicine { Code = "PARA", Name = "Paracetamol" };
            db.Add(existing);
            await db.SaveChangesAsync();

            var sut = new UpdateMedicineHandler(db);

            var cmd = new UpdateMedicineCommand(
                existing.Id,
                Code: "ibu",          
                Name: "ibuprofen");   

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

        [Fact]
        public async Task Handle_DoesNotChange_WhenCodeAlreadyUsedByAnotherMedicine()
        {
            await using var db = CreateInMemoryDb();

            var medA = new Medicine { Code = "IBU", Name = "Ibuprofen" };

            var (codeB, nameB) = FormatStringHelper.FormatCodeAndText("PARA", "Paracetamol");
            var medB = new Medicine { Code = codeB, Name = nameB };

            db.AddRange(medA, medB);
            await db.SaveChangesAsync();

            var sut = new UpdateMedicineHandler(db);

            var cmd = new UpdateMedicineCommand(
                medA.Id,
                Code: "para",
                Name: "PARACETAMOL");

            var result = await sut.Handle(cmd, CancellationToken.None);

            result.Succeeded.Should().BeFalse();
            result.ErrorMessage.Should().NotBeNullOrWhiteSpace();
            result.ErrorMessage!.Should().Contain("Kod 'PARA' jest już używany.");

            var reloadedA = await db.Set<Medicine>().SingleAsync(m => m.Id == medA.Id);
            var reloadedB = await db.Set<Medicine>().SingleAsync(m => m.Id == medB.Id);

            reloadedA.Code.Should().Be("IBU");
            reloadedA.Name.Should().Be("Ibuprofen");

            reloadedB.Code.Should().Be(codeB);
            reloadedB.Name.Should().Be(nameB);
        }
        #endregion
    }
}
