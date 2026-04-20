using GoodHamburger.Application.Interfaces;
using GoodHamburger.Application.Services;
using GoodHamburger.Domain.Interfaces;
using GoodHamburger.Infrastructure.Data;
using GoodHamburger.Infrastructure.Repositories;
using GoodHamburger.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GoodHamburger.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registra a infraestrutura. O provider é selecionado via "Database:Provider" no appsettings.
    /// Valores aceitos: "Sqlite" (padrão), "SqlServer", "PostgreSQL".
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString,
        string provider = "Sqlite")
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            _ = provider.ToLowerInvariant() switch
            {
                "sqlserver" => options.UseSqlServer(connectionString, sql =>
                    sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)),

                "postgresql" or "postgres" => options.UseNpgsql(connectionString, npg =>
                    npg.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)),

                _ => options.UseSqlite(connectionString, sqlite =>
                    sqlite.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName))
            };
        });

        // Repositories
        services.AddScoped<IMenuRepository, MenuRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        // Security
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<ITokenService, JwtTokenService>();

        // Services
        services.AddScoped<IMenuService, MenuService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
