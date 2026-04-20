using GoodHamburger.Application.DTOs;

namespace GoodHamburger.Application.Interfaces;

public interface IMenuService
{
    Task<IEnumerable<MenuItemDto>> GetMenuAsync();
}
