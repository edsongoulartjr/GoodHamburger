using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GoodHamburger.Application.Interfaces;
using GoodHamburger.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace GoodHamburger.Infrastructure.Security;

public class JwtTokenService(IConfiguration configuration) : ITokenService
{
    public string GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]
                ?? throw new InvalidOperationException("JWT Secret not configured.")));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email)
        };

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddHours(
            double.Parse(configuration["Jwt:ExpiresInHours"] ?? "8"));

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
