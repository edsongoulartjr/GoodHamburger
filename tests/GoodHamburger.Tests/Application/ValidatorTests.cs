using FluentAssertions;
using FluentValidation.TestHelper;
using GoodHamburger.Application.DTOs;
using GoodHamburger.Application.Validators;

namespace GoodHamburger.Tests.Application;

public class AuthValidatorTests
{
    private readonly RegisterRequestValidator _registerValidator = new();
    private readonly LoginRequestValidator _loginValidator = new();

    // ── RegisterRequest ───────────────────────────────────────────────────────

    [Fact]
    public void Register_ValidRequest_ShouldHaveNoErrors()
    {
        var result = _registerValidator.TestValidate(new RegisterRequest("João", "joao@test.com", "senha123"));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("", "joao@test.com", "senha123")]
    [InlineData("João", "", "senha123")]
    [InlineData("João", "email-invalido", "senha123")]
    [InlineData("João", "joao@test.com", "12345")]   // menos de 6 chars
    public void Register_InvalidRequest_ShouldHaveErrors(string name, string email, string password)
    {
        var result = _registerValidator.TestValidate(new RegisterRequest(name, email, password));
        result.Errors.Should().NotBeEmpty();
    }

    // ── LoginRequest ──────────────────────────────────────────────────────────

    [Fact]
    public void Login_ValidRequest_ShouldHaveNoErrors()
    {
        var result = _loginValidator.TestValidate(new LoginRequest("joao@test.com", "qualquer"));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("", "senha")]
    [InlineData("email-invalido", "senha")]
    [InlineData("joao@test.com", "")]
    public void Login_InvalidRequest_ShouldHaveErrors(string email, string password)
    {
        var result = _loginValidator.TestValidate(new LoginRequest(email, password));
        result.Errors.Should().NotBeEmpty();
    }
}

public class OrderValidatorTests
{
    private readonly CreateOrderRequestValidator _createValidator = new();
    private readonly UpdateOrderRequestValidator _updateValidator = new();

    [Fact]
    public void CreateOrder_WithValidItems_ShouldHaveNoErrors()
    {
        var result = _createValidator.TestValidate(new CreateOrderRequest([Guid.NewGuid()]));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateOrder_WithEmptyList_ShouldHaveError()
    {
        var result = _createValidator.TestValidate(new CreateOrderRequest([]));
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public void CreateOrder_WithDuplicateIds_ShouldHaveError()
    {
        var id = Guid.NewGuid();
        var result = _createValidator.TestValidate(new CreateOrderRequest([id, id]));
        result.Errors.Should().NotBeEmpty();
    }
}
