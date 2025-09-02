using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyApp.Data;
using MyApp.Models;

namespace MyApp.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _db;
    private readonly IPasswordHasher<User> _hasher;

    public UserService(ApplicationDbContext db, IPasswordHasher<User> hasher)
    {
        _db = db;
        _hasher = hasher;
    }


    public async Task<(bool Ok, string? Error, User? User)> RegisterAsync(string email, string password, string role = "User")
    {
        var normalized = email.Trim();
        if (await _db.Users.AnyAsync(u => u.Email == normalized))
            return (false, "Email is already registered", null);

        var user = new User { Email = normalized, Role = role };
        user.PasswordHash = _hasher.HashPassword(user, password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return (true, null, user);
    }

    public async Task<User?> ValidateCredentialsAsync(string email, string password)
    {
        var normalized = email.Trim();
        var user = await _db.Users.SingleOrDefaultAsync(u => u.Email == normalized);
        if (user is null) return null;

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);
        return result == PasswordVerificationResult.Failed ? null : user;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        var user = await _db.Users.SingleOrDefaultAsync(u => u.Email == email.Trim());
        return user;
    }

}
