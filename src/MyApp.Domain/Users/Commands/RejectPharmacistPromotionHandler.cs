using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Data;
using MyApp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyApp.Domain.Users.Commands
{
    public record class RejectPharmacistPromotionCommand(int RequestId) : IRequest<RejectPharmacistPromotionResult>;

    public record class RejectPharmacistPromotionResult : Result;

    public class RejectPharmacistPromotionHandler : IRequestHandler<RejectPharmacistPromotionCommand, RejectPharmacistPromotionResult>
    {
        private readonly ApplicationDbContext _db;

        public RejectPharmacistPromotionHandler(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<RejectPharmacistPromotionResult> Handle(RejectPharmacistPromotionCommand request, CancellationToken ct)
        {
            var validator = new RejectPharmacistPromotionValidator().Validate(request);
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

            promo.Status = "Rejected";

            await _db.SaveChangesAsync(ct);
            return new();
        }
    }

    public class RejectPharmacistPromotionValidator : AbstractValidator<RejectPharmacistPromotionCommand>
    {
        public RejectPharmacistPromotionValidator()
        {
            RuleFor(x => x.RequestId)
                .GreaterThan(0)
                    .WithMessage("Id zgłoszenia musi być liczbą dodatnią.");
        }
    }
}
