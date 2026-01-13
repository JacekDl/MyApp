using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Medicines.Queries;
using MyApp.Model;
using MyApp.Tests.Common;

namespace MyApp.Tests.Domain.Medicines
{
    public class GetMedicinesHandlerTests : TestBase
    {
        [Fact]
        public async Task ReturnsEmptyList_NoMedicinesExist()
        {
            await using var db = CreateInMemoryDb();

            var sut = new GetMedicinesQueryHandler(db);
            var query = new GetMedicinesQuery();

            var result = await sut.Handle(query, CancellationToken.None);

            result.Succeeded.Should().BeTrue();
            result.ErrorMessage.Should().BeNullOrEmpty();
            result.Value.Should().NotBeNull().And.BeEmpty();

            result.TotalCount.Should().Be(0);
            result.Page.Should().Be(1);
            result.PageSize.Should().Be(10);
        }

        [Fact]
        public async Task ReturnsMedicineList()
        {
            await using var db = CreateInMemoryDb();

            var m1 = new Medicine { Code = "ZZZ", Name = "Ostatni" };
            var m2 = new Medicine { Code = "AAA", Name = "Pierwszy" };
            var m3 = new Medicine { Code = "KOD", Name = "Środkowy" };
            var m4 = new Medicine { Code = "KOD", Name = "Środkowy 2" };

            db.Medicines.AddRange(m1, m2, m3, m4);
            await db.SaveChangesAsync();

            var sut = new GetMedicinesQueryHandler(db);
            var query = new GetMedicinesQuery(Page: 1, PageSize: 10);

            var result = await sut.Handle(query, CancellationToken.None);

            result.Succeeded.Should().BeTrue();
            result.Value.Should().HaveCount(4);

            var expectedIds = await db.Medicines
                .AsNoTracking()
                .OrderBy(m => m.Code)
                .ThenBy(m => m.Id)
                .Select(m => m.Id)
                .ToListAsync();

            result.Value!.Select(m => m.Id).Should().ContainInOrder(expectedIds);

            result.TotalCount.Should().Be(4);
            result.Page.Should().Be(1);
            result.PageSize.Should().Be(10);
        }
    }
}
