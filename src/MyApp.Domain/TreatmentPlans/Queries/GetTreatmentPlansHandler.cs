using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Data;
using MyApp.Domain.TreatmentPlans.Mappers;
using MyApp.Model;
using MyApp.Model.enums;
using System.Numerics;

namespace MyApp.Domain.TreatmentPlans.Queries;

public record class GetTreatmentPlansQuery(
    string? SearchTxt,
    string? CurrentUserId,
    TreatmentPlanStatus? Status,
    ConversationParty? ViewerParty,
    string? UserEmail = null,
    int Page = 1,
    int PageSize = 10
) : IRequest<GetTreatmentPlansResult>;
public record class GetTreatmentPlansResult : PagedResult<List<TreatmentPlanListItemDto>>;


public class GetTreatmentPlansHandler : IRequestHandler<GetTreatmentPlansQuery, GetTreatmentPlansResult>
{
    private readonly UserManager<User> _userManager;
    private readonly ApplicationDbContext _db;

    public GetTreatmentPlansHandler(UserManager<User> userManager, ApplicationDbContext db)
    {
        _userManager = userManager;
        _db = db;
    }

    public async Task<GetTreatmentPlansResult> Handle(GetTreatmentPlansQuery request, CancellationToken ct)
    {
        //TODO: validation

        string? effectiveUserId = request.CurrentUserId;

        if (string.IsNullOrWhiteSpace(effectiveUserId) && !string.IsNullOrWhiteSpace(request.UserEmail))
        {
            var user = await _userManager.FindByEmailAsync(request.UserEmail.Trim());
            effectiveUserId = user?.Id;
        }

        var query = _db.TreatmentPlans
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.SearchTxt))
        {
            var pattern = $"%{request.SearchTxt.Trim()}%";
            query = query.Where(r =>
                EF.Functions.Like(r.AdviceFullText, pattern));
        }

        if(request.Status is not null)
        {
            query = query.Where(tp => tp.Status == request.Status);
        }

        if (!string.IsNullOrWhiteSpace(request.CurrentUserId))
        {
            query = query.Where(r => r.IdPharmacist == request.CurrentUserId || r.IdPatient == request.CurrentUserId);
        }

        var totalCount = await query.CountAsync(ct);

        var viewerId = effectiveUserId ?? string.Empty;

        var skip = (request.Page - 1) * request.PageSize;

        var rows = await query
            .OrderByDescending(tp =>
                request.ViewerParty == ConversationParty.Patient
                    ? tp.Review != null && tp.Review.UnreadForPatient
                    : tp.Review != null && tp.Review.UnreadForPharmacist
            )
            .ThenByDescending(tp => tp.DateCreated)
            .Skip(skip)
            .Take(request.PageSize)
            .Select(tp => new
            {
                tp.Id,
                tp.Number,
                tp.DateCreated,
                tp.DateStarted,
                tp.DateCompleted,
                tp.AdviceFullText,
                tp.Status,
                UnreadForPatient = tp.Review != null && tp.Review.UnreadForPatient,
                UnreadForPharmacist = tp.Review != null && tp.Review.UnreadForPharmacist
            })
            .ToListAsync(ct);

        var items = rows
            .Select(r =>
                 new TreatmentPlanListItemDto(
                    r.Id,
                    r.Number,
                    r.DateCreated,
                    r.DateStarted,
                    r.DateCompleted,
                    r.AdviceFullText,
                    TreatmentPlanStatusMapper.ToPolish(r.Status),
                    r.UnreadForPatient,
                    r.UnreadForPharmacist
                    ))
            .ToList();


        return new()
        {
            Value = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
