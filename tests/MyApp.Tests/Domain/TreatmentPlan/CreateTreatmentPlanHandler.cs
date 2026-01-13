using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.TreatmentPlans.Commands;
using MyApp.Domain.Users;
using MyApp.Tests.Common;

namespace MyApp.Tests.Domain.TreatmentPlans;

public class CreateTreatmentPlanHandlerTests : TestBase
{

    [Fact]
    public async Task ReturnsError()
    {
        await using var db = CreateInMemoryDb();
        var sut = new CreateTreatmentPlanHandler(db);

        var cmd = new CreateTreatmentPlanCommand(
            PharmacistId: "",
            Medicines: Array.Empty<CreateTreatmentPlanMedicineDTO>(),
            Advice: null
        );

        var result = await sut.Handle(cmd, CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrWhiteSpace();
        result.ErrorMessage!.Should().Contain("Id farmaceuty nie może być puste.");
        result.ErrorMessage.Should().Contain("Plan leczenia musi zawierać co najmniej jeden lek.");

        (await db.TreatmentPlans.CountAsync()).Should().Be(0);
        (await db.TreatmentPlanMedicines.CountAsync()).Should().Be(0);
        (await db.TreatmentPlanAdvices.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task BuildsAdviceFullText()
    {
        await using var db = CreateInMemoryDb();
        var sut = new CreateTreatmentPlanHandler(db);

        var cmd = new CreateTreatmentPlanCommand(
            PharmacistId: "ph1",
            Medicines: new[]
            {
                new CreateTreatmentPlanMedicineDTO("Paracetamol", "1 tab", "raz_dziennie_wieczorem")
            },
            Advice: "   "
        );

        var result = await sut.Handle(cmd, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();

        var plan = result.Value!;
        plan.AdviceFullText.Should().Be("Paracetamol - 1 tab raz dziennie wieczorem.");

        (await db.TreatmentPlanAdvices.CountAsync()).Should().Be(0);
        (await db.TreatmentPlanMedicines.CountAsync()).Should().Be(1);
    }
}
