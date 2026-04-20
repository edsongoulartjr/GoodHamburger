using GoodHamburger.Domain.Enums;

namespace GoodHamburger.Domain.Entities;

public class MenuItem
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public MenuItemType Type { get; private set; }

    private MenuItem() { } // EF

    public MenuItem(string name, decimal price, MenuItemType type)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));
        if (price <= 0)
            throw new ArgumentException("Price must be greater than zero.", nameof(price));

        Id = Guid.NewGuid();
        Name = name;
        Price = price;
        Type = type;
    }
}
