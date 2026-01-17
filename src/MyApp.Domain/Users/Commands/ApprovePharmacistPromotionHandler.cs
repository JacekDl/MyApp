using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Data;
using MyApp.Model;

namespace MyApp.Domain.Users.Commands
{
    public record class ApprovePharmacistPromotionCommand(int RequestId) : IRequest<ApprovePharmacistPromotionResult>;

    public record class ApprovePharmacistPromotionResult : Result;

    public class ApprovePharmacistPromotionHandler : IRequestHandler<ApprovePharmacistPromotionCommand, ApprovePharmacistPromotionResult>
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<User> _userManager;
        public ApprovePharmacistPromotionHandler(UserManager<User> userManager, ApplicationDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }
        public async Task<ApprovePharmacistPromotionResult> Handle(ApprovePharmacistPromotionCommand request, CancellationToken ct)
        {
            var validator = new ApprovePharmacistPromotionValidator().Validate(request);
            if (!validator.IsValid)
            {
                return new() { ErrorMessage = string.Join(";", validator.Errors.Select(e => e.ErrorMessage)) };
            }

            var promo = await _db.PharmacistPromotionRequests
                .FirstOrDefaultAsync(x => x.Id == request.RequestId, ct);

            if (promo is null)
            {
                return new() { ErrorMessage = "Nie znaleziono zgłoszenia." };
            }

            if (!string.Equals(promo.Status, "Pending", StringComparison.OrdinalIgnoreCase))
            {
                return new() { ErrorMessage = "To zgłoszenie nie ma już statusu Pending." };
            }

            var user = await _userManager.FindByIdAsync(promo.UserId);
            if (user is null)
            {
                return new() { ErrorMessage = "Nie znaleziono użytkownika powiązanego ze zgłoszeniem." };
            }

            if (!await _userManager.IsInRoleAsync(user, UserRoles.Pharmacist))
            {
                var addRole = await _userManager.AddToRoleAsync(user, UserRoles.Pharmacist);
                if (!addRole.Succeeded)
                {
                    return new() { ErrorMessage = "Nie udało się nadać roli Farmaceuta." };
                }
            }

            //TODO: utrzymać obie role?
            if (await _userManager.IsInRoleAsync(user, UserRoles.Patient))
            {
                await _userManager.RemoveFromRoleAsync(user, UserRoles.Patient);
            }

            promo.Status = "Approved";
            await _db.SaveChangesAsync(ct);
            return new();
        }
    }

    public class ApprovePharmacistPromotionValidator : AbstractValidator<ApprovePharmacistPromotionCommand>
    {
        public ApprovePharmacistPromotionValidator()
        {
            RuleFor(x => x.RequestId)
                .GreaterThan(0)
                .WithMessage("Id zgłoszenia musi być dodatnie.");
        }
    }
}
