using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyApp.Application.Common;
using MyApp.Application.Data;
using MyApp.Domain;

namespace MyApp.Application.Reviews.Commands;

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
