using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Data;
using MyApp.Model;
using MyApp.Model.enums;
using static System.Net.Mime.MediaTypeNames;

namespace MyApp.Domain.TreatmentPlans.Commands
{
    public record class AddTreatmentPlanReviewEntryCommand(string Number, string CurrentUserId, string Text)
        : IRequest<AddTreatmentPlanReviewEntryResult>;

    public record class AddTreatmentPlanReviewEntryResult : Result;

    public class AddTreatmentPlanReviewEntryHandler : IRequestHandler<AddTreatmentPlanReviewEntryCommand, AddTreatmentPlanReviewEntryResult>
    {

        private readonly ApplicationDbContext _db;
        private readonly UserManager<User> _userManager;

        public AddTreatmentPlanReviewEntryHandler(ApplicationDbContext db, UserManager<User> userManager)
        {
            _db = db;
            _userManager = userManager;
        }
        public async Task<AddTreatmentPlanReviewEntryResult> Handle(AddTreatmentPlanReviewEntryCommand request, CancellationToken ct)
        {
            var plan = await _db.TreatmentPlans
            .Include(tp => tp.Review)
                .ThenInclude(r => r.ReviewEntries)
            .FirstOrDefaultAsync(tp => tp.Number == request.Number, ct);

            if (plan is null)
            {
                return new() { ErrorMessage = $"Nie znaleziono planu leczenia." };

            }

            ConversationParty author;

            if (!string.IsNullOrWhiteSpace(plan.IdPharmacist) && plan.IdPharmacist == request.CurrentUserId)
            {
                author = ConversationParty.Pharmacist;
            }
            else
            {
                author = ConversationParty.Patient;
            }

            if (plan.Review is null)
            {
                plan.Review = new TreatmentPlanReview
                {
                    IdTreatmentPlan = plan.Id,
                    UnreadForPatient = false,
                    UnreadForPharmacist = false,
                    ReviewEntries = new List<ReviewEntry>()
                };

                _db.TreatmentPlanReviews.Add(plan.Review);
            }

            var entry = new ReviewEntry
            {
                TreatmentPlanReview = plan.Review,
                Author = author,
                DateCreated = DateTime.UtcNow,
                Text = request.Text
            };

            plan.Review.ReviewEntries.Add(entry);

            if (author == ConversationParty.Patient)
                plan.Review.UnreadForPharmacist = true;
            else
                plan.Review.UnreadForPatient = true;

            await _db.SaveChangesAsync(ct);

            return new();
        }
    }
}
