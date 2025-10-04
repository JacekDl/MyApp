using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyApp.Application.Common;
using MyApp.Application.Data;
using MyApp.Domain;

namespace MyApp.Application.Reviews.Queries;

public record GetConversationQuery(string Number, string RequestingUserId) : IRequest<Result<ConversationDto>>;

public class GetConversationHandler : IRequestHandler<GetConversationQuery, Result<ConversationDto>>
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<User> _userManager;

    public GetConversationHandler(ApplicationDbContext db, UserManager<User> userManager)
    {
        _db = db;
        _userManager = userManager;
    }
    public async Task<Result<ConversationDto>> Handle(GetConversationQuery request, CancellationToken ct)
    {
        var review = await _db.Reviews
            .Include(r => r.Entries.OrderBy(e => e.CreatedUtc))
            .SingleOrDefaultAsync(r => r.Number == request.Number, ct);

        if (review is null)
            return Result<ConversationDto>.Fail("Review not found.");

        var user = await _userManager.FindByIdAsync(request.RequestingUserId);
        var viewerIsAdmin = user != null && await _userManager.IsInRoleAsync(user, "Admin");

        var viewerIsParticipant = review.PharmacistId == request.RequestingUserId || review.PatientId == request.RequestingUserId;

        if (!viewerIsParticipant && !viewerIsAdmin)
            return Result<ConversationDto>.Fail("Forbidden.");

        var dto = new ConversationDto(
            review.Number,
            review.Entries.Select(e => new EntryDto(e.UserId, e.Text, e.CreatedUtc)).ToList());

        return Result<ConversationDto>.Ok(dto);
    }
}
