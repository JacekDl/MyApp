using MyApp.Domain;

namespace MyApp.Application.Abstractions;

public interface IUserRepository
{
    void CreateUser(User user, CancellationToken ct);
    Task<List<User>> GetAllAsync(CancellationToken ct);
    Task<User?> GetByEmailAsync(string normalized, CancellationToken ct);
    Task<User?> GetByIdAsync(string id, CancellationToken ct);
    void RemoveAsync(User user, CancellationToken ct);
    void UpdateUser(User user, CancellationToken ct);
}
