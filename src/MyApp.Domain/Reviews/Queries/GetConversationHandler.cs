using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Data;
using MyApp.Domain.Instructions.Commands;
using MyApp.Model;

namespace MyApp.Domain.Reviews.Queries;

public record class GetConversationQuery(string Number, string RequestingUserId) : IRequest<GetConversationResult>;

public record class GetConversationResult : Result<ConversationDto>;

public class GetConversationHandler : IRequestHandler<GetConversationQuery, GetConversationResult>
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<User> _userManager;

    public GetConversationHandler(ApplicationDbContext db, UserManager<User> userManager)
    {
        _db = db;
        _userManager = userManager;
    }
    public async Task<GetConversationResult> Handle(GetConversationQuery request, CancellationToken ct)
    {
        var validator = new GetConversationValidator().Validate(request);
        if (!validator.IsValid)
        {
            return new() { ErrorMessage = string.Join(";", validator.Errors.Select(e => e.ErrorMessage)) };
        }

        var review = await _db.Reviews
            .Include(r => r.Entries.OrderBy(e => e.CreatedUtc))
            .SingleOrDefaultAsync(r => r.Number == request.Number, ct);

        if (review is null)
        {
            return new() { ErrorMessage = "Nie znaleziono zaleceń." };
        }

        var user = await _userManager.FindByIdAsync(request.RequestingUserId);
        var viewerIsAdmin = user != null && await _userManager.IsInRoleAsync(user, "Admin");

        var viewerIsParticipant = review.PharmacistId == request.RequestingUserId || review.PatientId == request.RequestingUserId;

        if (!viewerIsParticipant && !viewerIsAdmin)
        {
            return new() { ErrorMessage = "Brak dostępu." };
        }

        var entries = new List<EntryDto>();
        foreach (var e in review.Entries.OrderBy(en => en.CreatedUtc))
        {
            string displayName = "Unknown";
            if (!string.IsNullOrEmpty(e.UserId))
            {
                var entryUser = await _userManager.FindByIdAsync(e.UserId);
                if (entryUser != null)
                {
                    if (!string.IsNullOrEmpty(entryUser.DisplayName))
                    {
                        displayName = entryUser.DisplayName;
                    }
                    else
                    {
                        var roles = await _userManager.GetRolesAsync(entryUser);
                        var role = roles.FirstOrDefault() ?? string.Empty;

                        if (role.Equals("Pharmacist", StringComparison.OrdinalIgnoreCase))
                        {
                            displayName = "Farmaceuta";
                        }
                        else if (role.Equals("Patient", StringComparison.OrdinalIgnoreCase))
                        {
                            displayName = "Pacjent";
                        }
                        else if (role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                        {
                            displayName = "Admin";
                        }
                        else
                        {
                            displayName = "Użytkownik";
                        }
                    }
                }
            }
            else
            {
                displayName = "Pacjent";
            }

            entries.Add(new EntryDto(e.UserId, e.Text, e.CreatedUtc, displayName));
        }

        var dto = new ConversationDto(
            review.Number,
            review.Completed,
            entries);
            
        return new() { Value = dto };
    }

    public class GetConversationValidator : AbstractValidator<GetConversationQuery>
    {
        public GetConversationValidator()
        {
            RuleFor(x => x.Number)
                .NotEmpty()
                    .WithMessage("Numer tokenu jest wymagany.")
                .Length(16)
                    .WithMessage("Numer tokenu musi mieć dokładnie 16 znaków.")
                .Matches("^[a-zA-Z0-9]+$")
                    .WithMessage("Numer tokenu może zawierać tylko litery i cyfry.");

            RuleFor(x => x.RequestingUserId)
                .NotEmpty()
                    .WithMessage("Id użytkownika jest wymagane.");
        }
    }
}
