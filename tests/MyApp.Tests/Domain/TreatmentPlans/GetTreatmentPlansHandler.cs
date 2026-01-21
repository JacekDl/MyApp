using FluentAssertions;
using MyApp.Domain.TreatmentPlans.Queries;
using MyApp.Model;
using MyApp.Model.enums;
using MyApp.Tests.Common;

namespace MyApp.Tests.Domain.TreatmentPlans;

public class GetTreatmentPlansHandlerTests : TestBase
{
    #region Helpers

    private static TreatmentPlan CreatePlan(
        string number,
        string pharmacistId,
        string? patientId,
        TreatmentPlanStatus status,
        DateTime created,
        bool unreadForPatient = false,
        bool unreadForPharmacist = false)
        => new()
        {
            Number = number,
            IdPharmacist = pharmacistId,
            IdPatient = patientId,
            Status = status,
            DateCreated = created,
            AdviceFullText = $"Advice {number}",
            Review = new TreatmentPlanReview
            {
                UnreadForPatient = unreadForPatient,
                UnreadForPharmacist = unreadForPharmacist
            }
        };

    #endregion

    #region Handler

    [Fact]
    public async Task ReturnsAllPlans()
    {
        await using var db = CreateInMemoryDb();

        db.TreatmentPlans.AddRange(
            CreatePlan("PLAN000000000001", "ph1", "pt1", TreatmentPlanStatus.Created, DateTime.UtcNow.AddDays(-1)),
            CreatePlan("PLAN000000000002", "ph2", "pt2", TreatmentPlanStatus.Claimed, DateTime.UtcNow)
        );

        await db.SaveChangesAsync();

        var userManager = CreateUserManager();
        var sut = new GetTreatmentPlansHandler(userManager, db);

        var query = new GetTreatmentPlansQuery(
            SearchTxt: null,
            CurrentUserId: null,
            Status: null,
            ViewerParty: null
        );

        var result = await sut.Handle(query, CancellationToken.None);

        result.TotalCount.Should().Be(2);
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task Filters_By_Status()
    {
        await using var db = CreateInMemoryDb();

        db.TreatmentPlans.AddRange(
            CreatePlan("PLAN000000000001", "ph1", "pt1", TreatmentPlanStatus.Created, DateTime.UtcNow),
            CreatePlan("PLAN000000000002", "ph1", "pt1", TreatmentPlanStatus.Completed, DateTime.UtcNow)
        );

        await db.SaveChangesAsync();

        var sut = new GetTreatmentPlansHandler(CreateUserManager(), db);

        var query = new GetTreatmentPlansQuery(
            SearchTxt: null,
            CurrentUserId: null,
            Status: TreatmentPlanStatus.Completed,
            ViewerParty: null
        );

        var result = await sut.Handle(query, CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Value.Single().Status.Should().Be("Zakończony");
    }
    #endregion
}
