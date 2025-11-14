using MediatR;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Data;

namespace MyApp.Domain.Reviews.Commands;

public record class MarkConversationSeenCommand(string Number, string UserId) : IRequest<MarkConversationSeenResult>;

public record class MarkConversationSeenResult : Result;

public class MarkConversationSeenHandler : IRequestHandler<MarkConversationSeenCommand, MarkConversationSeenResult>
{
    private readonly ApplicationDbContext _db;
    public MarkConversationSeenHandler(ApplicationDbContext db)
    {
        _db = db;
    }
    public async Task<MarkConversationSeenResult> Handle(MarkConversationSeenCommand request, CancellationToken ct)
    {
        var review = await _db.Reviews
            .SingleOrDefaultAsync(r => r.Number == request.Number, ct);

        if (review is null)
        {
            return new() { ErrorMessage = "Nie znaleziono zaleceń." };
        }


        var currentUser = _db.Users.SingleOrDefault(u => u.Id == request.UserId);
        var isAdmin = currentUser.Role == "Admin";
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
}
