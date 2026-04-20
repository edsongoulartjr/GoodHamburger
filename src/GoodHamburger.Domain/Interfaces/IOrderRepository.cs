using GoodHamburger.Domain.Entities;

namespace GoodHamburger.Domain.Interfaces;

public interface IOrderRepository
{
    Task<IEnumerable<Order>> GetAllAsync();
    Task<IEnumerable<Order>> GetAllByUserAsync(Guid userId);
    Task<(IEnumerable<Order> Orders, int TotalCount)> GetAllByUserPagedAsync(Guid userId, int page, int pageSize);
    Task<Order?> GetByIdAsync(Guid id);
    Task AddAsync(Order order);
    Task UpdateAsync(Order order);
    Task DeleteAsync(Guid id);
}
