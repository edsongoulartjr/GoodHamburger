using GoodHamburger.Application.DTOs;
using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Enums;
using GoodHamburger.Domain.Exceptions;
using GoodHamburger.Domain.Interfaces;

namespace GoodHamburger.Application.Services;

public class OrderService(IOrderRepository orderRepository, IMenuRepository menuRepository)
{
    public async Task<IEnumerable<OrderDto>> GetAllByUserAsync(Guid userId)
    {
        var orders = await orderRepository.GetAllByUserAsync(userId);
        return orders.Select(MapToDto);
    }

    public async Task<OrderDto> GetByIdAsync(Guid id, Guid userId)
    {
        var order = await orderRepository.GetByIdAsync(id)
            ?? throw new DomainException($"Pedido '{id}' não encontrado.");
        if (order.UserId != userId)
            throw new DomainException($"Pedido '{id}' não encontrado.");
        return MapToDto(order);
    }

    public async Task<OrderDto> CreateAsync(CreateOrderRequest request, Guid userId)
    {
        var menuItems = await ValidateAndGetMenuItems(request.MenuItemIds);

        var order = new Order(userId);
        foreach (var menuItem in menuItems)
        {
            var item = new OrderItem(menuItem.Id, menuItem.Price);
            order.AddItem(item);
        }

        await orderRepository.AddAsync(order);

        var saved = await orderRepository.GetByIdAsync(order.Id);
        return MapToDto(saved!);
    }

    public async Task<OrderDto> UpdateAsync(Guid id, UpdateOrderRequest request, Guid userId)
    {
        var order = await orderRepository.GetByIdAsync(id)
            ?? throw new DomainException($"Pedido '{id}' não encontrado.");
        if (order.UserId != userId)
            throw new DomainException($"Pedido '{id}' não encontrado.");

        var menuItems = await ValidateAndGetMenuItems(request.MenuItemIds);

        var newItems = menuItems.Select(mi => new OrderItem(mi.Id, mi.Price)).ToList();
        order.ReplaceItems(newItems);

        await orderRepository.UpdateAsync(order);

        var updated = await orderRepository.GetByIdAsync(id);
        return MapToDto(updated!);
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        var order = await orderRepository.GetByIdAsync(id)
            ?? throw new DomainException($"Pedido '{id}' não encontrado.");
        if (order.UserId != userId)
            throw new DomainException($"Pedido '{id}' não encontrado.");
        await orderRepository.DeleteAsync(order.Id);
    }

    private async Task<List<MenuItem>> ValidateAndGetMenuItems(IList<Guid> menuItemIds)
    {
        if (menuItemIds == null || menuItemIds.Count == 0)
            throw new DomainException("Pedido inválido: informe ao menos um item do cardápio.");

        // Duplicate check
        var duplicates = menuItemIds.GroupBy(x => x).Where(g => g.Count() > 1).ToList();
        if (duplicates.Any())
            throw new DomainException("Pedido inválido: o mesmo item foi informado mais de uma vez. Itens duplicados não são permitidos.");

        var menuItems = (await menuRepository.GetByIdsAsync(menuItemIds)).ToList();

        var notFound = menuItemIds.Except(menuItems.Select(m => m.Id)).ToList();
        if (notFound.Any())
            throw new DomainException($"Pedido inválido: os seguintes itens não foram encontrados no cardápio: {string.Join(", ", notFound)}.");

        // Validate category uniqueness
        var sandwiches = menuItems.Where(m => m.Type == MenuItemType.Sandwich).ToList();
        var sides = menuItems.Where(m => m.Type == MenuItemType.SideDish).ToList();
        var beverages = menuItems.Where(m => m.Type == MenuItemType.Beverage).ToList();

        if (sandwiches.Count > 1)
            throw new DomainException("Pedido inválido: é permitido apenas um sanduíche por pedido. Itens duplicados não são aceitos.");
        if (sides.Count > 1)
            throw new DomainException("Pedido inválido: é permitido apenas uma batata frita por pedido. Itens duplicados não são aceitos.");
        if (beverages.Count > 1)
            throw new DomainException("Pedido inválido: é permitido apenas um refrigerante por pedido. Itens duplicados não são aceitos.");

        return menuItems;
    }

    private static OrderDto MapToDto(Order order)
    {
        order.RefreshDiscount();
        return new OrderDto(
            order.Id,
            order.CreatedAt,
            order.UpdatedAt,
            order.Items.Select(i => new OrderItemDto(
                i.MenuItemId,
                i.MenuItem?.Name ?? string.Empty,
                i.UnitPrice
            )).ToList().AsReadOnly(),
            order.Subtotal,
            order.DiscountPercentage,
            order.Discount,
            order.Total
        );
    }
}
