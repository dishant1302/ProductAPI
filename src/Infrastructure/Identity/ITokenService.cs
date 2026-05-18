namespace ProductAPI.Infrastructure.Identity;

public interface ITokenService
{
    string GenerateAccessToken(ApplicationUser user);
    string GenerateRefreshToken();
    int AccessTokenExpirationMinutes { get; }
    int RefreshTokenExpirationDays { get; }
}