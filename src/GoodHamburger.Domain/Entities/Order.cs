using GoodHamburger.Domain.Enums;

namespace GoodHamburger.Domain.Entities;

public class Order
{
    private readonly List<OrderItem> _items = [];

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    public decimal Subtotal => _items.Sum(i => i.UnitPrice);
    public decimal DiscountPercentage { get; private set; }
    public decimal Discount => Math.Round(Subtotal * DiscountPercentage / 100, 2);
    public decimal Total => Subtotal - Discount;

    public Order() { } // EF

    public Order(Guid userId)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        CreatedAt = DateTime.UtcNow;
    }

    public void AddItem(OrderItem item)
    {
        item.SetOrderId(Id);
        _items.Add(item);
        RecalculateDiscount();
    }

    public void ReplaceItems(IEnumerable<OrderItem> newItems)
    {
        _items.Clear();
        foreach (var item in newItems)
        {
            item.SetOrderId(Id);
            _items.Add(item);
        }
        UpdatedAt = DateTime.UtcNow;
        RecalculateDiscount();
    }

    private void RecalculateDiscount()
    {
        bool hasSandwich = _items.Any(i => i.MenuItemType == MenuItemType.Sandwich);
        bool hasSide = _items.Any(i => i.MenuItemType == MenuItemType.SideDish);
        bool hasBeverage = _items.Any(i => i.MenuItemType == MenuItemType.Beverage);

        DiscountPercentage = (hasSandwich, hasSide, hasBeverage) switch
        {
            (true, true, true) => 20m,
            (true, false, true) => 15m,
            (true, true, false) => 10m,
            _ => 0m
        };
    }

    /// <summary>Called by Infrastructure after loading from DB with navigation properties.</summary>
    public void RefreshDiscount() => RecalculateDiscount();
}
