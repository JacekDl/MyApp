using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Data;
using MyApp.Model;

namespace MyApp.Domain.Reviews.Queries;

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

        var entries = new List<EntryDto>();
        foreach (var e in review.Entries.OrderBy(en => en.CreatedUtc))
        {
            string displayName = "Unknown";
            if (!string.IsNullOrEmpty(e.UserId))
            {
                var entryUser = await _userManager.FindByIdAsync(e.UserId);
                if (entryUser != null)
                {
                    displayName = !string.IsNullOrWhiteSpace(entryUser.DisplayName) ? entryUser.DisplayName : entryUser.Email ?? "Unknown";
                    var roles = await _userManager.GetRolesAsync(entryUser);
                    if (string.IsNullOrEmpty(displayName) && roles.Count > 0)
                    {
                        displayName = roles[0];
                    }
                }
            }
            else
            {
                if (review.PharmacistId == request.RequestingUserId)
                {
                    displayName = "Pharmacist";
                }
                else if(review.PatientId == request.RequestingUserId)
                {
                    displayName = "Patient";
                }
                else
                {
                    displayName = "Admin"; 
                }
            }

            entries.Add(new EntryDto(e.UserId, e.Text, e.CreatedUtc, displayName));
        }

        var dto = new ConversationDto(
            review.Number,
            entries);
            

        return Result<ConversationDto>.Ok(dto);
    }
}
