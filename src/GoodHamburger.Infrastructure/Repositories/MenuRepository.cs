using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Interfaces;
using GoodHamburger.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GoodHamburger.Infrastructure.Repositories;

public class MenuRepository(AppDbContext db) : IMenuRepository
{
    public async Task<IEnumerable<MenuItem>> GetAllAsync() =>
        await db.MenuItems.AsNoTracking().ToListAsync();

    public async Task<MenuItem?> GetByIdAsync(Guid id) =>
        await db.MenuItems.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);

    public async Task<IEnumerable<MenuItem>> GetByIdsAsync(IEnumerable<Guid> ids) =>
        await db.MenuItems.AsNoTracking().Where(m => ids.Contains(m.Id)).ToListAsync();
}
