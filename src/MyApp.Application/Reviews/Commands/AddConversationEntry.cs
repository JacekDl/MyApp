using MediatR;
using Microsoft.EntityFrameworkCore;
using MyApp.Application.Common;
using MyApp.Application.Data;

namespace MyApp.Application.Reviews.Commands;

public record AddConversationEntryCommand(string Number, string UserId, string Text) : IRequest<Result<bool>>;

public class AddConversationEntryHandler : IRequestHandler<AddConversationEntryCommand, Result<bool>>
{
    private readonly ApplicationDbContext _db;

    public AddConversationEntryHandler(ApplicationDbContext db)
    {
        _db = db;
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

        var belongsToUser = review.PharmacistId == request.UserId || review.PatientId == request.UserId;
        if (!belongsToUser)
        {
            return Result<bool>.Fail("Forbidden.");
        }

        review.Entries.Add(new Domain.Entry
        {
            UserId = request.UserId,
            Text = text,
            ReviewId = review.Id
        });

        await _db.SaveChangesAsync(ct);
        return Result<bool>.Ok(true);
    }
}
