using MediatR;
using Microsoft.AspNetCore.Identity;
using MyApp.Application.Abstractions;
using MyApp.Application.Common;
using MyApp.Application.Data;
using MyApp.Domain;
using System.Security.Cryptography;

namespace MyApp.Application.Reviews.Commands;

public record CreateReviewCommand(string UserId, string Advice) : IRequest<Result<Review>>;

public class CreateReviewHandler : IRequestHandler<CreateReviewCommand, Result<Review>>
{
    private readonly UserManager<User> _userManager;
    private readonly ApplicationDbContext _db;

    public CreateReviewHandler(UserManager<User> userManager, ApplicationDbContext db)
    {
        _userManager = userManager;
        _db = db;
    }

    public async Task<Result<Review>> Handle(CreateReviewCommand request, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user is null)
            return Result<Review>.Fail("User not found.");

        string number = GenerateDigits();
        var review = Review.Create(request.UserId, request.Advice, number);

        _db.Add(review);
        await _db.SaveChangesAsync(ct);
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