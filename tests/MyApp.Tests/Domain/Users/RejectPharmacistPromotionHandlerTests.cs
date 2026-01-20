using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Users.Commands;
using MyApp.Model;
using MyApp.Tests.Common;

namespace MyApp.Tests.Domain.Users;

public class RejectPharmacistPromotionHandlerTests : TestBase
{
    #region Helpers

    private static PharmacistPromotionRequest CreateRequest(int id, string userId, string status)
        => new()
        {
            Id = id,
            UserId = userId,
            Status = status,

            NumerPWZF = "1234567"
        };

    #endregion

    [Theory]
    [InlineData("Approved")]
    [InlineData("Rejected")]
    [InlineData("pending-but-different")]
    public async Task ReturnsError_StatusIsNotPending(string status)
    {
        await using var db = CreateInMemoryDb();

        db.PharmacistPromotionRequests.Add(CreateRequest(1, "u1", status));
        await db.SaveChangesAsync();

        var sut = new RejectPharmacistPromotionHandler(db);

        var result = await sut.Handle(new RejectPharmacistPromotionCommand(1), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Be("To zgłoszenie nie ma już statusu Pending.");

        var reloaded = await db.PharmacistPromotionRequests.SingleAsync();
        reloaded.Status.Should().Be(status);
    }

    [Theory]
    [InlineData("Pending")]
    public async Task SetsStatusToRejected_WhenPending(string status)
    {
        await using var db = CreateInMemoryDb();

        db.PharmacistPromotionRequests.Add(CreateRequest(1, "u1", status));
        await db.SaveChangesAsync();

        var sut = new RejectPharmacistPromotionHandler(db);

        var result = await sut.Handle(new RejectPharmacistPromotionCommand(1), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.ErrorMessage.Should().BeNullOrEmpty();

        var reloaded = await db.PharmacistPromotionRequests.SingleAsync();
        reloaded.Status.Should().Be("Rejected");
    }
}
