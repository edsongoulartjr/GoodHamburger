using FluentAssertions;
using GoodHamburger.Application.DTOs;
using GoodHamburger.Application.Interfaces;
using GoodHamburger.Application.Services;
using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Exceptions;
using GoodHamburger.Domain.Interfaces;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace GoodHamburger.Tests.Application;

public class AuthServiceTests
{
    private readonly IUserRepository _userRepo = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly ITokenService _tokenService = Substitute.For<ITokenService>();
    private readonly IAuthService _sut;

    public AuthServiceTests()
    {
        _sut = new AuthService(_userRepo, _hasher, _tokenService);
    }

    // ── Register ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task RegisterAsync_WithNewEmail_ShouldReturnAuthResponse()
    {
        _userRepo.ExistsByEmailAsync(Arg.Any<string>()).Returns(false);
        _hasher.Hash(Arg.Any<string>()).Returns("hashed");
        _tokenService.GenerateToken(Arg.Any<User>()).Returns("jwt-token");

        var result = await _sut.RegisterAsync(new RegisterRequest("João", "joao@test.com", "senha123"));

        result.Token.Should().Be("jwt-token");
        result.Name.Should().Be("João");
        await _userRepo.Received(1).AddAsync(Arg.Any<User>());
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ShouldThrowDomainException()
    {
        _userRepo.ExistsByEmailAsync(Arg.Any<string>()).Returns(true);

        await _sut.Invoking(s => s.RegisterAsync(new RegisterRequest("João", "joao@test.com", "senha123")))
            .Should().ThrowAsync<DomainException>()
            .WithMessage("*e-mail*");
    }

    // ── Login ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnAuthResponse()
    {
        var user = new User("Maria", "maria@test.com", "hashed");
        _userRepo.GetByEmailAsync("maria@test.com").Returns(user);
        _hasher.Verify("senha123", "hashed").Returns(true);
        _tokenService.GenerateToken(user).Returns("jwt-token");

        var result = await _sut.LoginAsync(new LoginRequest("maria@test.com", "senha123"));

        result.Token.Should().Be("jwt-token");
        result.Email.Should().Be("maria@test.com");
    }

    [Fact]
    public async Task LoginAsync_WithUnknownEmail_ShouldThrowDomainException()
    {
        _userRepo.GetByEmailAsync(Arg.Any<string>()).ReturnsNull();

        await _sut.Invoking(s => s.LoginAsync(new LoginRequest("naoexiste@test.com", "qualquer")))
            .Should().ThrowAsync<DomainException>()
            .WithMessage("*inválidos*");
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ShouldThrowDomainException()
    {
        var user = new User("Maria", "maria@test.com", "hashed");
        _userRepo.GetByEmailAsync("maria@test.com").Returns(user);
        _hasher.Verify("errada", "hashed").Returns(false);

        await _sut.Invoking(s => s.LoginAsync(new LoginRequest("maria@test.com", "errada")))
            .Should().ThrowAsync<DomainException>()
            .WithMessage("*inválidos*");
    }
}
