using GoodHamburger.Domain.Entities;

namespace GoodHamburger.Application.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
}
