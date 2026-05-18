using ProductAPI.Application.DTOs.Auth;

namespace ProductAPI.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto dto, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task RevokeTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
}