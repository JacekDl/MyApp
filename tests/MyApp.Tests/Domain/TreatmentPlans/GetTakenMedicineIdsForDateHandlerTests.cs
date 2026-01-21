using FluentAssertions;
using MyApp.Domain.TreatmentPlans.Queries;
using MyApp.Tests.Common;

namespace MyApp.Tests.Domain.TreatmentPlans;

public class GetTakenMedicineIdsForDateHandlerTests : TestBase
{
    [Fact]
    public async Task ReturnsEmptySet()
    {
        await using var db = CreateInMemoryDb();
        var sut = new GetTakenMedicineIdsForDateHandler(db);

        var query = new GetTakenMedicineIdsForDateQuery(
            IdPatient: "p1",
            Date: DateTime.Today
        );

        var result = await sut.Handle(query, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
    }
}
