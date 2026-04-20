using GoodHamburger.Application.DTOs;
using GoodHamburger.Application.Interfaces;
using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Exceptions;
using GoodHamburger.Domain.Interfaces;

namespace GoodHamburger.Application.Services;

public class AuthService(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    ITokenService tokenService)
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new DomainException("Nome é obrigatório.");
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new DomainException("E-mail é obrigatório.");
        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
            throw new DomainException("Senha deve ter no mínimo 6 caracteres.");

        if (await userRepository.ExistsByEmailAsync(request.Email))
            throw new DomainException("Já existe um usuário cadastrado com este e-mail.");

        var hash = passwordHasher.Hash(request.Password);
        var user = new User(request.Name, request.Email, hash);
        await userRepository.AddAsync(user);

        var token = tokenService.GenerateToken(user);
        return new AuthResponse(token, user.Name, user.Email);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            throw new DomainException("E-mail e senha são obrigatórios.");

        var user = await userRepository.GetByEmailAsync(request.Email.ToLowerInvariant())
            ?? throw new DomainException("E-mail ou senha inválidos.");

        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new DomainException("E-mail ou senha inválidos.");

        var token = tokenService.GenerateToken(user);
        return new AuthResponse(token, user.Name, user.Email);
    }
}
