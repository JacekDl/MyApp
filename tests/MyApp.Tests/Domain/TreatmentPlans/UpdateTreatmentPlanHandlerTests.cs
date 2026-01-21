using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.TreatmentPlans.Commands;
using MyApp.Model;
using MyApp.Model.enums;
using MyApp.Tests.Common;

namespace MyApp.Tests.Domain.TreatmentPlans;

public class UpdateTreatmentPlanHandlerTests : TestBase
{
    [Fact]
    public async Task ReturnsError_PlanAlreadyCompleted()
    {
        await using var db = CreateInMemoryDb();

        var plan = new TreatmentPlan
        {
            Number = "ABCDEF1234567890",
            IdPharmacist = "ph1",
            IdPatient = "p1",
            AdviceFullText = "Advice",
            Status = TreatmentPlanStatus.Completed,
            DateCreated = DateTime.UtcNow
        };

        db.TreatmentPlans.Add(plan);
        await db.SaveChangesAsync();

        var sut = new UpdateTreatmentPlanHandler(db);
        var cmd = new UpdateTreatmentPlanCommand(Number: plan.Number, ReviewText: "Uwagi");

        var result = await sut.Handle(cmd, CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Be("Nie można dodać uwag do zakończonego planu leczenia.");

        (await db.TreatmentPlanReviews.CountAsync()).Should().Be(0);
        (await db.Set<ReviewEntry>().CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task CreatesReview()
    {
        await using var db = CreateInMemoryDb();

        var plan = new TreatmentPlan
        {
            Number = "ABCDEF1234567890",
            IdPharmacist = "ph1",
            IdPatient = "p1",
            AdviceFullText = "Advice",
            Status = TreatmentPlanStatus.Claimed,
            DateCreated = DateTime.UtcNow,
            Review = null
        };

        db.TreatmentPlans.Add(plan);
        await db.SaveChangesAsync();

        var sut = new UpdateTreatmentPlanHandler(db);
        var cmd = new UpdateTreatmentPlanCommand(Number: plan.Number, ReviewText: "Moje uwagi");

        var before = DateTime.UtcNow;
        var result = await sut.Handle(cmd, CancellationToken.None);
        var after = DateTime.UtcNow;

        result.Succeeded.Should().BeTrue();
        result.ErrorMessage.Should().BeNullOrEmpty();

        var savedPlan = await db.TreatmentPlans
            .Include(tp => tp.Review)
            .ThenInclude(r => r!.ReviewEntries)
            .SingleAsync(tp => tp.Id == plan.Id);

        savedPlan.Status.Should().Be(TreatmentPlanStatus.Completed);
        savedPlan.Review.Should().NotBeNull();

        savedPlan.Review!.UnreadForPharmacist.Should().BeTrue();
        savedPlan.Review.UnreadForPatient.Should().BeFalse();

        savedPlan.Review.ReviewEntries.Should().HaveCount(1);
        var entry = savedPlan.Review.ReviewEntries.Single();

        entry.Author.Should().Be(ConversationParty.Patient);
        entry.Text.Should().Be("Moje uwagi");
        entry.DateCreated.Should().BeOnOrAfter(before).And.BeOnOrBefore(after.AddSeconds(5));
    }
}
