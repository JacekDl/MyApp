using MediatR;
using Microsoft.EntityFrameworkCore;
using MyApp.Application.Common;
using MyApp.Application.Data;

namespace MyApp.Application.Reviews.Commands;

public record MarkConversationSeenCommand(string Number, string UserId) : IRequest<Result<bool>>;

public class MarkConversationSeenHandler : IRequestHandler<MarkConversationSeenCommand, Result<bool>>
{
    private readonly ApplicationDbContext _db;
    public MarkConversationSeenHandler(ApplicationDbContext db)
    {
        _db = db;
    }
    public async Task<Result<bool>> Handle(MarkConversationSeenCommand request, CancellationToken ct)
    {
        var review = await _db.Reviews
            .SingleOrDefaultAsync(r => r.Number == request.Number, ct);

        if (review is null)
        {
            return Result<bool>.Fail("Review not found.");
        }

        var belongsToUser = review.PharmacistId == request.UserId || review.PatientId == request.UserId;
        if (!belongsToUser)
        {
            return Result<bool>.Fail("Forbidden.");
        }

        if(request.UserId == review.PharmacistId)
        {
            review.PatientModified = false;
        }
        else if (request.UserId == review.PatientId)
        {
            review.PharmacistModified = false;
        }
        await _db.SaveChangesAsync(ct);
        return Result<bool>.Ok(true);
    }
}
