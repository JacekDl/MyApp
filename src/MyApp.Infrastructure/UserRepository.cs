using Microsoft.EntityFrameworkCore;
using MyApp.Domain;
using MyApp.Application.Abstractions;

namespace MyApp.Infrastructure;

public class UserRepository(ApplicationDbContext context) : IUserRepository
{
    public async Task<List<User>> GetAllAsync(CancellationToken ct) =>
        await context.Users
        .OrderByDescending(u => u.CreatedUtc)
        .ToListAsync(ct);
}
