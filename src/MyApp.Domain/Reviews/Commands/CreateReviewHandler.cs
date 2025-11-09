using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using MyApp.Domain.Common;
using MyApp.Domain.Data;
using MyApp.Model;
using System.Security.Cryptography;

namespace MyApp.Domain.Reviews.Commands;

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
        var user = await _userManager.FindByIdAsync(request.UserId); //user is Pharmacist
        if (user is null)
            return Result<Review>.Fail("Nie znaleziono użytkownika.");

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

public class CreateReviewCommandValidator : AbstractValidator<CreateReviewCommand>
{
    public CreateReviewCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId jest wymagany.");
        RuleFor(x => x.Advice)
            .NotEmpty().WithMessage("Zalecenia są wymagane.")
            .MaximumLength(500).WithMessage("Zalecania nie mogą być dłuższe niż 500 znaków.");
    }
}