using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using MyApp.Domain.Common;
using MyApp.Domain.Data;
using MyApp.Model;
using System.Security.Cryptography;

namespace MyApp.Domain.Reviews.Commands;

public record class CreateReviewCommand(string UserId, string Advice) : IRequest<CreateReviewResult>;

public record class CreateReviewResult : HResult<Review>
{
}

public class CreateReviewHandler : IRequestHandler<CreateReviewCommand, CreateReviewResult>
{
    private readonly UserManager<User> _userManager;
    private readonly ApplicationDbContext _db;

    public CreateReviewHandler(UserManager<User> userManager, ApplicationDbContext db)
    {
        _userManager = userManager;
        _db = db;
    }

    public async Task<CreateReviewResult> Handle(CreateReviewCommand request, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(request.UserId); //user is Pharmacist
        if (user is null)
            return new() { ErrorMessage="Nie znaleziono użytkownika." };

        string number = GenerateDigits();
        var review = Review.Create(request.UserId, request.Advice, number);

        _db.Add(review);
        await _db.SaveChangesAsync(ct);
        return new() { Value = review  };
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

public class CreateReviewValidator : AbstractValidator<CreateReviewCommand>
{
    public CreateReviewValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId jest wymagany.");
        RuleFor(x => x.Advice)
            .NotEmpty().WithMessage("Zalecenia są wymagane.")
            .MaximumLength(500).WithMessage("Zalecania nie mogą być dłuższe niż 500 znaków.");
    }
}