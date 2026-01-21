using FluentAssertions;
using MyApp.Domain.TreatmentPlans.Queries;
using MyApp.Model;
using MyApp.Model.enums;
using MyApp.Tests.Common;

namespace MyApp.Tests.Domain.TreatmentPlans;

public class GetTreatmentPlanComplianceHandlerTests : TestBase
{
    [Fact]
    public async Task ReturnsError_PlanNotStartedYet()
    {
        await using var db = CreateInMemoryDb();

        var plan = new TreatmentPlan
        {
            Number = "ABCDEF1234567890",
            Status = TreatmentPlanStatus.Created,
            DateStarted = null,
            DateCreated = DateTime.UtcNow,
            AdviceFullText = "Advice"
        };

        db.TreatmentPlans.Add(plan);
        await db.SaveChangesAsync();

        var sut = new GetTreatmentPlanComplianceHandler(db);
        var query = new GetTreatmentPlanComplianceQuery(plan.Number);

        var result = await sut.Handle(query, CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Be("Plan leczenia nie został jeszcze rozpoczęty.");
    }

    [Fact]
    public async Task CalculatesCompliance()
    {
        await using var db = CreateInMemoryDb();

        var start = DateTime.UtcNow.Date.AddDays(-2);

        var plan = new TreatmentPlan
        {
            Number = "ABCDEF1234567890",
            Status = TreatmentPlanStatus.Started,
            DateStarted = start,
            DateCreated = DateTime.UtcNow,
            AdviceFullText = "Advice",
            Medicines = new List<TreatmentPlanMedicine>
        {
            new() { MedicineName = "Paracetamol", Dosage = "1 tab", TimeOfDay = TimeOfDay.Rano },
            new() { MedicineName = "Paracetamol", Dosage = "1 tab", TimeOfDay = TimeOfDay.Wieczor },
            new() { MedicineName = "Ibuprofen",   Dosage = "200 mg", TimeOfDay = TimeOfDay.Rano }
        }
        };

        db.TreatmentPlans.Add(plan);
        await db.SaveChangesAsync();

        db.AddRange(
            new MedicineTakenConfirmation
            {
                IdTreatmentPlanMedicine = plan.Medicines[0].Id,
                DateTimeTaken = start.AddHours(8)
            },
            new MedicineTakenConfirmation
            {
                IdTreatmentPlanMedicine = plan.Medicines[1].Id,
                DateTimeTaken = start.AddDays(1).AddHours(20)
            },
            new MedicineTakenConfirmation
            {
                IdTreatmentPlanMedicine = plan.Medicines[0].Id,
                DateTimeTaken = DateTime.UtcNow.AddMinutes(-1)
            },
            new MedicineTakenConfirmation
            {
                IdTreatmentPlanMedicine = plan.Medicines[2].Id,
                DateTimeTaken = start.AddHours(9)
            },
            new MedicineTakenConfirmation
            {
                IdTreatmentPlanMedicine = plan.Medicines[2].Id,
                DateTimeTaken = start.AddDays(1).AddHours(9)
            }
        );

        await db.SaveChangesAsync();

        var sut = new GetTreatmentPlanComplianceHandler(db);
        var query = new GetTreatmentPlanComplianceQuery(plan.Number);

        var result = await sut.Handle(query, CancellationToken.None);

        result.Succeeded.Should().BeTrue();

        var rows = result.Value!.Medicines;

        rows.Should().HaveCount(2);
        rows[0].MedicineName.Should().Be("Ibuprofen");
        rows[1].MedicineName.Should().Be("Paracetamol");

        rows.Single(x => x.MedicineName == "Paracetamol").Percentage.Should().Be(50.00m);
        rows.Single(x => x.MedicineName == "Ibuprofen").Percentage.Should().Be(66.67m);
    }
}
