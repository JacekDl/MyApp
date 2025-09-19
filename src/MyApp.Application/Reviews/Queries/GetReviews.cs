using MediatR;
using Microsoft.AspNetCore.Identity;
using MyApp.Application.Abstractions;
using MyApp.Domain;

namespace MyApp.Application.Reviews.Queries;

public record GetReviewsQuery(string? searchTxt, string? userId, bool? completed, string? userEmail = null) : IRequest<List<ReviewDto>>;

public class GetReviewsHandler : IRequestHandler<GetReviewsQuery, List<ReviewDto>>
{
    private readonly IReviewRepository _repo;
    private readonly UserManager<User> _userManager;

    public GetReviewsHandler(UserManager<User> userManager, IReviewRepository repo)
    {
        _repo = repo;
        _userManager = userManager;
    }
    public async Task<List<ReviewDto>> Handle(GetReviewsQuery request, CancellationToken ct)
    {
        string? effectiveUserId = request.userId;

        if(string.IsNullOrWhiteSpace(effectiveUserId) && !string.IsNullOrWhiteSpace(request.userEmail))
        {
            var user = await _userManager.FindByEmailAsync(request.userEmail.Trim());
            effectiveUserId = user?.Id;
        }
        
        var list = await _repo.GetReviews(request.searchTxt, request.userId, request.completed, ct);

        return list
            .Select(r => new ReviewDto(
                r.Id,
                r.CreatedByUserId,
                r.Number,
                r.DateCreated,
                r.Advice,
                r.Response ?? string.Empty,
                r.Completed))
            .ToList();
    }
}
