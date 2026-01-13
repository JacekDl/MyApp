using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using MyApp.Domain.Users;
using MyApp.Domain.Users.Commands;
using MyApp.Model;
using MyApp.Tests.Common;

namespace MyApp.Tests.Domain.Users;

public class ApprovePharmacistPromotionHandlerTests : TestBase
{
    #region Helpers

    private static PharmacistPromotionRequest CreateRequest(
        int id,
        string userId,
        string status)
        => new()
        {
            Id = id,
            UserId = userId,
            Status = status,

            FirstName = "Jan",
            LastName = "Kowalski",
            NumerPWZF = "1234567"
        };

    #endregion

    [Fact]
    public async Task ReturnsError_AddToRoleFails()
    {
        await using var db = CreateInMemoryDb();

        db.PharmacistPromotionRequests.Add(CreateRequest(1, "u1", "Pending"));
        await db.SaveChangesAsync();

        var user = new User { Id = "u1", Email = "u1@test.com" };

        var userManagerMock = CreateConfiguredUserManagerMock(new[] { user });

        userManagerMock.Setup(x => x.IsInRoleAsync(user, UserRoles.Pharmacist))
            .ReturnsAsync(false);

        userManagerMock.Setup(x => x.AddToRoleAsync(user, UserRoles.Pharmacist))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "fail" }));

        var sut = new ApprovePharmacistPromotionHandler(userManagerMock.Object, db);

        var result = await sut.Handle(new ApprovePharmacistPromotionCommand(1), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Be("Nie udało się nadać roli Farmaceuta.");

        var promo = await db.PharmacistPromotionRequests.SingleAsync();
        promo.Status.Should().Be("Pending");
    }

    [Fact]
    public async Task ApprovesRequest_AddsPharmacistRole()
    {
        await using var db = CreateInMemoryDb();

        db.PharmacistPromotionRequests.Add(CreateRequest(1, "u1", "Pending"));
        await db.SaveChangesAsync();

        var user = new User { Id = "u1", Email = "u1@test.com" };

        var userManagerMock = CreateConfiguredUserManagerMock(new[] { user });

        userManagerMock.Setup(x => x.IsInRoleAsync(user, UserRoles.Pharmacist))
            .ReturnsAsync(false);
        userManagerMock.Setup(x => x.AddToRoleAsync(user, UserRoles.Pharmacist))
            .ReturnsAsync(IdentityResult.Success);

        userManagerMock.Setup(x => x.IsInRoleAsync(user, UserRoles.Patient))
            .ReturnsAsync(true);
        userManagerMock.Setup(x => x.RemoveFromRoleAsync(user, UserRoles.Patient))
            .ReturnsAsync(IdentityResult.Success);

        var sut = new ApprovePharmacistPromotionHandler(userManagerMock.Object, db);

        var result = await sut.Handle(new ApprovePharmacistPromotionCommand(1), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.ErrorMessage.Should().BeNullOrEmpty();

        var promo = await db.PharmacistPromotionRequests.SingleAsync();
        promo.Status.Should().Be("Approved");

        userManagerMock.Verify(x => x.AddToRoleAsync(user, UserRoles.Pharmacist), Times.Once);
        userManagerMock.Verify(x => x.RemoveFromRoleAsync(user, UserRoles.Patient), Times.Once);
    }
}
