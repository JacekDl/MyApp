﻿using MediatR;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Common;
using MyApp.Domain.Data;

namespace MyApp.Domain.Reviews.Commands
{
    public record DeleteReviewCommand(int Id) : IRequest<Result<bool>>;

    public class DeleteReviewHandler(ApplicationDbContext db) : IRequestHandler<DeleteReviewCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(DeleteReviewCommand request, CancellationToken ct)
        {
            var review = await db.Reviews.FirstOrDefaultAsync(r => r.Id == request.Id, ct);
            if (review is null)
                return Result<bool>.Fail("Review not found.");

            db.Reviews.Remove(review);

            try
            {
                await db.SaveChangesAsync(ct);
                return Result<bool>.Ok(true);
            }
            catch (DbUpdateException ex)
            {
                return Result<bool>.Fail($"Could not delete review: {ex.InnerException?.Message ?? ex.Message}");
            }
        }
    }
}
