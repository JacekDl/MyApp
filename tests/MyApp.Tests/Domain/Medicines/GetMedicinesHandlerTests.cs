using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Medicines.Queries;
using MyApp.Model;
using MyApp.Tests.Common;

namespace MyApp.Tests.Domain.Medicines
{
    public class GetMedicinesHandlerTests : TestBase
    {
        #region Validator
        [Fact]
        public void Validator_Succeeds_For_DefaultQuery()
        {
            var validator = new GetMedicinesValidator();
            var query = new GetMedicinesQuery();

            var result = validator.TestValidate(query);

            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }
        #endregion

        #region Handler
        [Fact]
        public async Task Handle_ReturnsEmptyList_WhenNoMedicinesExist()
        {
            await using var db = CreateInMemoryDb();

            var sut = new GetMedicinesQueryHandler(db);
            var query = new GetMedicinesQuery();

            var result = await sut.Handle(query, CancellationToken.None);

            result.Succeeded.Should().BeTrue();
            result.ErrorMessage.Should().BeNullOrEmpty();
            result.Value.Should().NotBeNull();
            result.Value!.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_ReturnsOrderedListOfMedicines_WhenTheyExist()
        {
            await using var db = CreateInMemoryDb();

            var med1 = new Medicine { Code = "ZZZ", Name = "Ostatni" };
            var med2 = new Medicine { Code = "AAA", Name = "Pierwszy" };
            var med3 = new Medicine { Code = "KOD", Name = "Środkowy" };

            db.Medicines.AddRange(med1, med2, med3);
            await db.SaveChangesAsync();

            var sut = new GetMedicinesQueryHandler(db);
            var query = new GetMedicinesQuery();

            var result = await sut.Handle(query, CancellationToken.None);

            result.Succeeded.Should().BeTrue();
            result.ErrorMessage.Should().BeNullOrEmpty();
            result.Value.Should().NotBeNull();
            result.Value!.Should().HaveCount(3);

            var list = result.Value!.ToList();

            list[0].Code.Should().Be("AAA");
            list[0].Name.Should().Be("Pierwszy");

            list[1].Code.Should().Be("KOD");
            list[1].Name.Should().Be("Środkowy");

            list[2].Code.Should().Be("ZZZ");
            list[2].Name.Should().Be("Ostatni");

            var idsFromDb = await db.Medicines
                .OrderBy(m => m.Code)
                .Select(m => m.Id)
                .ToListAsync();

            list.Select(m => m.Id).Should().ContainInOrder(idsFromDb);
        }
        #endregion
    }
}
