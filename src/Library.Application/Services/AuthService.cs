using Library.Application.DTOs;
using Library.Application.Exceptions;
using Library.Application.Interfaces;
using Library.Application.Mappers;
using Library.Domain.Entities;
using Library.Domain.Enums;

namespace Library.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenService _tokens;

    public AuthService(IUserRepository users, IUnitOfWork uow, IPasswordHasher hasher, ITokenService tokens)
    {
        _users = users;
        _uow = uow;
        _hasher = hasher;
        _tokens = tokens;
    }

    public async Task<UserDto> RegisterAsync(RegisterDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();

        if (await _users.EmailExistsAsync(email))
            throw new BusinessRuleException("This email is already registered.");

        var user = new User
        {
            FullName = dto.FullName.Trim(),
            Email = email,
            PasswordHash = _hasher.Hash(dto.Password),   // never store the plain password
            Role = UserRole.User                          // public registration always creates a normal User
        };
        // MIGRATION-2 (step 3 of 4): uncomment
        // user.PhoneNumber = dto.PhoneNumber;

        await _users.AddAsync(user);
        await _uow.SaveChangesAsync();
        return user.ToDto();
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _users.GetByEmailAsync(dto.Email.Trim().ToLowerInvariant());

        // same message for both cases so nobody can discover which emails exist
        if (user == null || !_hasher.Verify(dto.Password, user.PasswordHash))
            throw new AuthenticationFailedException("Invalid email or password.");

        var (token, expires) = _tokens.CreateToken(user);
        return new AuthResponseDto { Token = token, ExpiresAt = expires, User = user.ToDto() };
    }
}
