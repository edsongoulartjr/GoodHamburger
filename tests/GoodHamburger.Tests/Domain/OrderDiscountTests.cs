using FluentAssertions;
using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Enums;

namespace GoodHamburger.Tests.Domain;

public class OrderDiscountTests
{
    private static MenuItem MakeSandwich(string name = "X Burger", decimal price = 5.00m) =>
        new(name, price, MenuItemType.Sandwich);

    private static MenuItem MakeSide(decimal price = 2.00m) =>
        new("Batata frita", price, MenuItemType.SideDish);

    private static MenuItem MakeBeverage(decimal price = 2.50m) =>
        new("Refrigerante", price, MenuItemType.Beverage);

    private static Order BuildOrderWith(params MenuItem[] menuItems)
    {
        var order = new Order(Guid.NewGuid());
        foreach (var mi in menuItems)
        {
            // Agora o construtor recebe MenuItemType diretamente — sem necessidade de reflection
            var item = new OrderItem(mi.Id, mi.Type, mi.Price);
            order.AddItem(item);
        }
        return order;
    }

    [Fact]
    public void Order_WithSandwichSideAndBeverage_ShouldApply20PercentDiscount()
    {
        var order = BuildOrderWith(MakeSandwich(), MakeSide(), MakeBeverage());

        order.DiscountPercentage.Should().Be(20m);
        order.Subtotal.Should().Be(9.50m);
        order.Discount.Should().Be(1.90m);
        order.Total.Should().Be(7.60m);
    }

    [Fact]
    public void Order_WithSandwichAndBeverage_ShouldApply15PercentDiscount()
    {
        var order = BuildOrderWith(MakeSandwich(), MakeBeverage());

        order.DiscountPercentage.Should().Be(15m);
        order.Subtotal.Should().Be(7.50m);
        order.Discount.Should().Be(1.12m); // Math.Round(1.125, 2) = 1.12 (banker's rounding)
        order.Total.Should().Be(6.38m);
    }

    [Fact]
    public void Order_WithSandwichAndSide_ShouldApply10PercentDiscount()
    {
        var order = BuildOrderWith(MakeSandwich(), MakeSide());

        order.DiscountPercentage.Should().Be(10m);
        order.Subtotal.Should().Be(7.00m);
        order.Discount.Should().Be(0.70m);
        order.Total.Should().Be(6.30m);
    }

    [Fact]
    public void Order_WithSandwichOnly_ShouldApplyNoDiscount()
    {
        var order = BuildOrderWith(MakeSandwich());

        order.DiscountPercentage.Should().Be(0m);
        order.Discount.Should().Be(0m);
        order.Total.Should().Be(order.Subtotal);
    }

    [Fact]
    public void Order_WithSideOnly_ShouldApplyNoDiscount()
    {
        var order = BuildOrderWith(MakeSide());

        order.DiscountPercentage.Should().Be(0m);
    }

    [Fact]
    public void Order_WithBeverageOnly_ShouldApplyNoDiscount()
    {
        var order = BuildOrderWith(MakeBeverage());

        order.DiscountPercentage.Should().Be(0m);
    }

    [Fact]
    public void Order_Total_ShouldBeSubtotalMinusDiscount()
    {
        var order = BuildOrderWith(MakeSandwich(price: 7.00m), MakeSide(), MakeBeverage());

        order.Total.Should().Be(order.Subtotal - order.Discount);
    }
}
