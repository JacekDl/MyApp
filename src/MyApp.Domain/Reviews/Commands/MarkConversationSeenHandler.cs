using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Data;
using MyApp.Domain.Users;
using MyApp.Model;

namespace MyApp.Domain.Reviews.Commands;

public record class MarkConversationSeenCommand(string Number, string UserId) : IRequest<MarkConversationSeenResult>;

public record class MarkConversationSeenResult : Result;

public class MarkConversationSeenHandler : IRequestHandler<MarkConversationSeenCommand, MarkConversationSeenResult>
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<User> _userManager;

    public MarkConversationSeenHandler(ApplicationDbContext db, UserManager<User> userManager)
    {
        _db = db;
        _userManager = userManager;
    }
    public async Task<MarkConversationSeenResult> Handle(MarkConversationSeenCommand request, CancellationToken ct)
    {
        var validator = new MarkConversationSeenValidator().Validate(request);
        if (!validator.IsValid)
        {
            return new() { ErrorMessage = string.Join(";", validator.Errors.Select(e => e.ErrorMessage)) };
        }

        var review = await _db.Reviews
            .SingleOrDefaultAsync(r => r.Number == request.Number, ct);

        if (review is null)
        {
            return new() { ErrorMessage = "Nie znaleziono zaleceń." };
        }

        var currentUser = _db.Users.SingleOrDefault(u => u.Id == request.UserId);
        if (currentUser is null)
        {
             return new() { ErrorMessage = "Nie znaleziono użytkownika." };
        }    

        var isAdmin = await _userManager.IsInRoleAsync(currentUser, UserRoles.Admin);
        var belongsToUser = review.PharmacistId == request.UserId || review.PatientId == request.UserId || isAdmin;
        if (!belongsToUser)
        {
            return new() { ErrorMessage = "Brak dostępu." };
        }

        if (request.UserId == review.PharmacistId)
        {
            review.PatientModified = false;
        }
        else if (request.UserId == review.PatientId)
        {
            review.PharmacistModified = false;
        }
        await _db.SaveChangesAsync(ct);
        return new();
    }

    public class MarkConversationSeenValidator : AbstractValidator<MarkConversationSeenCommand>
    {
        public MarkConversationSeenValidator()
        {
            RuleFor(x => x.Number)
                .NotEmpty().WithMessage("Numer rozmowy jest wymagany.")
                .Length(16).WithMessage("Numer rozmowy musi mieć dokładnie 16 znaków.");

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("Identyfikator użytkownika jest wymagany.");
        }
    }
}
