using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GoodHamburger.Application.DTOs;
using GoodHamburger.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GoodHamburger.Tests.Integration;

/// <summary>
/// Factory customizada que substitui a connection string do SQLite por uma DB in-memory
/// usando o mesmo provider (Sqlite), evitando conflito de providers no EF Core 9.
/// </summary>
public class GoodHamburgerApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection;

    public GoodHamburgerApiFactory()
    {
        // Mantém a conexão aberta para que o SQLite in-memory persista durante os testes
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // Fornece as configurações obrigatórias
        builder.UseSetting("Jwt:Secret", "TestSecret-Min32Chars-ForIntegrationTests!!");
        builder.UseSetting("Jwt:Issuer", "GoodHamburger");
        builder.UseSetting("Jwt:Audience", "GoodHamburger");
        // Substitui a connection string pelo SQLite in-memory
        builder.UseSetting("ConnectionStrings:DefaultConnection", "DataSource=:memory:");

        builder.ConfigureServices(services =>
        {
            // Remover o DbContext registrado com SQLite de arquivo
            var descriptors = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>))
                .ToList();
            foreach (var d in descriptors) services.Remove(d);

            // Re-registrar usando a conexão SQLite in-memory persistente
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(_connection));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing) _connection.Dispose();
    }
}

public class AuthIntegrationTests(GoodHamburgerApiFactory factory)
    : IClassFixture<GoodHamburgerApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Register_WithValidData_ShouldReturn201WithToken()
    {
        var request = new RegisterRequest("Teste User", $"user_{Guid.NewGuid():N}@test.com", "senha123");

        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        result!.Token.Should().NotBeNullOrEmpty();
        result.Name.Should().Be("Teste User");
    }

    [Fact]
    public async Task Register_WithInvalidEmail_ShouldReturn400()
    {
        var request = new { Name = "Test", Email = "not-an-email", Password = "senha123" };

        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithShortPassword_ShouldReturn400()
    {
        var request = new { Name = "Test", Email = "test@test.com", Password = "12345" };

        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithUnknownEmail_ShouldReturn400()
    {
        var request = new LoginRequest("naoexiste@test.com", "qualquer");

        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetOrders_WithoutToken_ShouldReturn401()
    {
        var response = await _client.GetAsync("/api/orders");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMenu_WithoutToken_ShouldReturn200()
    {
        var response = await _client.GetAsync("/api/menu");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task HealthCheck_ShouldReturn200()
    {
        var response = await _client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
