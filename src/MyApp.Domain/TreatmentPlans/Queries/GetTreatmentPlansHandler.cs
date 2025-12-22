using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Data;
using MyApp.Model;

namespace MyApp.Domain.TreatmentPlans.Queries;

public record class GetTreatmentPlansQuery(
    string? SearchTxt,
    string? CurrentUserId,
    bool? Completed,
    string? UserEmail = null,
    int Page = 1,
    int PageSize = 10
    ) : IRequest<GetTreatmentPlansResult>;
public record class GetTreatmentPlansResult : PagedResult<List<TreatmentPlanDto>>;


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

        if (!string.IsNullOrWhiteSpace(request.CurrentUserId))
        {
            query = query.Where(r => r.IdPharmacist == request.CurrentUserId || r.IdPatient == request.CurrentUserId);
        }

        var totalCount = await query.CountAsync(ct);

        var viewerId = effectiveUserId ?? string.Empty;

        var skip = (request.Page - 1) * request.PageSize;

        var rows = await query
            .Select(r => new
            {
                r.Id,
                r.Number,
                r.IdPharmacist,
                r.IdPatient,
                r.DateCreated,
                r.AdviceFullText,
                r.Claimed

            })
            .OrderByDescending(x => x.DateCreated)
            .Skip(skip)
            .Take(request.PageSize)
            .ToListAsync(ct);

        var items = rows
            .Select(r =>
                 new TreatmentPlanDto(
                    r.Id,
                    r.Number,
                    r.DateCreated,
                    r.IdPharmacist ?? "",
                    r.IdPatient ?? "",
                    r.AdviceFullText,
                    r.Claimed
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
