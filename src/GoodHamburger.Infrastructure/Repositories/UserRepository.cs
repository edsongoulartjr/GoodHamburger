using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Interfaces;
using GoodHamburger.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GoodHamburger.Infrastructure.Repositories;

public class UserRepository(AppDbContext db) : IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email) =>
        await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant());

    public async Task<User?> GetByIdAsync(Guid id) =>
        await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);

    public async Task AddAsync(User user)
    {
        db.Users.Add(user);
        await db.SaveChangesAsync();
    }

    public async Task<bool> ExistsByEmailAsync(string email) =>
        await db.Users.AnyAsync(u => u.Email == email.ToLowerInvariant());
}
