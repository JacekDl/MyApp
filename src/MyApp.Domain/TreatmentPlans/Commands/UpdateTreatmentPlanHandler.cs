using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Data;
using MyApp.Model;
using MyApp.Model.enums;

namespace MyApp.Domain.TreatmentPlans.Commands
{
    public record class UpdateTreatmentPlanCommand(string Number, string ReviewText ) : IRequest<UpdateTreatmentPlanResult>;

    public record class UpdateTreatmentPlanResult : Result;

    public class UpdateTreatmentPlanHandler : IRequestHandler<UpdateTreatmentPlanCommand, UpdateTreatmentPlanResult>
    {

        private readonly ApplicationDbContext _db;

        public UpdateTreatmentPlanHandler(ApplicationDbContext db)
        { 
            _db = db;
        }

        public async Task<UpdateTreatmentPlanResult> Handle(UpdateTreatmentPlanCommand request, CancellationToken ct)
        {
            var validator = new UpdateTreatmentPlanValidator().Validate(request);
            if (!validator.IsValid)
            {
                return new() { ErrorMessage = string.Join(";", validator.Errors.Select(e => e.ErrorMessage)) };
            }

            var plan = await _db.TreatmentPlans
                .FirstOrDefaultAsync(tp => tp.Number == request.Number, ct);

            if (plan is null)
            {
                return new() { ErrorMessage = "Nie znaleziono planu leczenia." };
            }

            if (plan.Status == Model.enums.TreatmentPlanStatus.Completed)
            {
                return new() { ErrorMessage = "Nie można dodać uwag do zakończonego planu leczenia." };
            }

            if (plan.Review is null)
            {
                plan.Review = new TreatmentPlanReview
                {
                    IdTreatmentPlan = plan.Id,
                    UnreadForPatient = false,
                    UnreadForPharmacist = true,
                    ReviewEntries = new List<ReviewEntry>()
                };
                _db.TreatmentPlanReviews.Add(plan.Review);
            }

            var entry = new ReviewEntry
            {
                TreatmentPlanReview = plan.Review,
                Author = ConversationParty.Patient,
                DateCreated = DateTime.UtcNow,
                Text = request.ReviewText
            };

            plan.Review.ReviewEntries.Add(entry);
            plan.Status = TreatmentPlanStatus.Completed;

            await _db.SaveChangesAsync(ct);

            return new();
        }
    }

    public class UpdateTreatmentPlanValidator : AbstractValidator<UpdateTreatmentPlanCommand>
    {
        public UpdateTreatmentPlanValidator()
        {
            RuleFor(x => x.Number)
                .Must(n => !string.IsNullOrWhiteSpace(n))
                    .WithMessage("Numer planu leczenia nie może być pusty.")
                .Length(16)
                    .WithMessage("Numer planu leczenia musi mieć dokładnie 16 znaków.")
                .Matches("^[a-zA-Z0-9]+$")
                    .WithMessage("Numer planu leczenia może zawierać tylko litery i cyfry.");

            RuleFor(x => x.ReviewText)
                .Must(t => !string.IsNullOrWhiteSpace(t))
                    .WithMessage("Treść uwag nie może być pusta.")
                .MaximumLength(500)
                    .WithMessage("Treść uwag nie może przekraczać 500 znaków.");
        }
    }
}
