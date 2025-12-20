using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Data;
using MyApp.Model;

namespace MyApp.Domain.Users.Commands
{
    public record class RequestPharmacistPromotionCommand (
        string UserId, 
        string FirstName, 
        string LastName, 
        string NumerPWZF) : IRequest<RequestPharmacistPromotionResult>;

    public record class RequestPharmacistPromotionResult : Result;

    public class RequestPharmacistPromotionHandler : IRequestHandler<RequestPharmacistPromotionCommand, RequestPharmacistPromotionResult>
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<User> _userManager;

        public RequestPharmacistPromotionHandler(UserManager<User> userManager, ApplicationDbContext db)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<RequestPharmacistPromotionResult> Handle(RequestPharmacistPromotionCommand request, CancellationToken ct)
        {
            var userExists = await _db.Users
                .AnyAsync(u => u.Id == request.UserId, ct);
            if (!userExists)
            {
                return new RequestPharmacistPromotionResult() { ErrorMessage = "Nie znaleziono użytkownika." };
            }

            var pendingExists = await _db.PharmacistPromotionRequests
                .AnyAsync(r => r.UserId == request.UserId && r.Status == "Pending", ct);

            if (pendingExists)
            {
                return new RequestPharmacistPromotionResult() { ErrorMessage = "Masz już wysłane zgłoszenie oczekujące na weryfikację." };

            }

            var entity = new PharmacistPromotionRequest
            {
                UserId = request.UserId,
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                NumerPWZF = request.NumerPWZF.Trim(),
                Status = "Pending",
                CreatedUtc = DateTime.UtcNow
            };

            _db.PharmacistPromotionRequests.Add(entity);
            await _db.SaveChangesAsync(ct);

            return new();
        }
    }

    public class RequestPharmacistPromotionValidator : AbstractValidator<RequestPharmacistPromotionCommand>
    {
        public RequestPharmacistPromotionValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty();

            RuleFor(x => x.FirstName)
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(x => x.LastName)
                .NotEmpty()
                .MaximumLength(80);

            RuleFor(x => x.NumerPWZF)
                .NotEmpty()
                .Length(8)                 
                .Matches("^[0-9]{8}$")     
                .WithMessage("Numer PWZF musi składać się dokładnie z 8 cyfr.");
        }
    }
}
