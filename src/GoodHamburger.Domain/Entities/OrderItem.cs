using GoodHamburger.Domain.Enums;

namespace GoodHamburger.Domain.Entities;

public class OrderItem
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid MenuItemId { get; private set; }
    public MenuItemType MenuItemType { get; private set; }
    public MenuItem MenuItem { get; private set; } = null!;
    public decimal UnitPrice { get; private set; }

    private OrderItem() { } // EF

    public OrderItem(Guid menuItemId, MenuItemType menuItemType, decimal unitPrice)
    {
        Id = Guid.NewGuid();
        MenuItemId = menuItemId;
        MenuItemType = menuItemType;
        UnitPrice = unitPrice;
    }

    internal void SetOrderId(Guid orderId) => OrderId = orderId;
}
