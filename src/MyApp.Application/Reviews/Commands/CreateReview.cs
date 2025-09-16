using MediatR;
using MyApp.Application.Abstractions;
using MyApp.Application.Common;
using MyApp.Domain;
using System.Security.Cryptography;

namespace MyApp.Application.Reviews.Commands;

public record CreateReviewCommand(int CurrentUserId, string Advice) : IRequest<Result<Review>>;

public class CreateReviewHandler(IReviewRepository repo) : IRequestHandler<CreateReviewCommand, Result<Review>>
{
    public async Task<Result<Review>> Handle(CreateReviewCommand request, CancellationToken ct)
    {
        string number = GenerateDigits();
        var review = Review.Create(request.CurrentUserId, request.Advice, number);
       
        await repo.CreateAsync(review, ct);
        return Result<Review>.Ok(review);
    }

    private static string GenerateDigits(int bytes = 16)
    {
        byte[] buffer = new byte[bytes];
        RandomNumberGenerator.Fill(buffer);
        var token = Convert.ToBase64String(buffer)
            .Replace("+", "")
            .Replace("/", "")
            .Replace("=", "") 
            [..bytes];
        return token;
    }
}