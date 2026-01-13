using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.TreatmentPlans.Commands;
using MyApp.Model;
using MyApp.Model.enums;
using MyApp.Tests.Common;

namespace MyApp.Tests.Domain.TreatmentPlans;

public class ToggleMedicineTakenHandlerTests : TestBase
{
    [Fact]
    public async Task ReturnsError_MedicineDoesNotBelongToPatient()
    {
        await using var db = CreateInMemoryDb();

        var plan = new TreatmentPlan
        {
            Number = "ABCDEF1234567890",
            IdPharmacist = "ph1",
            IdPatient = "p-other",
            Status = TreatmentPlanStatus.Claimed,
            DateCreated = DateTime.UtcNow,
            AdviceFullText = "Advice"
        };

        db.TreatmentPlans.Add(plan);
        await db.SaveChangesAsync();

        var med = new TreatmentPlanMedicine
        {
            IdTreatmentPlan = plan.Id,
            MedicineName = "Paracetamol",
            Dosage = "1 tab",
            TimeOfDay = TimeOfDay.Rano
        };

        db.TreatmentPlanMedicines.Add(med);
        await db.SaveChangesAsync();

        var sut = new ToggleMedicineTakenHandler(db);

        var cmd = new ToggleMedicineTakenCommand(
            IdPatient: "p1",
            TreatmentPlanMedicineId: med.Id,
            Date: DateTime.Today,
            IsTaken: true
        );

        var result = await sut.Handle(cmd, CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Be("Brak dostępu do tego leku.");

        (await db.Set<MedicineTakenConfirmation>().CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task AddsConfirmation()
    {
        await using var db = CreateInMemoryDb();

        var plan = new TreatmentPlan
        {
            Number = "ABCDEF1234567890",
            IdPharmacist = "ph1",
            IdPatient = "p1",
            Status = TreatmentPlanStatus.Claimed,
            DateCreated = DateTime.UtcNow,
            AdviceFullText = "Advice"
        };

        db.TreatmentPlans.Add(plan);
        await db.SaveChangesAsync();

        var med = new TreatmentPlanMedicine
        {
            IdTreatmentPlan = plan.Id,
            MedicineName = "Paracetamol",
            Dosage = "1 tab",
            TimeOfDay = TimeOfDay.Rano
        };

        db.TreatmentPlanMedicines.Add(med);
        await db.SaveChangesAsync();

        var sut = new ToggleMedicineTakenHandler(db);

        var date = DateTime.Today.AddHours(10).AddMinutes(15);

        var cmd = new ToggleMedicineTakenCommand(
            IdPatient: "p1",
            TreatmentPlanMedicineId: med.Id,
            Date: date,
            IsTaken: true
        );

        var result = await sut.Handle(cmd, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.ErrorMessage.Should().BeNullOrEmpty();

        var confirmations = await db.Set<MedicineTakenConfirmation>()
            .AsNoTracking()
            .Where(x => x.IdTreatmentPlanMedicine == med.Id)
            .ToListAsync();

        confirmations.Should().HaveCount(1);
        confirmations[0].DateTimeTaken.Should().Be(date);
    }

    [Fact]
    public async Task RemovesConfirmation()
    {
        await using var db = CreateInMemoryDb();

        var plan = new TreatmentPlan
        {
            Number = "ABCDEF1234567890",
            IdPharmacist = "ph1",
            IdPatient = "p1",
            Status = TreatmentPlanStatus.Claimed,
            DateCreated = DateTime.UtcNow,
            AdviceFullText = "Advice"
        };

        db.TreatmentPlans.Add(plan);
        await db.SaveChangesAsync();

        var med = new TreatmentPlanMedicine
        {
            IdTreatmentPlan = plan.Id,
            MedicineName = "Paracetamol",
            Dosage = "1 tab",
            TimeOfDay = TimeOfDay.Rano
        };

        db.TreatmentPlanMedicines.Add(med);
        await db.SaveChangesAsync();

        var day = DateTime.Today;

        db.Add(new MedicineTakenConfirmation
        {
            IdTreatmentPlanMedicine = med.Id,
            DateTimeTaken = day.AddHours(9)
        });
        await db.SaveChangesAsync();

        var sut = new ToggleMedicineTakenHandler(db);

        var cmd = new ToggleMedicineTakenCommand(
            IdPatient: "p1",
            TreatmentPlanMedicineId: med.Id,
            Date: day.AddHours(20),
            IsTaken: false
        );

        var result = await sut.Handle(cmd, CancellationToken.None);

        result.Succeeded.Should().BeTrue();

        (await db.Set<MedicineTakenConfirmation>()
            .CountAsync(x => x.IdTreatmentPlanMedicine == med.Id))
            .Should().Be(0);
    }
}

