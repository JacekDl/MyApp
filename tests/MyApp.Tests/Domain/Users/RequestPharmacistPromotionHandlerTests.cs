using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Users.Commands;
using MyApp.Model;
using MyApp.Tests.Common;

namespace MyApp.Tests.Domain.Users;

public class RequestPharmacistPromotionHandlerTests : TestBase
{
    [Fact]
    public async Task ReturnsError()
    {
        await using var db = CreateInMemoryDb();

        db.Users.Add(new User { Id = "u1", Email = "u1@test.com", UserName = "u1@test.com" });

        db.PharmacistPromotionRequests.Add(new PharmacistPromotionRequest
        {
            UserId = "u1",
            FirstName = "Jan",
            LastName = "Kowalski",
            NumerPWZF = "12345678",
            Status = "Pending",
            CreatedUtc = DateTime.UtcNow.AddDays(-1)
        });

        await db.SaveChangesAsync();

        var sut = new RequestPharmacistPromotionHandler(userManager: null!, db);

        var cmd = new RequestPharmacistPromotionCommand(
            UserId: "u1",
            FirstName: "Jan",
            LastName: "Kowalski",
            NumerPWZF: "12345678"
        );

        var result = await sut.Handle(cmd, CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Be("Masz już wysłane zgłoszenie oczekujące na weryfikację.");

        (await db.PharmacistPromotionRequests.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task CreatesPendingRequest()
    {
        await using var db = CreateInMemoryDb();

        db.Users.Add(new User { Id = "u1", Email = "u1@test.com", UserName = "u1@test.com" });
        await db.SaveChangesAsync();

        var sut = new RequestPharmacistPromotionHandler(userManager: null!, db);

        var cmd = new RequestPharmacistPromotionCommand(
            UserId: "u1",
            FirstName: "  Jan  ",
            LastName: "  Kowalski ",
            NumerPWZF: "12345678"
        );

        var before = DateTime.UtcNow;

        var result = await sut.Handle(cmd, CancellationToken.None);

        var after = DateTime.UtcNow;

        result.Succeeded.Should().BeTrue();
        result.ErrorMessage.Should().BeNullOrEmpty();

        var req = await db.PharmacistPromotionRequests.SingleAsync();

        req.UserId.Should().Be("u1");
        req.Status.Should().Be("Pending");
        req.FirstName.Should().Be("Jan");
        req.LastName.Should().Be("Kowalski");
        req.NumerPWZF.Should().Be("12345678");

        req.CreatedUtc.Should().BeOnOrAfter(before);
        req.CreatedUtc.Should().BeOnOrBefore(after);
    }
}
