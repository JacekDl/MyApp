using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.TreatmentPlans.Commands;
using MyApp.Model;
using MyApp.Model.enums;
using MyApp.Tests.Common;

namespace MyApp.Tests.Domain.TreatmentPlans;

public class UpdateTreatmentPlanStartHandlerTests : TestBase
{
    [Theory]
    [InlineData(TreatmentPlanStatus.Completed)]
    [InlineData(TreatmentPlanStatus.Expired)]
    public async Task ReturnsError_PlanIsCompletedOrExpired(TreatmentPlanStatus status)
    {
        await using var db = CreateInMemoryDb();

        var plan = new TreatmentPlan
        {
            Number = "ABCDEF1234567890",
            IdPharmacist = "ph1",
            IdPatient = "p1",
            Status = status,
            DateCreated = DateTime.UtcNow,
            AdviceFullText = "Advice"
        };

        db.TreatmentPlans.Add(plan);
        await db.SaveChangesAsync();

        var sut = new UpdateTreatmentPlanStartHandler(db);

        var cmd = new UpdateTreatmentPlanStartCommand(
            Number: plan.Number,
            IdPatient: "p1",
            DateStarted: DateTime.UtcNow.Date,
            DateCompleted: DateTime.UtcNow.Date.AddDays(1)
        );

        var result = await sut.Handle(cmd, CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Be("Nie można zmienić dat dla zakończonego lub wygasłego planu.");

        var reloaded = await db.TreatmentPlans.SingleAsync();
        reloaded.Status.Should().Be(status);
        reloaded.DateStarted.Should().BeNull();
        reloaded.DateCompleted.Should().BeNull();
    }

    [Fact]
    public async Task UpdatesDates_SetsStatusStarted()
    {
        await using var db = CreateInMemoryDb();

        var plan = new TreatmentPlan
        {
            Number = "ABCDEF1234567890",
            IdPharmacist = "ph1",
            IdPatient = "p1",
            Status = TreatmentPlanStatus.Created,
            DateCreated = DateTime.UtcNow,
            AdviceFullText = "Advice"
        };

        db.TreatmentPlans.Add(plan);
        await db.SaveChangesAsync();

        var sut = new UpdateTreatmentPlanStartHandler(db);

        var start = DateTime.UtcNow.Date.AddDays(2).AddHours(13).AddMinutes(45);
        var end = DateTime.UtcNow.Date.AddDays(10).AddHours(23);

        var cmd = new UpdateTreatmentPlanStartCommand(
            Number: plan.Number,
            IdPatient: "p1",
            DateStarted: start,
            DateCompleted: end
        );

        var result = await sut.Handle(cmd, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.ErrorMessage.Should().BeNullOrEmpty();

        var reloaded = await db.TreatmentPlans.SingleAsync();
        reloaded.Status.Should().Be(TreatmentPlanStatus.Started);
        reloaded.DateStarted.Should().Be(start.Date);
        reloaded.DateCompleted.Should().Be(end.Date);
    }
}
