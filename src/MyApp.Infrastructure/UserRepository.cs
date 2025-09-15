using Microsoft.EntityFrameworkCore;
using MyApp.Domain;
using MyApp.Application.Abstractions;

namespace MyApp.Infrastructure;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _db;

    public UserRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<User>> GetAllAsync(CancellationToken ct) =>
        await _db.Users
        .OrderByDescending(u => u.CreatedUtc)
        .ToListAsync(ct);

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct)
    {
        return await _db.Users
            .SingleOrDefaultAsync(u => u.Email == email);
    }

    public void CreateUser(User user, CancellationToken ct)
    {
        _db.Add(user);
        _db.SaveChangesAsync(ct);
    }
}
