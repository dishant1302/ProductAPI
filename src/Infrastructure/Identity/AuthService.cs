using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProductAPI.Application.DTOs.Auth;
using ProductAPI.Application.Interfaces;
using ProductAPI.Domain.Exceptions;
using ProductAPI.Infrastructure.Data;

namespace ProductAPI.Infrastructure.Identity;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly ApplicationDbContext _context;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _context = context;
    }

    public async Task<AuthResponseDto> RegisterAsync(
        RegisterRequestDto dto, CancellationToken cancellationToken = default)
    {
        var existing = await _userManager.FindByEmailAsync(dto.Email);
        if (existing is not null)
            throw new BadRequestException("A user with this email already exists.");

        var user = new ApplicationUser
        {
            Email = dto.Email,
            UserName = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            throw new BadRequestException(
                string.Join(" ", result.Errors.Select(e => e.Description)));

        return await BuildAuthResponseAsync(user, cancellationToken);
    }

    public async Task<AuthResponseDto> LoginAsync(
        LoginRequestDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            throw new UnauthorizedException("Invalid email or password.");

        return await BuildAuthResponseAsync(user, cancellationToken);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(
        string refreshToken, CancellationToken cancellationToken = default)
    {
        var stored = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken, cancellationToken);

        if (stored is null || stored.IsRevoked || stored.ExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedException("Invalid or expired refresh token.");

        // Rotate: revoke the old token
        stored.IsRevoked = true;
        await _context.SaveChangesAsync(cancellationToken);

        return await BuildAuthResponseAsync(stored.User, cancellationToken);
    }

    public async Task RevokeTokenAsync(
        string refreshToken, CancellationToken cancellationToken = default)
    {
        var stored = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken, cancellationToken)
            ?? throw new NotFoundException("Refresh token not found.");

        stored.IsRevoked = true;
        await _context.SaveChangesAsync(cancellationToken);
    }

    // ──────────────────────────────────────────────────────────────────
    private async Task<AuthResponseDto> BuildAuthResponseAsync(
        ApplicationUser user, CancellationToken cancellationToken)
    {
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        _context.RefreshTokens.Add(new RefreshToken
        {
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(_tokenService.RefreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false,
            UserId = user.Id
        });
        await _context.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_tokenService.AccessTokenExpirationMinutes),
            Email = user.Email!,
            FullName = $"{user.FirstName} {user.LastName}"
        };
    }
}