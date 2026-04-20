namespace GoodHamburger.Application.DTOs;

public record OrderItemDto(
    Guid MenuItemId,
    string MenuItemName,
    decimal UnitPrice
);

public record OrderDto(
    Guid Id,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<OrderItemDto> Items,
    decimal Subtotal,
    decimal DiscountPercentage,
    decimal Discount,
    decimal Total
);

public record CreateOrderRequest(IList<Guid> MenuItemIds);

public record UpdateOrderRequest(IList<Guid> MenuItemIds);
