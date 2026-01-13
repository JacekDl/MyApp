using FluentAssertions;
using MyApp.Domain.Users.Queries;
using MyApp.Model;
using MyApp.Tests.Common;

namespace MyApp.Tests.Domain.Users;

public class GetPendingPromotionsHandlerTests : TestBase
{
    private static PharmacistPromotionRequest CreateRequest(
        int id,
        string userId,
        string status,
        DateTime createdUtc,
        string firstName = "Jan",
        string lastName = "Kowalski",
        string numerPwzf = "12345678")
        => new()
        {
            Id = id,
            UserId = userId,
            Status = status,
            CreatedUtc = createdUtc,

            FirstName = firstName,
            LastName = lastName,
            NumerPWZF = numerPwzf
        };

    [Fact]
    public async Task ReturnsOnlyPending()
    {
        await using var db = CreateInMemoryDb();

        db.Users.AddRange(
            new User { Id = "u1", Email = "u1@test.com", UserName = "u1@test.com", DisplayName = "User One" },
            new User { Id = "u2", Email = "u2@test.com", UserName = "u2@test.com", DisplayName = "User Two" }
        );

        var now = DateTime.UtcNow;

        db.PharmacistPromotionRequests.AddRange(
            CreateRequest(id: 10, userId: "u1", status: "Pending", createdUtc: now.AddHours(-1)),
            CreateRequest(id: 11, userId: "u2", status: "Pending", createdUtc: now.AddHours(-2)),
            CreateRequest(id: 12, userId: "u1", status: "Pending", createdUtc: now.AddMinutes(-10))
        );

        db.PharmacistPromotionRequests.Add(CreateRequest(id: 99, userId: "u1", status: "Approved", createdUtc: now));

        await db.SaveChangesAsync();

        var sut = new GetPendingPromotionsHandler(db);

        var result = await sut.Handle(new GetPendingPromotionsQuery(), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();

        result.Value!.Select(x => x.Id)
            .Should().ContainInOrder(12, 10, 11);

        result.Value!.All(x => x.Status == "Pending").Should().BeTrue();

        var first = result.Value![0];
        first.UserId.Should().Be("u1");
        first.Email.Should().Be("u1@test.com");
        first.DisplayName.Should().Be("User One");
    }

    [Fact]
    public async Task MapsRequestFieldsCorrectly()
    {
        await using var db = CreateInMemoryDb();

        db.Users.Add(new User
        {
            Id = "u1",
            Email = "u1@test.com",
            UserName = "u1@test.com",
            DisplayName = "Janek"
        });

        var created = DateTime.UtcNow.AddMinutes(-30);

        db.PharmacistPromotionRequests.Add(CreateRequest(
            id: 7,
            userId: "u1",
            status: "Pending",
            createdUtc: created,
            firstName: "Adam",
            lastName: "Nowak",
            numerPwzf: "87654321"
        ));

        await db.SaveChangesAsync();

        var sut = new GetPendingPromotionsHandler(db);

        var result = await sut.Handle(new GetPendingPromotionsQuery(), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Value!.Should().HaveCount(1);

        var dto = result.Value![0];
        dto.Id.Should().Be(7);
        dto.UserId.Should().Be("u1");
        dto.Email.Should().Be("u1@test.com");
        dto.DisplayName.Should().Be("Janek");
        dto.FirstName.Should().Be("Adam");
        dto.LastName.Should().Be("Nowak");
        dto.NumerPWZF.Should().Be("87654321");
        dto.Status.Should().Be("Pending");
        dto.CreatedUtc.Should().Be(created);
    }
}
