using FluentAssertions;
using GoodHamburger.Application.DTOs;
using GoodHamburger.Application.Services;
using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Enums;
using GoodHamburger.Domain.Exceptions;
using GoodHamburger.Domain.Interfaces;
using NSubstitute;

namespace GoodHamburger.Tests.Application;

public class OrderServiceValidationTests
{
    private readonly IOrderRepository _orderRepo = Substitute.For<IOrderRepository>();
    private readonly IMenuRepository _menuRepo = Substitute.For<IMenuRepository>();
    private readonly OrderService _sut;
    private static readonly Guid TestUserId = Guid.NewGuid();

    private static readonly MenuItem Sandwich = new("X Burger", 5.00m, MenuItemType.Sandwich);
    private static readonly MenuItem Side = new("Batata frita", 2.00m, MenuItemType.SideDish);
    private static readonly MenuItem Beverage = new("Refrigerante", 2.50m, MenuItemType.Beverage);

    public OrderServiceValidationTests()
    {
        _sut = new OrderService(_orderRepo, _menuRepo);
    }

    [Fact]
    public async Task CreateAsync_WithEmptyItems_ShouldThrowDomainException()
    {
        var request = new CreateOrderRequest([]);

        await _sut.Invoking(s => s.CreateAsync(request, TestUserId))
            .Should().ThrowAsync<DomainException>()
            .WithMessage("*ao menos um item*");
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateIds_ShouldThrowDomainException()
    {
        var id = Sandwich.Id;
        var request = new CreateOrderRequest([id, id]);

        await _sut.Invoking(s => s.CreateAsync(request, TestUserId))
            .Should().ThrowAsync<DomainException>()
            .WithMessage("*duplicado*");
    }

    [Fact]
    public async Task CreateAsync_WithTwoSandwiches_ShouldThrowDomainException()
    {
        var sandwich2 = new MenuItem("X Bacon", 7.00m, MenuItemType.Sandwich);
        var ids = new List<Guid> { Sandwich.Id, sandwich2.Id };

        _menuRepo.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>())
            .Returns([Sandwich, sandwich2]);

        var request = new CreateOrderRequest(ids);

        await _sut.Invoking(s => s.CreateAsync(request, TestUserId))
            .Should().ThrowAsync<DomainException>()
            .WithMessage("*apenas um sanduíche*");
    }

    [Fact]
    public async Task CreateAsync_WithTwoBeverages_ShouldThrowDomainException()
    {
        var beverage2 = new MenuItem("Suco", 3.00m, MenuItemType.Beverage);
        var ids = new List<Guid> { Beverage.Id, beverage2.Id };

        _menuRepo.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>())
            .Returns([Beverage, beverage2]);

        var request = new CreateOrderRequest(ids);

        await _sut.Invoking(s => s.CreateAsync(request, TestUserId))
            .Should().ThrowAsync<DomainException>()
            .WithMessage("*apenas um refrigerante*");
    }

    [Fact]
    public async Task CreateAsync_WithTwoSides_ShouldThrowDomainException()
    {
        var side2 = new MenuItem("Onion Rings", 3.00m, MenuItemType.SideDish);
        var ids = new List<Guid> { Side.Id, side2.Id };

        _menuRepo.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>())
            .Returns([Side, side2]);

        var request = new CreateOrderRequest(ids);

        await _sut.Invoking(s => s.CreateAsync(request, TestUserId))
            .Should().ThrowAsync<DomainException>()
            .WithMessage("*apenas uma batata*");
    }

    [Fact]
    public async Task CreateAsync_WithValidItems_ShouldCallAddAsync()
    {
        var ids = new List<Guid> { Sandwich.Id, Side.Id };
        _menuRepo.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>())
            .Returns([Sandwich, Side]);

        var savedOrder = BuildOrder(Sandwich, Side);
        _orderRepo.AddAsync(Arg.Any<Order>()).Returns(Task.CompletedTask);
        _orderRepo.GetByIdAsync(Arg.Any<Guid>()).Returns(savedOrder);

        var request = new CreateOrderRequest(ids);
        var result = await _sut.CreateAsync(request, TestUserId);

        await _orderRepo.Received(1).AddAsync(Arg.Any<Order>());
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ShouldThrowDomainException()
    {
        _orderRepo.GetByIdAsync(Arg.Any<Guid>()).Returns((Order?)null);

        await _sut.Invoking(s => s.GetByIdAsync(Guid.NewGuid(), TestUserId))
            .Should().ThrowAsync<DomainException>()
            .WithMessage("*não encontrado*");
    }

    private static Order BuildOrder(params MenuItem[] menuItems)
    {
        var order = new Order(TestUserId);
        foreach (var mi in menuItems)
        {
            var item = new OrderItem(mi.Id, mi.Price);
            typeof(OrderItem)
                .GetProperty(nameof(OrderItem.MenuItem))!
                .SetValue(item, mi);
            order.AddItem(item);
        }
        return order;
    }
}
