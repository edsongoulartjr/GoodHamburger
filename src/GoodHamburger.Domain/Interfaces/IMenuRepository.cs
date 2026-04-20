using GoodHamburger.Domain.Entities;

namespace GoodHamburger.Domain.Interfaces;

public interface IMenuRepository
{
    Task<IEnumerable<MenuItem>> GetAllAsync();
    Task<MenuItem?> GetByIdAsync(Guid id);
    Task<IEnumerable<MenuItem>> GetByIdsAsync(IEnumerable<Guid> ids);
}
