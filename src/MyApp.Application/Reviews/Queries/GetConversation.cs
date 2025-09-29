using MediatR;
using Microsoft.EntityFrameworkCore;
using MyApp.Application.Common;
using MyApp.Application.Data;

namespace MyApp.Application.Reviews.Queries;

public record GetConversationQuery(string Number, string RequestingUserId) : IRequest<Result<ConversationDto>>;

public class GetConversationHandler : IRequestHandler<GetConversationQuery, Result<ConversationDto>>
{
    private readonly ApplicationDbContext _db;

    public GetConversationHandler(ApplicationDbContext db)
    {
        _db = db;
    }
    public async Task<Result<ConversationDto>> Handle(GetConversationQuery request, CancellationToken ct)
    {
        var review = await _db.Reviews
            .Include(r => r.Entries.OrderBy(e => e.CreatedUtc))
            .SingleOrDefaultAsync(r => r.Number == request.Number, ct);

        if (review is null)
            return Result<ConversationDto>.Fail("Review not found.");

        if (review.PharmacistId != request.RequestingUserId)
            return Result<ConversationDto>.Fail("Forbidden.");

        var dto = new ConversationDto(
            review.Number,
            review.Entries.Select(e => new EntryDto(e.UserId, e.Text, e.CreatedUtc)).ToList());

        return Result<ConversationDto>.Ok(dto);
    }
}
