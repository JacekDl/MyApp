using MediatR;
using Microsoft.EntityFrameworkCore;
using MyApp.Application.Common;
using MyApp.Application.Data;

namespace MyApp.Application.Reviews.Commands;

public record UpdateReviewCommand(string Number, string ReviewText) : IRequest<Result<bool>>;

public class UpdateReviewHandler : IRequestHandler<UpdateReviewCommand, Result<bool>>
{
    private readonly ApplicationDbContext _db;
    public UpdateReviewHandler(ApplicationDbContext db)
    {
        _db = db;
    }
    public Task<Result<bool>> Handle(UpdateReviewCommand request, CancellationToken ct)
            => Task.FromResult(Result<bool>.Ok(true));
}
