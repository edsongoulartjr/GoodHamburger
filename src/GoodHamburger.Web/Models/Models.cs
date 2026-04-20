using GoodHamburger.Domain.Enums;

namespace GoodHamburger.Web.Models;

public class MenuItemModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public MenuItemType Type { get; set; }
    public string TypeDescription { get; set; } = string.Empty;
}

public class OrderItemModel
{
    public Guid MenuItemId { get; set; }
    public string MenuItemName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
}

public class OrderModel
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<OrderItemModel> Items { get; set; } = [];
    public decimal Subtotal { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }
}

public class CreateOrderRequest
{
    public List<Guid> MenuItemIds { get; set; } = [];
}
