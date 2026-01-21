using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.TreatmentPlans.Commands;
using MyApp.Model;
using MyApp.Model.enums;
using MyApp.Tests.Common;

namespace MyApp.Tests.Domain.TreatmentPlans;

public class AddTreatmentPlanReviewEntryHandlerTests : TestBase
{
    [Fact]
    public async Task ReturnsError_TreatmentPlanNotFound()
    {
        await using var db = CreateInMemoryDb();
        var userManager = CreateUserManager();

        var sut = new AddTreatmentPlanReviewEntryHandler(db, userManager);

        var cmd = new AddTreatmentPlanReviewEntryCommand(
            Number: "ABCDEF1234567890",
            CurrentUserId: "u1",
            Text: "Wpis"
        );

        var result = await sut.Handle(cmd, CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Be("Nie znaleziono planu leczenia.");
    }

    [Fact]
    public async Task CreatesReviewAndEntry_AuthorIsPharmacist()
    {
        await using var db = CreateInMemoryDb();
        var userManager = CreateUserManager();

        var plan = new TreatmentPlan
        {
            Number = "ABCDEF1234567890",
            IdPharmacist = "ph1",
            IdPatient = "pt1",
            AdviceFullText = "Advice",
            Status = TreatmentPlanStatus.Claimed,
            DateCreated = DateTime.UtcNow
        };

        db.TreatmentPlans.Add(plan);
        await db.SaveChangesAsync();

        var sut = new AddTreatmentPlanReviewEntryHandler(db, userManager);

        var cmd = new AddTreatmentPlanReviewEntryCommand(
            Number: plan.Number,
            CurrentUserId: "ph1",
            Text: "Wiadomość od farmaceuty"
        );

        var result = await sut.Handle(cmd, CancellationToken.None);

        result.Succeeded.Should().BeTrue();

        var savedPlan = await db.TreatmentPlans
            .Include(tp => tp.Review)
            .ThenInclude(r => r!.ReviewEntries)
            .SingleAsync();

        savedPlan.Review!.UnreadForPatient.Should().BeTrue();
        savedPlan.Review.UnreadForPharmacist.Should().BeFalse();

        savedPlan.Review.ReviewEntries.Single().Author
            .Should().Be(ConversationParty.Pharmacist);
    }
}
