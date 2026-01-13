using FluentAssertions;
using MyApp.Domain.TreatmentPlans.Queries;
using MyApp.Model;
using MyApp.Model.enums;
using MyApp.Tests.Common;

namespace MyApp.Tests.Domain.TreatmentPlans;

public class GetTreatmentPlanMedicinesHandlerTests : TestBase
{
    [Fact]
    public async Task ReturnsEmptyList_NoMatchingPlans()
    {
        await using var db = CreateInMemoryDb();
        var sut = new GetTreatmentPlanMedicinesHandler(db);

        var query = new GetTreatmentPlanMedicinesQuery(
            PatientId: "p1",
            Date: DateTime.Today
        );

        var result = await sut.Handle(query, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task ReturnsOnlyMedicines_ActiveOnGivenDay_ForPatient()
    {
        await using var db = CreateInMemoryDb();

        var day = DateTime.Today;

        var planActive = new TreatmentPlan
        {
            Number = "PLAN000000000001",
            IdPatient = "p1",
            IdPharmacist = "ph1",
            Status = TreatmentPlanStatus.Started,
            DateCreated = DateTime.UtcNow,
            DateStarted = day.AddDays(-1),
            DateCompleted = day.AddDays(2),
            AdviceFullText = "Advice"
        };

        var planInactive = new TreatmentPlan
        {
            Number = "PLAN000000000002",
            IdPatient = "p1",
            IdPharmacist = "ph1",
            Status = TreatmentPlanStatus.Completed,
            DateCreated = DateTime.UtcNow,
            DateStarted = day.AddDays(-5),
            DateCompleted = day.AddDays(-2),
            AdviceFullText = "Advice"
        };

        var planOtherPatient = new TreatmentPlan
        {
            Number = "PLAN000000000003",
            IdPatient = "p2",
            IdPharmacist = "ph1",
            Status = TreatmentPlanStatus.Started,
            DateCreated = DateTime.UtcNow,
            DateStarted = day.AddDays(-1),
            DateCompleted = day.AddDays(1),
            AdviceFullText = "Advice"
        };

        db.TreatmentPlans.AddRange(planActive, planInactive, planOtherPatient);
        await db.SaveChangesAsync();

        var med1 = new TreatmentPlanMedicine
        {
            IdTreatmentPlan = planActive.Id,
            MedicineName = "Paracetamol",
            Dosage = "1 tab",
            TimeOfDay = TimeOfDay.Rano
        };

        var med2 = new TreatmentPlanMedicine
        {
            IdTreatmentPlan = planActive.Id,
            MedicineName = "Ibuprofen",
            Dosage = "200 mg",
            TimeOfDay = TimeOfDay.Wieczor
        };

        var medInactive = new TreatmentPlanMedicine
        {
            IdTreatmentPlan = planInactive.Id,
            MedicineName = "Aspirin",
            Dosage = "1 tab",
            TimeOfDay = TimeOfDay.Rano
        };

        var medOtherPatient = new TreatmentPlanMedicine
        {
            IdTreatmentPlan = planOtherPatient.Id,
            MedicineName = "Metformin",
            Dosage = "500 mg",
            TimeOfDay = TimeOfDay.Rano
        };

        db.TreatmentPlanMedicines.AddRange(med1, med2, medInactive, medOtherPatient);
        await db.SaveChangesAsync();

        var sut = new GetTreatmentPlanMedicinesHandler(db);

        var query = new GetTreatmentPlanMedicinesQuery(
            PatientId: "p1",
            Date: day
        );

        var result = await sut.Handle(query, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Value.Should().HaveCount(2);

        result.Value![0].MedicineName.Should().Be("Paracetamol");
        result.Value[1].MedicineName.Should().Be("Ibuprofen");

        result.Value.Select(x => x.TreatmentPlanNumber)
            .Distinct()
            .Should().ContainSingle("PLAN000000000001");
    }
}
