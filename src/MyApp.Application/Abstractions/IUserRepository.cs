using MyApp.Domain;

namespace MyApp.Application.Abstractions;

public interface IUserRepository
{
    Task<List<User>> GetAllAsync(CancellationToken ct);
}
