using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Medicines.Commands;
using MyApp.Model;
using MyApp.Tests.Common;

namespace MyApp.Tests.Domain;

public class AddMedicineHandlerTests : TestBase
{
    [Theory]
    [InlineData("", "Nazwa", "Kod leku jest wymagany.")]
    [InlineData("ABC", "", "Nazwa leku jest wymagana.")]
    public void Validator_Fails_On_EmptyFields(string code, string name, string expectedMessagePart)
    {
        var validator = new AddMedicineValidator();

        var res = validator.TestValidate(new AddMedicineCommand(code, name));
        res.IsValid.Should().BeFalse();
        res.Errors.Should().Contain(e => e.ErrorMessage.Contains(expectedMessagePart));
    }

    [Fact]
    public void Validator_Fails_On_TooLongFields()
    {
        var validator = new AddMedicineValidator();

        var tooLongCode = new string('C', 33);
        var tooLongName = new string('N', 129);

        var res = validator.TestValidate(new AddMedicineCommand(tooLongCode, tooLongName));
        res.IsValid.Should().BeFalse();
        res.Errors.Should().Contain(e => e.ErrorMessage.Contains("32"));
        res.Errors.Should().Contain(e => e.ErrorMessage.Contains("128"));
    }

    [Fact]
    public async Task Handle_AddsMedicine_WhenCodeDoesNotExist()
    {
        await using var db = CreateInMemoryDb();

        var cmd = new AddMedicineCommand("ibu", "ibuprofen");
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

    [Fact]
    public async Task Handle_ReturnsError_WhenCodeAlreadyExists_AfterFormatting()
    {
        await using var db = CreateInMemoryDb();

        var (seedCode, seedName) = FormatStringHelper.FormatCodeAndText("PARA", "Paracetamol");
        db.Add(new Medicine { Code = seedCode, Name = seedName });
        await db.SaveChangesAsync();

        var cmd = new AddMedicineCommand("para", "PARACETAMOL");
        var sut = new AddMedicineHandler(db);

        var result = await sut.Handle(cmd, CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrWhiteSpace();
        (await db.Set<Medicine>().CountAsync()).Should().Be(1);
    }
}
