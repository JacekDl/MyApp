using MyApp.Models;

namespace MyApp.Services;

public interface IUserService
{
    Task<(bool Ok, string? Error, User? User)> RegisterAsync(string email, string password, string role = "User");
    Task<User?> ValidateCredentialsAsync(string email, string password);
    Task<User?> GetByEmailAsync(string email);
}
