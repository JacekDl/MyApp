using MyApp.Models;

namespace MyApp.Services;

public interface IUserService
{
    Task<OperationResult<User>> RegisterAsync(string email, string password, string role = "User");

    Task<User?> ValidateCredentialsAsync(string email, string password);

    Task<User?> GetByEmailAsync(string email);

    Task<IReadOnlyList<UserListItem>> GetUsersAsync();

    Task<OperationResult> RemoveUserAsync(int id);
    Task<User?> GetByIdAsync(int currentUserId);
    Task<OperationResult> UpdateProfileAsync(int currentUserId, string? name, string? pharmacyName, string? pharmacyCity);
    Task<OperationResult> UpdateEmailAsync(int userId, string newEmail, string currentPassword);
    Task<OperationResult> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
}