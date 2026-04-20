using GoodHamburger.Domain.Enums;

namespace GoodHamburger.Application.DTOs;

public record MenuItemDto(
    Guid Id,
    string Name,
    decimal Price,
    MenuItemType Type,
    string TypeDescription
);
