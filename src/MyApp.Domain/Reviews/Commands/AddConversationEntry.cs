using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Data;
using MyApp.Model;

namespace MyApp.Domain.Reviews.Commands;

public record AddConversationEntryCommand(string Number, string RequestingUserId, string Text) : IRequest<Result<bool>>;

public class AddConversationEntryHandler : IRequestHandler<AddConversationEntryCommand, Result<bool>>
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<User> _userManager;

    public AddConversationEntryHandler(ApplicationDbContext db, UserManager<User> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<Result<bool>> Handle(AddConversationEntryCommand request, CancellationToken ct)
    {
        var text = (request.Text ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(text))
        {
            return Result<bool>.Fail("Message cannot be empty.");   
        }

        var review = await _db.Reviews
            .Include(r => r.Entries)
            .SingleOrDefaultAsync(r => r.Number == request.Number, ct);

        if (review is null)
        {
            return Result<bool>.Fail("Review not found.");
        }


        var user = await _userManager.FindByIdAsync(request.RequestingUserId);
        var viewerIsAdmin = user != null && await _userManager.IsInRoleAsync(user, "Admin");

        var viewerIsParticipant = review.PharmacistId == request.RequestingUserId || review.PatientId == request.RequestingUserId;

        if (!viewerIsParticipant && !viewerIsAdmin)
            return Result<bool>.Fail("Forbidden.");

        review.Entries.Add(new Entry
        {
            UserId = request.RequestingUserId,
            Text = text,
            ReviewId = review.Id,
            CreatedUtc = DateTime.UtcNow
        });

        if(request.RequestingUserId == review.PharmacistId)
        {
            review.PharmacistModified = true;
        }
        else if (request.RequestingUserId == review.PatientId)
        {
            review.PatientModified = true;
        }
        else
        {
            review.PharmacistModified = true;
            review.PatientModified = true;
        }

        await _db.SaveChangesAsync(ct);
        return Result<bool>.Ok(true);
    }
}

public class AddConversationEntryCommandValidator : AbstractValidator<AddConversationEntryCommand>
{
    private const int RequiredNumberLength = 16;
    private const int MaxTextLength = 200;

    public AddConversationEntryCommandValidator()
    {
        RuleFor(x => x.Number)
            .NotEmpty().WithMessage("Review number is required.")
            .Length(RequiredNumberLength)
                .WithMessage($"Review number must be exactly {RequiredNumberLength} characters long.")
            .Matches("^[A-Za-z0-9]+$")
                .WithMessage("Review number must contain only letters and digits.");

        RuleFor(x => x.RequestingUserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.Text)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("Wiadomość nie może być pusta.")
            .Must(s => !string.IsNullOrWhiteSpace(s))
                .WithMessage("Message cannot be empty.")
            .Must(s => (s ?? string.Empty).Trim().Length <= MaxTextLength)
                .WithMessage($"Message cannot exceed {MaxTextLength} characters.")
            .OverridePropertyName("text");
    }
}
