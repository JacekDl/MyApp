using FluentAssertions;
using FluentValidation.TestHelper;
using MyApp.Domain.Dictionaries.Queries;
using MyApp.Model;
using MyApp.Tests.Common;

namespace MyApp.Tests.Domain.Dictionaries
{
    public  class GetDictionariesHandlerTests : TestBase
    {

        [Fact]
        public void Validator_Succeeds_For_DefaultQuery()
        {
            var validator = new GetDictionariesValidator();
            var query = new GetDictionariesQuery();

            var result = validator.TestValidate(query);

            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }
        [Fact]
        public async Task Handle_ReturnsEmptyDictionaries_WhenNoData()
        {
            await using var db = CreateInMemoryDb();
            var sut = new GetDictionariesHandler(db);
            var query = new GetDictionariesQuery();

            var result = await sut.Handle(query, CancellationToken.None);

            result.Succeeded.Should().BeTrue();
            result.ErrorMessage.Should().BeNullOrEmpty();
            result.Value.Should().NotBeNull();

            result.Value!.InstructionMap.Should().NotBeNull();
            result.Value.InstructionMap.Should().BeEmpty();

            result.Value.MedicineMap.Should().NotBeNull();
            result.Value.MedicineMap.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_ReturnsDictionaries_WithInstructionsAndMedicines()
        {
            await using var db = CreateInMemoryDb();

            db.Set<Instruction>().AddRange(
                new Instruction { Code = "1B", Text = "Jedna tabletka dwa razy dziennie." },
                new Instruction { Code = "1N", Text = "Jedna tabletka wieczorem." }
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

            dto.InstructionMap["1B"].Should().Be("Jedna tabletka dwa razy dziennie.");
            dto.InstructionMap["1N"].Should().Be("Jedna tabletka wieczorem.");

            dto.MedicineMap["para"].Should().Be("Paracetamol");
            dto.MedicineMap["ibu"].Should().Be("Ibuprofen");
        }

        [Fact]
        public async Task Handle_UsesCaseInsensitiveKeys_ForDictionaries()
        {
            await using var db = CreateInMemoryDb();

            db.Set<Instruction>().Add(
                new Instruction { Code = "1N", Text = "Instrukcja z różną wielkością znaków." }
            );

            db.Set<Medicine>().Add(
                new Medicine { Code = "PaRa", Name = "Lek z różną wielkością znaków." }
            );

            await db.SaveChangesAsync();

            var sut = new GetDictionariesHandler(db);
            var query = new GetDictionariesQuery();

            var result = await sut.Handle(query, CancellationToken.None);

            result.Succeeded.Should().BeTrue();
            result.ErrorMessage.Should().BeNullOrEmpty();
            result.Value.Should().NotBeNull();

            var dto = result.Value!;

            dto.InstructionMap.ContainsKey("1N").Should().BeTrue();
            dto.MedicineMap.ContainsKey("PARA").Should().BeTrue();

            dto.InstructionMap["1N"].Should().Be("Instrukcja z różną wielkością znaków.");
            dto.MedicineMap["PARA"].Should().Be("Lek z różną wielkością znaków.");
        }
    }
}
