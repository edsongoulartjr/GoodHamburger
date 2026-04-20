using GoodHamburger.Application.DTOs;
using GoodHamburger.Application.Interfaces;
using GoodHamburger.Domain.Interfaces;

namespace GoodHamburger.Application.Services;

public class MenuService(IMenuRepository menuRepository) : IMenuService
{
    public async Task<IEnumerable<MenuItemDto>> GetMenuAsync()
    {
        var items = await menuRepository.GetAllAsync();
        return items.Select(m => new MenuItemDto(
            m.Id,
            m.Name,
            m.Price,
            m.Type,
            m.Type.ToString()
        ));
    }
}
