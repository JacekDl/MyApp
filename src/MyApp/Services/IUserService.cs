using MyApp.Models;
using MyApp.Domain;

namespace MyApp.Services;

public interface IUserService
{
    Task<OperationResult<User>> RegisterAsync(string email, string password, string role = "User");

    Task<User?> ValidateCredentialsAsync(string email, string password);

    Task<User?> GetByEmailAsync(string email);

    Task<IReadOnlyList<UserListItem>> GetUsersAsync();

    Task<OperationResult> RemoveUserAsync(string id);
    Task<User?> GetByIdAsync(string currentUserId);
    Task<OperationResult> UpdateProfileAsync(string currentUserId, string? name, string? pharmacyName, string? pharmacyCity);
    Task<OperationResult> UpdateEmailAsync(string userId, string newEmail, string currentPassword);
    Task<OperationResult> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
}