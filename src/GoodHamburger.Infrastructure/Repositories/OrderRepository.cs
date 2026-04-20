using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Interfaces;
using GoodHamburger.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GoodHamburger.Infrastructure.Repositories;

public class OrderRepository(AppDbContext db) : IOrderRepository
{
    private IQueryable<Order> OrdersWithItems =>
        db.Orders.Include(o => o.Items).ThenInclude(i => i.MenuItem);

    public async Task<IEnumerable<Order>> GetAllAsync() =>
        await OrdersWithItems.AsNoTracking().ToListAsync();

    public async Task<IEnumerable<Order>> GetAllByUserAsync(Guid userId) =>
        await OrdersWithItems.AsNoTracking().Where(o => o.UserId == userId).ToListAsync();

    public async Task<(IEnumerable<Order> Orders, int TotalCount)> GetAllByUserPagedAsync(Guid userId, int page, int pageSize)
    {
        var query = OrdersWithItems.AsNoTracking().Where(o => o.UserId == userId).OrderByDescending(o => o.CreatedAt);
        var total = await db.Orders.CountAsync(o => o.UserId == userId);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, total);
    }

    public async Task<Order?> GetByIdAsync(Guid id) =>
        await OrdersWithItems.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id);

    public async Task AddAsync(Order order)
    {
        db.Orders.Add(order);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Order order)
    {
        // Remove existing items and replace
        var existingItems = await db.OrderItems.Where(i => i.OrderId == order.Id).ToListAsync();
        db.OrderItems.RemoveRange(existingItems);

        db.Orders.Update(order);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var order = await db.Orders.FindAsync(id);
        if (order is not null)
        {
            db.Orders.Remove(order);
            await db.SaveChangesAsync();
        }
    }
}
