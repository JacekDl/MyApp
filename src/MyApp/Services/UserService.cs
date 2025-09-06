using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyApp.Data;
using MyApp.Models;
using System.ComponentModel.DataAnnotations;

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

    public async Task<OperationResult<User>> RegisterAsync(string email, string password, string role = "User")
    {
        var normalized = email.Trim();
        if (await _db.Users.AnyAsync(u => u.Email == normalized))
            return OperationResult<User>.Failure("Email is already registered.");


        var user = new User { Email = normalized, Role = role };
        user.PasswordHash = _hasher.HashPassword(user, password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return OperationResult<User>.Success(user);
    }

    public async Task<User?> ValidateCredentialsAsync(string email, string password)
    {
        var normalized = email.Trim().ToLower();
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

    public async Task<IReadOnlyList<UserListItem>> GetUsersAsync()
    {
        return await _db.Users
            .AsNoTracking()
            .OrderByDescending(u => u.CreatedUtc)
            .Select(u => new UserListItem
            {
                Id = u.Id,
                Email = u.Email,
                Role = u.Role,
                CreatedUtc = u.CreatedUtc
            })
            .ToListAsync();      
    }

    public async Task<OperationResult> RemoveUserAsync(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user is null)
        {
            return OperationResult.Failure("User not found.");
        }

        if (string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return OperationResult.Failure("Cannot remove an Admin user.");
        }

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();

        return OperationResult.Success();
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _db.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Id == id);
    }

    public async Task<OperationResult> UpdateProfileAsync(int userId, string? name, string? pharmacyName, string? pharmacyCity)
    {
        var user = await _db.Users.SingleOrDefaultAsync(u => u.Id == userId);
        if (user is null)
        {
            return OperationResult.Failure("User not found.");
        }

        name = string.IsNullOrWhiteSpace(name) ? null : name.Trim();
        pharmacyName = string.IsNullOrWhiteSpace(pharmacyName) ? null : pharmacyName.Trim();
        pharmacyCity = string.IsNullOrWhiteSpace(pharmacyCity) ? null : pharmacyCity.Trim();

        var changed = false;

        if (user.Name != name)
        {
            user.Name = name;
            changed = true;
        }

        if (user.PharmacyName != pharmacyName)
        {
            user.PharmacyName = pharmacyName;
            changed = true;
        }

        if(user.PharmacyCity != pharmacyCity)
        {
            user.PharmacyCity = pharmacyCity;
            changed = true;
        }

        if (!changed)
        {
            return OperationResult.Success();
        }

        await _db.SaveChangesAsync();
        return OperationResult.Success();
    }

    public async Task<OperationResult> UpdateEmailAsync(int userId, string newEmail, string currentPassword)
    {
        var user = await _db.Users.SingleOrDefaultAsync(u => u.Id == userId);
        if (user is null)
        {
            return OperationResult.Failure("User not found.");
        }

        newEmail = (newEmail ?? string.Empty).Trim();

        var emailAttr = new EmailAddressAttribute();
        if (!emailAttr.IsValid(newEmail))
        {
            return OperationResult.Failure("Please enter a valid email address.");
        }

        if (string.Equals(user.Email, newEmail, StringComparison.OrdinalIgnoreCase))
        {
            return OperationResult.Success();
        }

        var verify = _hasher.VerifyHashedPassword(user, user.PasswordHash, currentPassword ?? string.Empty);
        if (verify == PasswordVerificationResult.Failed)
        {
            return OperationResult.Failure("Current password is incorrect.");
        }

        var exists = await _db.Users
            .AsNoTracking()
            .AnyAsync(u => u.Id != userId && u.Email.ToLower() == newEmail.ToLower());

        if (exists)
        {
            return OperationResult.Failure("The new email is already in use.");
        }

        user.Email = newEmail;
        await _db.SaveChangesAsync();
        return OperationResult.Success();

    }

    public async Task<OperationResult> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        var user = await _db.Users.SingleOrDefaultAsync(u => u.Id == userId);
        if (user is null)
        {
            return OperationResult.Failure("User not found.");
        }

        var verify = _hasher.VerifyHashedPassword(user, user.PasswordHash, currentPassword ?? string.Empty);
        if (verify == PasswordVerificationResult.Failed)
        {
            return OperationResult.Failure("Current password is incorrect.");
        }

        user.PasswordHash = _hasher.HashPassword(user, newPassword);
        await _db.SaveChangesAsync();

        return OperationResult.Success();
    }
}