using MyApp.Domain;

namespace MyApp.Application.Abstractions;

public interface IUserRepository
{
    void CreateUser(User user, CancellationToken ct);
    Task<List<User>> GetAllAsync(CancellationToken ct);
    Task<User?> GetByEmailAsync(string normalized, CancellationToken ct);
    Task<User?> GetByIdAsync(int id, CancellationToken ct);
}
