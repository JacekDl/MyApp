using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.TreatmentPlans.Commands;
using MyApp.Model;
using MyApp.Model.enums;
using MyApp.Tests.Common;

namespace MyApp.Tests.Domain.TreatmentPlans;

public class ClaimTreatmentPlanHandlerTests : TestBase
{
    [Fact]
    public async Task ReturnsError_PlanAlreadyClaimedOrFurther()
    {
        await using var db = CreateInMemoryDb();

        var plan = new TreatmentPlan
        {
            Number = "ABCDEF1234567890",
            IdPharmacist = "ph1",
            Status = TreatmentPlanStatus.Claimed,
            DateCreated = DateTime.UtcNow.AddDays(-1),
            AdviceFullText = "Advice"
        };

        db.TreatmentPlans.Add(plan);
        await db.SaveChangesAsync();

        var sut = new ClaimTreatmentPlanHandler(db);
        var cmd = new ClaimTreatmentPlanCommand(plan.Number, PatientId: "p1");

        var result = await sut.Handle(cmd, CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Be("Plan lecznia został już pobrany.");

        var reloaded = await db.TreatmentPlans.SingleAsync();
        reloaded.Status.Should().Be(TreatmentPlanStatus.Claimed);
        reloaded.IdPatient.Should().BeNull();
    }

    [Fact]
    public async Task ExpiresPlan_OlderThan30Days()
    {
        await using var db = CreateInMemoryDb();

        var plan = new TreatmentPlan
        {
            Number = "ABCDEF1234567890",
            IdPharmacist = "ph1",
            Status = TreatmentPlanStatus.Created,
            DateCreated = DateTime.UtcNow.AddDays(-31),
            AdviceFullText = "Advice"
        };

        db.TreatmentPlans.Add(plan);
        await db.SaveChangesAsync();

        var sut = new ClaimTreatmentPlanHandler(db);
        var cmd = new ClaimTreatmentPlanCommand(plan.Number, PatientId: "p1");

        var result = await sut.Handle(cmd, CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Be("Upłynął termin ważności planu lecznia.");

        var reloaded = await db.TreatmentPlans.SingleAsync();
        reloaded.Status.Should().Be(TreatmentPlanStatus.Expired);
        reloaded.IdPatient.Should().BeNull();
    }

    [Fact]
    public async Task ClaimsPlan()
    {
        await using var db = CreateInMemoryDb();

        var plan = new TreatmentPlan
        {
            Number = "ABCDEF1234567890",
            IdPharmacist = "ph1",
            Status = TreatmentPlanStatus.Created,
            DateCreated = DateTime.UtcNow.AddDays(-1),
            AdviceFullText = "Advice",
            IdPatient = null
        };

        db.TreatmentPlans.Add(plan);
        await db.SaveChangesAsync();

        var sut = new ClaimTreatmentPlanHandler(db);
        var cmd = new ClaimTreatmentPlanCommand(plan.Number, PatientId: "p1");

        var result = await sut.Handle(cmd, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.ErrorMessage.Should().BeNullOrEmpty();

        var reloaded = await db.TreatmentPlans.SingleAsync();
        reloaded.IdPatient.Should().Be("p1");
        reloaded.Status.Should().Be(TreatmentPlanStatus.Claimed);
    }
}
