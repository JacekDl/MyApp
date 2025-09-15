using MediatR;
using MyApp.Application.Abstractions;
using MyApp.Application.Common;
using MyApp.Domain;
using System.Security.Cryptography;

namespace MyApp.Application.Reviews.Commands;

public record CreateReviewCommand(int currentUserId, string Advice) : IRequest<Result<Review>>;

public class CreateReviewHandler : IRequestHandler<CreateReviewCommand, Result<Review>>
{
    private readonly IReviewRepository _repo;
    public CreateReviewHandler(IReviewRepository repo)
    {
        _repo = repo;
    }
    public async Task<Result<Review>> Handle(CreateReviewCommand request, CancellationToken ct)
    {
        string number = GenerateDigits(20);
        var review = Review.Create(request.currentUserId, request.Advice, number);
       
        await _repo.CreateAsync(review, ct);
        return Result<Review>.Ok(review);
    }

    private static string GenerateDigits(int digits)
    {
        var chars = new char[digits];
        for (int i = 0; i < digits; i++)
            chars[i] = (char)('0' + RandomNumberGenerator.GetInt32(0, 10));
        return new string(chars);
    }
}