using GoodHamburger.Application.DTOs;

namespace GoodHamburger.Application.Interfaces;


public interface IOrderService
{
    Task<PagedResult<OrderDto>> GetAllByUserAsync(Guid userId, int page, int pageSize);
    Task<OrderDto> GetByIdAsync(Guid id, Guid userId);
    Task<OrderDto> CreateAsync(CreateOrderRequest request, Guid userId);
    Task<OrderDto> UpdateAsync(Guid id, UpdateOrderRequest request, Guid userId);
    Task DeleteAsync(Guid id, Guid userId);
}
