using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.TreatmentPlans.Queries;
using MyApp.Model;
using MyApp.Model.enums;
using MyApp.Tests.Common;

namespace MyApp.Tests.Domain.TreatmentPlans;

public class GetTreatmentPlanHandlerTests : TestBase
{
    [Fact]
    public async Task ReturnsError_PlanNotFound()
    {
        await using var db = CreateInMemoryDb();
        var sut = new GetTreatmentPlanHandler(db);

        var query = new GetTreatmentPlanQuery("ABCDEF1234567890", "u1");

        var result = await sut.Handle(query, CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Be("Nie znaleziono planu leczenia.");
    }

    [Fact]
    public async Task ClearsUnreadForPharmacist()
    {
        await using var db = CreateInMemoryDb();

        var plan = new TreatmentPlan
        {
            Number = "ABCDEF1234567890",
            IdPharmacist = "ph1",
            IdPatient = "pt1",
            Status = TreatmentPlanStatus.Claimed,
            DateCreated = DateTime.UtcNow,
            AdviceFullText = "Advice",
            Review = new TreatmentPlanReview
            {
                UnreadForPharmacist = true,
                UnreadForPatient = true,
                ReviewEntries = new List<ReviewEntry>()
            }
        };

        db.TreatmentPlans.Add(plan);
        await db.SaveChangesAsync();

        var sut = new GetTreatmentPlanHandler(db);

        var query = new GetTreatmentPlanQuery(plan.Number, "ph1");

        var result = await sut.Handle(query, CancellationToken.None);

        result.Succeeded.Should().BeTrue();

        var reloaded = await db.TreatmentPlans
            .Include(p => p.Review)
            .SingleAsync();

        reloaded.Review!.UnreadForPharmacist.Should().BeFalse();
        reloaded.Review.UnreadForPatient.Should().BeTrue();
    }
}
